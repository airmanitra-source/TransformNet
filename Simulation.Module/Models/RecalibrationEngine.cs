using Simulation.Module.Config;
using AgentCompany = Company.Module.Models.Company;
using AgentImporter = Company.Module.Models.Importer;
using AgentExporter = Company.Module.Models.Exporter;
using AgentGovernment = Government.Module.Models.Government;
using Company.Module.Models;

namespace Simulation.Module.Models;

/// <summary>
/// Moteur de recalibration mensuelle.
/// Compare les grandeurs macroéconomiques simulées aux données observées
/// et ajuste les paramètres internes de la simulation pour converger vers la réalité.
///
/// Principe : à chaque fin de ctx.Mois (jour 30, 60, 90…), on calcule l'écart entre
/// la valeur simulée et la cible observée, puis on applique une correction proportionnelle
/// sur le paramètre qui pilote cette grandeur.
///
/// Grandeurs recalibrées :
///   1. M3 (masse monétaire)     → ajuste CroissanceCreditJour
///   2. Recettes fiscales         → ajuste ProductiviteParEmployeJour (volume des ventes)
///   3. Exports FOB               → ajuste ValeurFOBJour des ctx.Exportateurs
///   4. Imports CIF               → ajuste ValeurCIFJour des ctx.Importateurs
///
/// Note : les taux fiscaux (TVA, IS, IR) sont des paramètres de politique économique
/// et ne sont jamais modifiés par la recalibration. L'écart de recettes s'explique
/// par un volume de ventes simulé différent de la réalité, d'où l'ajustement
/// de la productivité effective des ctx.Entreprises.
/// </summary>
public static class RecalibrationEngine
{
    /// <summary>
    /// Exécute la recalibration pour un ctx.Mois donné.
    /// Compare les snapshots simulés aux cibles et retourne les ajustements.
    /// </summary>
    public static CalibrationEvent? Recalibrer(RecalibrationContext ctx)
    {
        double alpha = Math.Clamp(ctx.Config.VitesseConvergenceRecalibration, 0.05, 1.0);
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
            // La M3 simulée est à l'échelle de la simulation (× ctx.FacteurEchelle)
            // La cible est à l'échelle réelle → mettre à l'échelle
            double m3Cible = ctx.Cible.M3Cible.Value * ctx.FacteurEchelle;
            double m3Simulee = ctx.SnapshotActuel.MasseMonetaireM3;

            if (m3Simulee > 0)
            {
                // Écart relatif : si simulé > cible → ralentir le crédit, sinon accélérer
                double ratio = m3Cible / m3Simulee;

                // Jours restants dans la simulation pour estimer le taux journalier nécessaire
                int joursRestants = Math.Max(30, ctx.Config.DureeJours - ctx.JourCourant);

                // Nouveau taux = taux nécessaire pour atteindre la cible du prochain ctx.Mois
                // On corrige partiellement : α × correction + (1−α) × taux actuel
                double tauxCourant = ctx.Config.CroissanceCreditJour;

                // Le taux nécessaire pour combler l'écart en joursRestants :
                // m3_cible = m3_simulee × (1 + r)^joursRestants  →  r = (cible/simulé)^(1/jours) - 1
                double tauxNecessaire = Math.Pow(ratio, 1.0 / joursRestants) - 1.0;
                tauxNecessaire = Math.Clamp(tauxNecessaire, -0.005, 0.005);

                double nouveauTaux = alpha * tauxNecessaire + (1.0 - alpha) * tauxCourant;
                nouveauTaux = Math.Clamp(nouveauTaux, 0, 0.005);

                evt.Ajustements.Add(new CalibrationAdjustment
                {
                    Parametre = "CroissanceCreditJour",
                    AncienneValeur = tauxCourant,
                    NouvelleValeur = nouveauTaux,
                    ValeurSimulee = m3Simulee / ctx.FacteurEchelle,
                    ValeurCible = ctx.Cible.M3Cible.Value,
                });

                ctx.Config.CroissanceCreditJour = nouveauTaux;
            }
        }

