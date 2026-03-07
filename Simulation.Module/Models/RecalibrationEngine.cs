using Simulation.Module.Config;
using AgentCompany = Company.Module.Models.Company;
using AgentImporter = Company.Module.Models.Importer;
using AgentExporter = Company.Module.Models.Exporter;
using AgentGovernment = Government.Module.Models.Government;
using Company.Module.Models;

namespace Simulation.Module.Models;

/// <summary>
/// Moteur de recalibration mensuelle — logique GARDE-FOUS.
///
/// Les données observées (INSTAT, BCM, DGI) ne sont PAS des cibles à atteindre.
/// Elles définissent une bande de tolérance (±SeuilTolerance, défaut ±30%) autour
/// de la valeur de référence. La simulation est libre de diverger dans cette bande.
///
/// Correction uniquement si la dérive dépasse le seuil :
///   - On calcule l'excès au-delà du bord de la bande
///   - On corrige proportionnellement cet excès (pas l'écart total)
///   - On ramène vers le BORD de la bande, pas vers le centre
///
/// Cela permet aux scénarios (choc carburant, hausse SMIG…) de produire des
/// résultats qui divergent légitimement de l'historique, tout en empêchant
/// les dérives numériques incontrôlées.
///
/// Grandeurs surveillées :
///   1. M3 (masse monétaire)     → ajuste CroissanceCreditJour
///   2. Recettes fiscales         → ajuste ProductiviteParEmployeJour
///   3. Exports FOB               → ajuste ValeurFOBJour des exportateurs
///   4. Imports CIF               → ajuste ValeurCIFJour des importateurs
///   5. Devises tourisme          → ajuste productivité hôtellerie
///   6. Affiliés CNaPS            → formalisation d'entreprises (si dérive > seuil)
/// </summary>
public static class RecalibrationEngine
{
    /// <summary>
    /// Exécute la recalibration garde-fous pour un mois donné.
    /// Ne corrige QUE les grandeurs qui dérivent au-delà de la bande de tolérance.
    /// </summary>
    public static CalibrationEvent? Recalibrer(RecalibrationContext ctx)
    {
        double alpha = Math.Clamp(ctx.Config.VitesseConvergenceRecalibration, 0.05, 1.0);
        double seuil = Math.Clamp(ctx.Config.SeuilToleranceRecalibration, 0.05, 1.0);

        var evt = new CalibrationEvent
        {
            Jour = ctx.JourCourant,
            Mois = ctx.Mois,
        };

        // ═══════════════════════════════════════════
        //  1. MASSE MONÉTAIRE M3
        // ═══════════════════════════════════════════
        if (ctx.Cible.M3Cible.HasValue && ctx.Cible.M3Cible.Value > 0)
        {
            double m3Reference = ctx.Cible.M3Cible.Value * ctx.FacteurEchelle;
            double m3Simulee = ctx.SnapshotActuel.MasseMonetaireM3;

            if (m3Simulee > 0 && m3Reference > 0)
            {
                double ecart = (m3Simulee - m3Reference) / m3Reference;

                // Ne corriger QUE si l'écart dépasse la bande ±seuil
                if (Math.Abs(ecart) > seuil)
                {
                    // L'excès au-delà du bord de la bande
                    double exces = ecart > 0
                        ? ecart - seuil     // simulé trop haut → excès positif
                        : ecart + seuil;    // simulé trop bas → excès négatif

                    // Bord de la bande (là où on veut ramener)
                    double m3Bord = ecart > 0
                        ? m3Reference * (1.0 + seuil)
                        : m3Reference * (1.0 - seuil);

                    int joursRestants = Math.Max(30, ctx.Config.DureeJours - ctx.JourCourant);
                    double tauxCourant = ctx.Config.CroissanceCreditJour;

                    // Taux nécessaire pour ramener au bord (pas au centre)
                    double ratioBord = m3Bord / m3Simulee;
                    double tauxNecessaire = Math.Pow(ratioBord, 1.0 / joursRestants) - 1.0;
                    tauxNecessaire = Math.Clamp(tauxNecessaire, -0.005, 0.005);

                    // Correction partielle (α) de l'excès uniquement
                    double nouveauTaux = tauxCourant + alpha * (tauxNecessaire - tauxCourant);
                    nouveauTaux = Math.Clamp(nouveauTaux, 0, 0.005);

                    evt.Ajustements.Add(new CalibrationAdjustment
                    {
                        Parametre = "CroissanceCreditJour",
                        AncienneValeur = tauxCourant,
                        NouvelleValeur = nouveauTaux,
                        ValeurSimulee = m3Simulee / ctx.FacteurEchelle,
                        ValeurReference = ctx.Cible.M3Cible.Value,
                        EcartRelatif = ecart,
                        SeuilApplique = seuil,
                    });

                    ctx.Config.CroissanceCreditJour = nouveauTaux;
                }
            }
        }

        // ═══════════════════════════════════════════
        //  2. RECETTES FISCALES → ajuste la productivité (volume des ventes)
        // ═══════════════════════════════════════════
        if (ctx.Cible.RecettesFiscalesCumuleesCible.HasValue && ctx.Cible.RecettesFiscalesCumuleesCible.Value > 0)
        {
            double reference = ctx.Cible.RecettesFiscalesCumuleesCible.Value * ctx.FacteurEchelle;
            double simulee = ctx.Etat.TotalRecettesFiscales;

            if (simulee > 0 && reference > 0 && ctx.Entreprises.Count > 0)
            {
                double ecart = (simulee - reference) / reference;

                if (Math.Abs(ecart) > seuil)
                {
                    // Ramener vers le bord de la bande, pas le centre
                    double bordRatio = ecart > 0
                        ? reference * (1.0 + seuil) / simulee
                        : reference * (1.0 - seuil) / simulee;

                    double facteurCorrection = 1.0 + alpha * (bordRatio - 1.0);
                    facteurCorrection = Math.Clamp(facteurCorrection, 0.5, 1.5);

                    double productiviteMoyenneAvant = ctx.Entreprises.Average(e => e.ProductiviteParEmployeJour);

                    foreach (var ent in ctx.Entreprises)
                        ent.ProductiviteParEmployeJour *= facteurCorrection;
                    foreach (var imp in ctx.Importateurs)
                        imp.ProductiviteParEmployeJour *= facteurCorrection;
                    foreach (var exp in ctx.Exportateurs)
                        exp.ProductiviteParEmployeJour *= facteurCorrection;

                    evt.Ajustements.Add(new CalibrationAdjustment
                    {
                        Parametre = "ProductiviteParEmployeJour",
                        AncienneValeur = productiviteMoyenneAvant,
                        NouvelleValeur = ctx.Entreprises.Average(e => e.ProductiviteParEmployeJour),
                        ValeurSimulee = simulee / ctx.FacteurEchelle,
                        ValeurReference = ctx.Cible.RecettesFiscalesCumuleesCible.Value,
                        EcartRelatif = ecart,
                        SeuilApplique = seuil,
                    });
                }
            }
        }

        // ═══════════════════════════════════════════
        //  3. EXPORTATIONS FOB
        // ═══════════════════════════════════════════
        if (ctx.Cible.ExportationsFOBCumuleesCible.HasValue && ctx.Cible.ExportationsFOBCumuleesCible.Value > 0)
        {
            double reference = ctx.Cible.ExportationsFOBCumuleesCible.Value * ctx.FacteurEchelle;
            double fobSimulee = ctx.Exportateurs.Sum(e => e.TotalExportationsFOB);

            if (fobSimulee > 0 && reference > 0)
            {
                double ecart = (fobSimulee - reference) / reference;

                if (Math.Abs(ecart) > seuil)
                {
                    double bordRatio = ecart > 0
                        ? reference * (1.0 + seuil) / fobSimulee
                        : reference * (1.0 - seuil) / fobSimulee;

                    double facteurCorrection = 1.0 + alpha * (bordRatio - 1.0);
                    facteurCorrection = Math.Clamp(facteurCorrection, 0.5, 2.0);

                    double fobMoyenneAvant = ctx.Exportateurs.Count > 0
                        ? ctx.Exportateurs.Average(e => e.ValeurFOBJour) : 0;

                    foreach (var exp in ctx.Exportateurs)
                        exp.ValeurFOBJour *= facteurCorrection;

                    evt.Ajustements.Add(new CalibrationAdjustment
                    {
                        Parametre = "ValeurFOBJour (exportateurs)",
                        AncienneValeur = fobMoyenneAvant,
                        NouvelleValeur = ctx.Exportateurs.Count > 0 ? ctx.Exportateurs.Average(e => e.ValeurFOBJour) : 0,
                        ValeurSimulee = fobSimulee / ctx.FacteurEchelle,
                        ValeurReference = ctx.Cible.ExportationsFOBCumuleesCible.Value,
                        EcartRelatif = ecart,
                        SeuilApplique = seuil,
                    });
                }
            }
        }

        // ═══════════════════════════════════════════
        //  4. IMPORTATIONS CIF
        // ═══════════════════════════════════════════
        if (ctx.Cible.ImportationsCIFCumuleesCible.HasValue && ctx.Cible.ImportationsCIFCumuleesCible.Value > 0)
        {
            double reference = ctx.Cible.ImportationsCIFCumuleesCible.Value * ctx.FacteurEchelle;
            double cifSimulee = ctx.Importateurs.Sum(e => e.TotalImportationsCIF);

            if (cifSimulee > 0 && reference > 0)
            {
                double ecart = (cifSimulee - reference) / reference;

                if (Math.Abs(ecart) > seuil)
                {
                    double bordRatio = ecart > 0
                        ? reference * (1.0 + seuil) / cifSimulee
                        : reference * (1.0 - seuil) / cifSimulee;

                    double facteurCorrection = 1.0 + alpha * (bordRatio - 1.0);
                    facteurCorrection = Math.Clamp(facteurCorrection, 0.5, 2.0);

                    double cifMoyenneAvant = ctx.Importateurs.Count > 0
                        ? ctx.Importateurs.Average(e => e.ValeurCIFJour) : 0;

                    foreach (var imp in ctx.Importateurs)
                        imp.ValeurCIFJour *= facteurCorrection;

                    evt.Ajustements.Add(new CalibrationAdjustment
                    {
                        Parametre = "ValeurCIFJour (importateurs)",
                        AncienneValeur = cifMoyenneAvant,
                        NouvelleValeur = ctx.Importateurs.Count > 0 ? ctx.Importateurs.Average(e => e.ValeurCIFJour) : 0,
                        ValeurSimulee = cifSimulee / ctx.FacteurEchelle,
                        ValeurReference = ctx.Cible.ImportationsCIFCumuleesCible.Value,
                        EcartRelatif = ecart,
                        SeuilApplique = seuil,
                    });
                }
            }
        }

        // ═══════════════════════════════════════════
        //  5. DEVISES TOURISME
        // ═══════════════════════════════════════════
        if (ctx.Cible.DevisesTourismeCible.HasValue && ctx.Cible.DevisesTourismeCible.Value > 0)
        {
            double reference = ctx.Cible.DevisesTourismeCible.Value * ctx.FacteurEchelle;
            double tourismeCumulee = ctx.SnapshotActuel.RecettesTourismeCumulees;
            double tourismeMois = ctx.Mois > 1 && tourismeCumulee > 0
                ? tourismeCumulee / ctx.Mois
                : tourismeCumulee;

            var entreprisesTourisme = ctx.Entreprises
                .Where(e => e.SecteurActivite == ESecteurActivite.HotellerieTourisme)
                .ToList();

            if (tourismeMois > 0 && reference > 0 && entreprisesTourisme.Count > 0)
            {
                double ecart = (tourismeMois - reference) / reference;

                if (Math.Abs(ecart) > seuil)
                {
                    double bordRatio = ecart > 0
                        ? reference * (1.0 + seuil) / tourismeMois
                        : reference * (1.0 - seuil) / tourismeMois;

                    double facteurCorrection = 1.0 + alpha * (bordRatio - 1.0);
                    facteurCorrection = Math.Clamp(facteurCorrection, 0.5, 2.0);

                    double productiviteAvant = entreprisesTourisme.Average(e => e.ProductiviteParEmployeJour);
                    foreach (var ent in entreprisesTourisme)
                        ent.ProductiviteParEmployeJour *= facteurCorrection;

                    evt.Ajustements.Add(new CalibrationAdjustment
                    {
                        Parametre = "ProductiviteParEmployeJour (tourisme)",
                        AncienneValeur = productiviteAvant,
                        NouvelleValeur = entreprisesTourisme.Average(e => e.ProductiviteParEmployeJour),
                        ValeurSimulee = tourismeMois / ctx.FacteurEchelle,
                        ValeurReference = ctx.Cible.DevisesTourismeCible.Value,
                        EcartRelatif = ecart,
                        SeuilApplique = seuil,
                    });
                }
            }
        }

        // ═══════════════════════════════════════════
        //  6. AFFILIÉS CNaPS (emploi formel)
        // ═══════════════════════════════════════════
        //  Garde-fou : ne formalise que si l'écart d'emploi formel
        //  est excessif (bien au-delà de la bande). Les scénarios qui
        //  réduisent volontairement l'informel ne seront pas contrecarrés.
        if (ctx.Cible.NouveauxAffiliesCNaPSCible.HasValue && ctx.Cible.NouveauxAffiliesCNaPSCible.Value > 0)
        {
            int referenceAffiliés = (int)(ctx.Cible.NouveauxAffiliesCNaPSCible.Value * ctx.FacteurEchelle);
            int formelsSimules = ctx.SnapshotActuel.NbSalariesSecteurFormel;

            if (referenceAffiliés > 0 && formelsSimules > 0)
            {
                // Écart : simulation a moins de formels que la référence
                double ecart = (double)(formelsSimules - referenceAffiliés) / referenceAffiliés;

                // Ne formaliser que si le simulé est significativement en dessous
                if (ecart < -seuil)
                {
                    // Nombre à formaliser : seulement l'excès au-delà du bord
                    int bordInferieur = (int)(referenceAffiliés * (1.0 - seuil));
                    int deficitExces = Math.Max(0, bordInferieur - formelsSimules);

                    // Correction partielle
                    int aFormaliser = (int)(deficitExces * alpha);
                    if (aFormaliser <= 0) aFormaliser = 1;

                    var informellesCandidates = ctx.Entreprises
                        .Where(e => e.EstInformel)
                        .OrderByDescending(e => e.ChiffreAffairesCumule)
                        .ToList();

                    int employesFormalises = 0;
                    int entreprisesFormaliseesCount = 0;

                    foreach (var ent in informellesCandidates)
                    {
                        if (employesFormalises >= aFormaliser)
                            break;
                        ent.EstInformel = false;
                        employesFormalises += ent.NombreEmployes;
                        entreprisesFormaliseesCount++;
                    }

                    if (entreprisesFormaliseesCount > 0)
                    {
                        evt.Ajustements.Add(new CalibrationAdjustment
                        {
                            Parametre = "Formalisation entreprises (CNaPS)",
                            AncienneValeur = informellesCandidates.Count + entreprisesFormaliseesCount,
                            NouvelleValeur = informellesCandidates.Count,
                            ValeurSimulee = formelsSimules,
                            ValeurReference = referenceAffiliés,
                            EcartRelatif = ecart,
                            SeuilApplique = seuil,
                        });
                    }
                }
            }
        }

        return evt.Ajustements.Count > 0 ? evt : null;
    }
}
