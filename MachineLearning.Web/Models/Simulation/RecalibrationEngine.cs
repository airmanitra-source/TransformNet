using MachineLearning.Web.Models.Simulation.Config;
using AgentCompany = Company.Module.Models.Company;
using AgentImporter = Company.Module.Models.Importer;
using AgentExporter = Company.Module.Models.Exporter;
using AgentGovernment = Government.Module.Models.Government;
using Company.Module.Models;

namespace MachineLearning.Web.Models.Simulation;

/// <summary>
/// Moteur de recalibration mensuelle.
/// Compare les grandeurs macroéconomiques simulées aux données observées
/// et ajuste les paramètres internes de la simulation pour converger vers la réalité.
///
/// Principe : à chaque fin de mois (jour 30, 60, 90…), on calcule l'écart entre
/// la valeur simulée et la cible observée, puis on applique une correction proportionnelle
/// sur le paramètre qui pilote cette grandeur.
///
/// Grandeurs recalibrées :
///   1. M3 (masse monétaire)     → ajuste CroissanceCreditJour
///   2. Recettes fiscales         → ajuste ProductiviteParEmployeJour (volume des ventes)
///   3. Exports FOB               → ajuste ValeurFOBJour des exportateurs
///   4. Imports CIF               → ajuste ValeurCIFJour des importateurs
///
/// Note : les taux fiscaux (TVA, IS, IR) sont des paramètres de politique économique
/// et ne sont jamais modifiés par la recalibration. L'écart de recettes s'explique
/// par un volume de ventes simulé différent de la réalité, d'où l'ajustement
/// de la productivité effective des entreprises.
/// </summary>
public static class RecalibrationEngine
{
    /// <summary>
    /// Exécute la recalibration pour un mois donné.
    /// Compare les snapshots simulés aux cibles et retourne les ajustements.
    /// </summary>
    public static CalibrationEvent? Recalibrer(
        int jourCourant,
        int mois,
        MonthlyCalibrationTarget cible,
        DailySnapshotViewModel snapshotActuel,
        AgentGovernment etat,
        ScenarioConfigViewModel config,
        IReadOnlyList<AgentCompany> entreprises,
        IReadOnlyList<AgentImporter> importateurs,
        IReadOnlyList<AgentExporter> exportateurs,
        double facteurEchelle)
    {
        double alpha = Math.Clamp(config.VitesseConvergenceRecalibration, 0.05, 1.0);
        var evt = new CalibrationEvent
        {
            Jour = jourCourant,
            Mois = mois,
        };

        // ═══════════════════════════════════════════
        //  1. MASSE MONÉTAIRE M3
        // ═══════════════════════════════════════════
        if (cible.M3Cible.HasValue && cible.M3Cible.Value > 0)
        {
            // La M3 simulée est à l'échelle de la simulation (× facteurEchelle)
            // La cible est à l'échelle réelle → mettre à l'échelle
            double m3Cible = cible.M3Cible.Value * facteurEchelle;
            double m3Simulee = snapshotActuel.MasseMonetaireM3;

            if (m3Simulee > 0)
            {
                // Écart relatif : si simulé > cible → ralentir le crédit, sinon accélérer
                double ratio = m3Cible / m3Simulee;

                // Jours restants dans la simulation pour estimer le taux journalier nécessaire
                int joursRestants = Math.Max(30, config.DureeJours - jourCourant);

                // Nouveau taux = taux nécessaire pour atteindre la cible du prochain mois
                // On corrige partiellement : α × correction + (1−α) × taux actuel
                double tauxCourant = config.CroissanceCreditJour;

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
                    ValeurSimulee = m3Simulee / facteurEchelle,
                    ValeurCible = cible.M3Cible.Value,
                });

                config.CroissanceCreditJour = nouveauTaux;
            }
        }

        // ═══════════════════════════════════════════
        //  2. RECETTES FISCALES → ajuste la productivité (volume des ventes)
        // ═══════════════════════════════════════════
        //
        // Les taux (TVA, IS, IR) sont des paramètres de politique fiscale : on n'y touche pas.
        // Si les recettes simulées s'écartent de la cible, c'est que le volume de ventes
        // (CA des entreprises) est trop haut ou trop bas.
        //
        // Levier : ProductiviteParEmployeJour de chaque entreprise.
        //   CA_jour = NombreEmployes × ProductiviteParEmployeJour × facteurPrixVente
        //   TVA = CA × taux / (1+taux)   →  TVA ∝ ProductiviteParEmployeJour
        //   IS ∝ bénéfice ∝ CA            →  IS ∝ ProductiviteParEmployeJour
        //
        if (cible.RecettesFiscalesCumuleesCible.HasValue && cible.RecettesFiscalesCumuleesCible.Value > 0)
        {
            double cibleFiscale = cible.RecettesFiscalesCumuleesCible.Value * facteurEchelle;
            double simulee = etat.TotalRecettesFiscales;

            if (simulee > 0 && cibleFiscale > 0 && entreprises.Count > 0)
            {
                double ratio = cibleFiscale / simulee;

                // Correction proportionnelle avec convergence douce
                double facteurCorrection = alpha * ratio + (1.0 - alpha);
                // Borner pour éviter les chocs violents (±50% max par mois)
                facteurCorrection = Math.Clamp(facteurCorrection, 0.5, 1.5);

                double productiviteMoyenneAvant = entreprises.Average(e => e.ProductiviteParEmployeJour);

                foreach (var ent in entreprises)
                {
                    ent.ProductiviteParEmployeJour *= facteurCorrection;
                }
                // Appliquer aussi aux importateurs et exportateurs (qui héritent de Company)
                foreach (var imp in importateurs)
                {
                    imp.ProductiviteParEmployeJour *= facteurCorrection;
                }
                foreach (var exp in exportateurs)
                {
                    exp.ProductiviteParEmployeJour *= facteurCorrection;
                }

                double productiviteMoyenneApres = entreprises.Average(e => e.ProductiviteParEmployeJour);

                evt.Ajustements.Add(new CalibrationAdjustment
                {
                    Parametre = "ProductiviteParEmployeJour",
                    AncienneValeur = productiviteMoyenneAvant,
                    NouvelleValeur = productiviteMoyenneApres,
                    ValeurSimulee = simulee / facteurEchelle,
                    ValeurCible = cible.RecettesFiscalesCumuleesCible.Value,
                });
            }
        }

        // ═══════════════════════════════════════════
        //  3. EXPORTATIONS FOB
        // ═══════════════════════════════════════════
        if (cible.ExportationsFOBCumuleesCible.HasValue && cible.ExportationsFOBCumuleesCible.Value > 0)
        {
            double cibleFOB = cible.ExportationsFOBCumuleesCible.Value * facteurEchelle;
            double fobSimulee = exportateurs.Sum(e => e.TotalExportationsFOB);

            if (fobSimulee > 0 && cibleFOB > 0)
            {
                double ratio = cibleFOB / fobSimulee;
                double facteurCorrection = alpha * ratio + (1.0 - alpha);
                facteurCorrection = Math.Clamp(facteurCorrection, 0.5, 2.0);

                double fobMoyenneAvant = exportateurs.Count > 0
                    ? exportateurs.Average(e => e.ValeurFOBJour) : 0;

                foreach (var exp in exportateurs)
                {
                    exp.ValeurFOBJour *= facteurCorrection;
                }

                double fobMoyenneApres = exportateurs.Count > 0
                    ? exportateurs.Average(e => e.ValeurFOBJour) : 0;

                evt.Ajustements.Add(new CalibrationAdjustment
                {
                    Parametre = "ValeurFOBJour (exportateurs)",
                    AncienneValeur = fobMoyenneAvant,
                    NouvelleValeur = fobMoyenneApres,
                    ValeurSimulee = fobSimulee / facteurEchelle,
                    ValeurCible = cible.ExportationsFOBCumuleesCible.Value,
                });
            }
        }

        // ═══════════════════════════════════════════
        //  4. IMPORTATIONS CIF
        // ═══════════════════════════════════════════
        if (cible.ImportationsCIFCumuleesCible.HasValue && cible.ImportationsCIFCumuleesCible.Value > 0)
        {
            double cibleCIF = cible.ImportationsCIFCumuleesCible.Value * facteurEchelle;
            double cifSimulee = importateurs.Sum(e => e.TotalImportationsCIF);

            if (cifSimulee > 0 && cibleCIF > 0)
            {
                double ratio = cibleCIF / cifSimulee;
                double facteurCorrection = alpha * ratio + (1.0 - alpha);
                facteurCorrection = Math.Clamp(facteurCorrection, 0.5, 2.0);

                double cifMoyenneAvant = importateurs.Count > 0
                    ? importateurs.Average(e => e.ValeurCIFJour) : 0;

                foreach (var imp in importateurs)
                {
                    imp.ValeurCIFJour *= facteurCorrection;
                }

                double cifMoyenneApres = importateurs.Count > 0
                    ? importateurs.Average(e => e.ValeurCIFJour) : 0;

                evt.Ajustements.Add(new CalibrationAdjustment
                {
                    Parametre = "ValeurCIFJour (importateurs)",
                    AncienneValeur = cifMoyenneAvant,
                    NouvelleValeur = cifMoyenneApres,
                    ValeurSimulee = cifSimulee / facteurEchelle,
                    ValeurCible = cible.ImportationsCIFCumuleesCible.Value,
                });
            }
        }

        // ═══════════════════════════════════════════
        //  5. DEVISES TOURISME (visiteurs non-résidents)
        // ═══════════════════════════════════════════
        //
        // Les recettes touristiques sont un apport de devises étrangères
        // (balance des paiements, poste "voyages"). Dans la simulation,
        // elles correspondent au CA des entreprises HôtellerieTourisme.
        //
        // Levier : ProductiviteParEmployeJour des entreprises HôtellerieTourisme
        //   → augmente leur CA → augmente les recettes simulées
        //
        // Note : on ajuste uniquement le secteur tourisme, pas toutes les entreprises
        // (contrairement à la correction fiscale qui touche tout le monde).
        //
        if (cible.DevisesTourismeCible.HasValue && cible.DevisesTourismeCible.Value > 0)
        {
            // La cible est mensuelle (apport du mois), le simulé est cumulé
            // → on utilise le CA cumulé des entreprises tourisme pour ce mois
            double cibleTourisme = cible.DevisesTourismeCible.Value * facteurEchelle;
            double tourismeCumuleeSimulee = snapshotActuel.RecettesTourismeCumulees;

            // Estimer le CA du mois courant (cumulé actuel - cumulé du mois précédent)
            // Approximation : CA du mois ≈ CA cumulé / nombre de mois écoulés × 1
            double tourismeMoisSimulee = mois > 1 && tourismeCumuleeSimulee > 0
                ? tourismeCumuleeSimulee / mois  // approximation moyenne
                : tourismeCumuleeSimulee;

            var entreprisesTourisme = entreprises
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
                    ValeurSimulee = tourismeMoisSimulee / facteurEchelle,
                    ValeurCible = cible.DevisesTourismeCible.Value,
                });
            }
        }

        // ═══════════════════════════════════════════
        //  6. AFFILIÉS CNaPS (emploi formel)
        // ═══════════════════════════════════════════
        //
        // Le nombre de nouveaux affiliés CNaPS reflète la croissance de l'emploi
        // formel. Si la cible est supérieure au simulé, on formalise des entreprises
        // informelles (EstInformel → false), ce qui :
        //   - augmente les cotisations CNaPS collectées
        //   - augmente la TVA (les informelles en sont exonérées)
        //   - augmente l'IS
        //
        // Levier : toggle EstInformel sur les entreprises les plus proches
        // du seuil de formalisation (triées par CA décroissant = les plus viables).
        //
        if (cible.NouveauxAffiliesCNaPSCible.HasValue && cible.NouveauxAffiliesCNaPSCible.Value > 0)
        {
            int cibleAffiliés = (int)(cible.NouveauxAffiliesCNaPSCible.Value * facteurEchelle);
            int salariesFormelsSimules = snapshotActuel.NbSalariesSecteurFormel;

            // Nouveaux affiliés simulés ce mois ≈ variation par rapport au stock initial
            // On compare le stock actuel au stock attendu (stock initial + cible cumulée)
            int stockAttendu = salariesFormelsSimules + cibleAffiliés;

            if (cibleAffiliés > 0)
            {
                // Identifier les entreprises informelles candidates à la formalisation
                // Trier par CA cumulé décroissant : les plus grosses informelles d'abord
                var informellesCandidates = entreprises
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
                        Parametre = "Formalisation entreprises (CNaPS)",
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