        // ═══════════════════════════════════════════
        //  2. RECETTES FISCALES → ajuste la productivité (volume des ventes)
        // ═══════════════════════════════════════════
        //
        // Les taux (TVA, IS, IR) sont des paramètres de politique fiscale : on n'y touche pas.
        // Si les recettes simulées s'écartent de la cible, c'est que le volume de ventes
        // (CA des ctx.Entreprises) est trop haut ou trop bas.
        //
        // Levier : ProductiviteParEmployeJour de chaque entreprise.
        //   CA_jour = NombreEmployes × ProductiviteParEmployeJour × facteurPrixVente
        //   TVA = CA × taux / (1+taux)   →  TVA ∝ ProductiviteParEmployeJour
        //   IS ∝ bénéfice ∝ CA            →  IS ∝ ProductiviteParEmployeJour
        //
        if (ctx.Cible.RecettesFiscalesCumuleesCible.HasValue && ctx.Cible.RecettesFiscalesCumuleesCible.Value > 0)
        {
            double cibleFiscale = ctx.Cible.RecettesFiscalesCumuleesCible.Value * ctx.FacteurEchelle;
            double simulee = ctx.Etat.TotalRecettesFiscales;

            if (simulee > 0 && cibleFiscale > 0 && ctx.Entreprises.Count > 0)
            {
                double ratio = cibleFiscale / simulee;

                // Correction proportionnelle avec convergence douce
                double facteurCorrection = alpha * ratio + (1.0 - alpha);
                // Borner pour éviter les chocs violents (±50% max par ctx.Mois)
                facteurCorrection = Math.Clamp(facteurCorrection, 0.5, 1.5);

                double productiviteMoyenneAvant = ctx.Entreprises.Average(e => e.ProductiviteParEmployeJour);

                foreach (var ent in ctx.Entreprises)
                {
                    ent.ProductiviteParEmployeJour *= facteurCorrection;
                }
                // Appliquer aussi aux ctx.Importateurs et ctx.Exportateurs (qui héritent de Company)
                foreach (var imp in ctx.Importateurs)
                {
                    imp.ProductiviteParEmployeJour *= facteurCorrection;
                }
                foreach (var exp in ctx.Exportateurs)
                {
                    exp.ProductiviteParEmployeJour *= facteurCorrection;
                }

                double productiviteMoyenneApres = ctx.Entreprises.Average(e => e.ProductiviteParEmployeJour);

                evt.Ajustements.Add(new CalibrationAdjustment
                {
                    Parametre = "ProductiviteParEmployeJour",
                    AncienneValeur = productiviteMoyenneAvant,
                    NouvelleValeur = productiviteMoyenneApres,
                    ValeurSimulee = simulee / ctx.FacteurEchelle,
                    ValeurCible = ctx.Cible.RecettesFiscalesCumuleesCible.Value,
                });
            }
        }

        // ═══════════════════════════════════════════
        //  3. EXPORTATIONS FOB
        // ═══════════════════════════════════════════
        if (ctx.Cible.ExportationsFOBCumuleesCible.HasValue && ctx.Cible.ExportationsFOBCumuleesCible.Value > 0)
        {
            double cibleFOB = ctx.Cible.ExportationsFOBCumuleesCible.Value * ctx.FacteurEchelle;
            double fobSimulee = ctx.Exportateurs.Sum(e => e.TotalExportationsFOB);

            if (fobSimulee > 0 && cibleFOB > 0)
            {
                double ratio = cibleFOB / fobSimulee;
                double facteurCorrection = alpha * ratio + (1.0 - alpha);
                facteurCorrection = Math.Clamp(facteurCorrection, 0.5, 2.0);

                double fobMoyenneAvant = ctx.Exportateurs.Count > 0
                    ? ctx.Exportateurs.Average(e => e.ValeurFOBJour) : 0;

                foreach (var exp in ctx.Exportateurs)
                {
                    exp.ValeurFOBJour *= facteurCorrection;
                }

                double fobMoyenneApres = ctx.Exportateurs.Count > 0
                    ? ctx.Exportateurs.Average(e => e.ValeurFOBJour) : 0;

                evt.Ajustements.Add(new CalibrationAdjustment
                {
                    Parametre = "ValeurFOBJour (ctx.Exportateurs)",
                    AncienneValeur = fobMoyenneAvant,
                    NouvelleValeur = fobMoyenneApres,
                    ValeurSimulee = fobSimulee / ctx.FacteurEchelle,
                    ValeurCible = ctx.Cible.ExportationsFOBCumuleesCible.Value,
                });
            }
        }

        // ═══════════════════════════════════════════
        //  4. IMPORTATIONS CIF
        // ═══════════════════════════════════════════
        if (ctx.Cible.ImportationsCIFCumuleesCible.HasValue && ctx.Cible.ImportationsCIFCumuleesCible.Value > 0)
        {
            double cibleCIF = ctx.Cible.ImportationsCIFCumuleesCible.Value * ctx.FacteurEchelle;
            double cifSimulee = ctx.Importateurs.Sum(e => e.TotalImportationsCIF);

            if (cifSimulee > 0 && cibleCIF > 0)
            {
                double ratio = cibleCIF / cifSimulee;
                double facteurCorrection = alpha * ratio + (1.0 - alpha);
                facteurCorrection = Math.Clamp(facteurCorrection, 0.5, 2.0);

                double cifMoyenneAvant = ctx.Importateurs.Count > 0
                    ? ctx.Importateurs.Average(e => e.ValeurCIFJour) : 0;

                foreach (var imp in ctx.Importateurs)
                {
                    imp.ValeurCIFJour *= facteurCorrection;
                }

                double cifMoyenneApres = ctx.Importateurs.Count > 0
                    ? ctx.Importateurs.Average(e => e.ValeurCIFJour) : 0;

                evt.Ajustements.Add(new CalibrationAdjustment
                {
                    Parametre = "ValeurCIFJour (ctx.Importateurs)",
                    AncienneValeur = cifMoyenneAvant,
                    NouvelleValeur = cifMoyenneApres,
                    ValeurSimulee = cifSimulee / ctx.FacteurEchelle,
                    ValeurCible = ctx.Cible.ImportationsCIFCumuleesCible.Value,
                });
            }
        }

        // ═══════════════════════════════════════════
        //  5. DEVISES TOURISME (visiteurs non-résidents)
        // ═══════════════════════════════════════════
        //
        // Les recettes touristiques sont un apport de devises étrangères
        // (balance des paiements, poste "voyages"). Dans la simulation,
        // elles correspondent au CA des ctx.Entreprises HôtellerieTourisme.
        //
        // Levier : ProductiviteParEmployeJour des ctx.Entreprises HôtellerieTourisme
        //   → augmente leur CA → augmente les recettes simulées
        //
        // Note : on ajuste uniquement le secteur tourisme, pas toutes les ctx.Entreprises
        // (contrairement à la correction fiscale qui touche tout le monde).
        //
        if (ctx.Cible.DevisesTourismeCible.HasValue && ctx.Cible.DevisesTourismeCible.Value > 0)
        {
            // La cible est mensuelle (apport du ctx.Mois), le simulé est cumulé
            // → on utilise le CA cumulé des ctx.Entreprises tourisme pour ce ctx.Mois
            double cibleTourisme = ctx.Cible.DevisesTourismeCible.Value * ctx.FacteurEchelle;
            double tourismeCumuleeSimulee = ctx.SnapshotActuel.RecettesTourismeCumulees;

            // Estimer le CA du ctx.Mois courant (cumulé actuel - cumulé du ctx.Mois précédent)
            // Approximation : CA du ctx.Mois ≈ CA cumulé / nombre de ctx.Mois écoulés × 1
            double tourismeMoisSimulee = ctx.Mois > 1 && tourismeCumuleeSimulee > 0
                ? tourismeCumuleeSimulee / ctx.Mois  // approximation moyenne
                : tourismeCumuleeSimulee;

            var entreprisesTourisme = ctx.Entreprises
                .Where(e => e.SecteurActivite == ESecteurActivite.HotellerieTourisme)
                .ToList();

            if (tourismeMoisSimulee > 0 && cibleTourisme > 0 && entreprisesTourisme.Count > 0)
            {
                double ratio = cibleTourisme / tourismeMoisSimulee;
                double facteurCorrection = alpha * ratio + (1.0 - alpha);
                facteurCorrection = Math.Clamp(facteurCorrection, 0.5, 2.0);

                double productiviteAvant = entreprisesTourisme.Average(e => e.ProductiviteParEmployeJour);

                foreach (var ent in entreprisesTourisme)
                {
                    ent.ProductiviteParEmployeJour *= facteurCorrection;
                }

                double productiviteApres = entreprisesTourisme.Average(e => e.ProductiviteParEmployeJour);

                evt.Ajustements.Add(new CalibrationAdjustment
                {
                    Parametre = "ProductiviteParEmployeJour (tourisme)",
                    AncienneValeur = productiviteAvant,
                    NouvelleValeur = productiviteApres,
                    ValeurSimulee = tourismeMoisSimulee / ctx.FacteurEchelle,
                    ValeurCible = ctx.Cible.DevisesTourismeCible.Value,
                });
            }
        }

        // ═══════════════════════════════════════════
        //  6. AFFILIÉS CNaPS (emploi formel)
        // ═══════════════════════════════════════════
        //
        // Le nombre de nouveaux affiliés CNaPS reflète la croissance de l'emploi
        // formel. Si la cible est supérieure au simulé, on formalise des ctx.Entreprises
        // informelles (EstInformel → false), ce qui :
        //   - augmente les cotisations CNaPS collectées
        //   - augmente la TVA (les informelles en sont exonérées)
        //   - augmente l'IS
        //
        // Levier : toggle EstInformel sur les ctx.Entreprises les plus proches
        // du seuil de formalisation (triées par CA décroissant = les plus viables).
        //
        if (ctx.Cible.NouveauxAffiliesCNaPSCible.HasValue && ctx.Cible.NouveauxAffiliesCNaPSCible.Value > 0)
        {
            int cibleAffiliés = (int)(ctx.Cible.NouveauxAffiliesCNaPSCible.Value * ctx.FacteurEchelle);
            int salariesFormelsSimules = ctx.SnapshotActuel.NbSalariesSecteurFormel;

            // Nouveaux affiliés simulés ce ctx.Mois ≈ variation par rapport au stock initial
            // On compare le stock actuel au stock attendu (stock initial + cible cumulée)
            int stockAttendu = salariesFormelsSimules + cibleAffiliés;

            if (cibleAffiliés > 0)
            {
                // Identifier les ctx.Entreprises informelles candidates à la formalisation
                // Trier par CA cumulé décroissant : les plus grosses informelles d'abord
                var informellesCandidates = ctx.Entreprises
                    .Where(e => e.EstInformel)
                    .OrderByDescending(e => e.ChiffreAffairesCumule)
                    .ToList();

                int employesFormalises = 0;
                int entreprisesFormaliseesCount = 0;

                foreach (var ent in informellesCandidates)
                {
                    if (employesFormalises >= cibleAffiliés)
                        break;

                    ent.EstInformel = false;
                    employesFormalises += ent.NombreEmployes;
                    entreprisesFormaliseesCount++;
                }

                if (entreprisesFormaliseesCount > 0)
                {
                    evt.Ajustements.Add(new CalibrationAdjustment
                    {
                        Parametre = "Formalisation ctx.Entreprises (CNaPS)",
                        AncienneValeur = informellesCandidates.Count + entreprisesFormaliseesCount,
                        NouvelleValeur = informellesCandidates.Count,
                        ValeurSimulee = salariesFormelsSimules,
                        ValeurCible = stockAttendu,
                    });
                }
            }
        }

        return evt.Ajustements.Count > 0 ? evt : null;
    }
}
