using Company.Module.Models;

namespace Company.Module
{
    /// <summary>
    /// Implémentation du module Entreprises.
    /// Centralise tous les calculs métier : productivité, trésorerie, taxes, coefficients sectoriels.
    /// Elimine la duplication de logique dans Company, Importer, Exporter.
    /// </summary>
    public class CompanyModule : ICompanyModule
    {
        /// <summary>
        /// Productivité calibrée par secteur (CA/employé/jour en MGA).
        /// Basée sur les statistiques INSTAT SCN Madagascar.
        /// </summary>
        private static readonly Dictionary<ESecteurActivite, double> ProductiviteParSecteur = new()
        {
            { ESecteurActivite.Agriculture, 8_000 },      // ~1,2M MGA/an par agriculteur
            { ESecteurActivite.Textiles, 35_000 },        // Zones franches export + productivité élevée
            { ESecteurActivite.Commerces, 25_000 },       // CA retail typique
            { ESecteurActivite.Services, 18_000 },        // Moins capitalistique
            { ESecteurActivite.SecteurMinier, 150_000 },  // Très productive (Ambatovy, Sherritt)
            { ESecteurActivite.Construction, 40_000 },     // BTP calibré sur PIB
            { ESecteurActivite.HotellerieTourisme, 30_000 }  // Hôtellerie/tourisme (CA/employé/jour)
        };

        /// <summary>
        /// Trésorerie initiale par secteur (fonds de roulement minimum en MGA).
        /// </summary>
        private static readonly Dictionary<ESecteurActivite, double> TresorerieInitialeParSecteur = new()
        {
            { ESecteurActivite.Agriculture, 2_000_000 },
            { ESecteurActivite.Textiles, 10_000_000 },
            { ESecteurActivite.Commerces, 5_000_000 },
            { ESecteurActivite.Services, 8_000_000 },
            { ESecteurActivite.SecteurMinier, 50_000_000 },
            { ESecteurActivite.Construction, 15_000_000 },
            { ESecteurActivite.HotellerieTourisme, 12_000_000 }
        };

        /// <summary>
        /// Coefficients de droits de douane par catégorie d'importation.
        /// Réflète les taux douaniers Madagascar (Tarif Extérieur Commun CEMAC).
        /// </summary>
        private static readonly Dictionary<ECategorieImport, double> CoefficientsDroitsDouane = new()
        {
            { ECategorieImport.Carburant, 0.05 },           // Taux réduit (énergie essentielle)
            { ECategorieImport.Alimentaire, 0.15 },         // Taux standard
            { ECategorieImport.Electronique, 0.20 },        // TEC CEMAC
            { ECategorieImport.Vehicule, 0.25 },            // Taux élevé (bien de luxe)
            { ECategorieImport.BienConsommation, 0.20 },    // Standard
            { ECategorieImport.MatierePremiere, 0.10 }      // Réduit (intrants)
        };

        /// <summary>
        /// Coefficients d'accise par catégorie d'importation.
        /// Appliqués aux biens spécifiques (alcool, tabac, carburant).
        /// </summary>
        private static readonly Dictionary<ECategorieImport, double> CoefficientsAccise = new()
        {
            { ECategorieImport.Carburant, 0.15 },           // Accise forte
            { ECategorieImport.Alimentaire, 0.00 },         // Pas d'accise
            { ECategorieImport.Electronique, 0.00 },
            { ECategorieImport.Vehicule, 0.05 },            // Accise modérée
            { ECategorieImport.BienConsommation, 0.02 },
            { ECategorieImport.MatierePremiere, 0.00 }
        };

        /// <summary>
        /// Coefficients de taxe à l'export selon la catégorie.
        /// Certaines exportations stratégiques bénéficient d'incitations.
        /// </summary>
        private static readonly Dictionary<ECategorieExport, double> CoefficientsTaxeExport = new()
        {
            { ECategorieExport.BiensAlimentaires, 1.0 },
            { ECategorieExport.Vanille, 1.5 },              // Fortement taxée (produit premium)
            { ECategorieExport.Crevettes, 1.1 },
            { ECategorieExport.Cafe, 1.0 },
            { ECategorieExport.Girofle, 1.2 },
            { ECategorieExport.ProduitsMiniers, 1.0 },
            { ECategorieExport.ZonesFranches, 0.5 }         // Régime fiscal favorable
        };

        public double GetProductiviteParSecteur(ESecteurActivite secteur)
        {
            return ProductiviteParSecteur.GetValueOrDefault(secteur, 15_000d);
        }

        public double GetTresorerieInitialeParSecteur(ESecteurActivite secteur)
        {
            return TresorerieInitialeParSecteur.GetValueOrDefault(secteur, 5_000_000d);
        }

        public double GetCoefficientDroitsDouaneParCategorie(ECategorieImport categorie)
        {
            return CoefficientsDroitsDouane.GetValueOrDefault(categorie, 0.15d);
        }

        public double GetCoefficientAcciseParCategorie(ECategorieImport categorie)
        {
            return CoefficientsAccise.GetValueOrDefault(categorie, 0.0d);
        }

        public Models.ESecteurActivite GetSecteurDepusCategorieExport(ECategorieExport categorie)
        {
            return categorie switch
            {
                ECategorieExport.ProduitsMiniers => ESecteurActivite.SecteurMinier,
                ECategorieExport.ZonesFranches => ESecteurActivite.Textiles,
                _ => ESecteurActivite.Commerces
            };
        }

        public double GetCoefficientTaxeExportParCategorie(ECategorieExport categorie)
        {
            return CoefficientsTaxeExport.GetValueOrDefault(categorie, 1.0d);
        }

        public CompanyDailyResult SimulerJournee(
            Models.Company entreprise,
            double demandeConsommationMenages,
            double tauxIS,
            double tauxTVA,
            double tauxInflation,
            double tauxDirecteur,
            bool estJourOuvrable = true,
            Jirama? Jirama = null,
            double consoElecParEmployeKWhJour = 0,
            double tauxCNaPSPatronale = 0)
        {
            var result = new CompanyDailyResult();

            // Si l'entreprise ne travaille pas ce jour, seule l'électricité est facturée
            if (!estJourOuvrable)
            {
                // Électricité Jirama (facturée tous les jours, même hors jour ouvrable)
                double kwhJourRepos = consoElecParEmployeKWhJour * entreprise.NombreEmployes * 0.3; // 30% en repos
                double facteurInflRepos = 1.0 + (tauxInflation / 365.0);
                double depElecRepos = Jirama != null ? kwhJourRepos * Jirama.PrixElectriciteArKWh * facteurInflRepos : 0;
                if (Jirama != null && kwhJourRepos > 0)
                {
                    Jirama.EnregistrerPaiementEntreprise(depElecRepos, kwhJourRepos, tauxTVA);
                }
                result.DepensesElectricite = depElecRepos;
                entreprise.Tresorerie -= depElecRepos;
                result.Tresorerie = entreprise.Tresorerie;
                return result;
            }

            // 1. Capacité de production du jour
            double productionJour = entreprise.NombreEmployes * entreprise.ProductiviteParEmployeJour;

            // 2. Ventes
            // B2C : limitée par la demande des ménages et la capacité de production
            double capaciteB2C = productionJour * (1.0 - entreprise.PartB2B);
            double ventesB2C = Math.Min(demandeConsommationMenages, capaciteB2C);

            // B2B : proportionnel à la production
            double ventesB2B = productionJour * entreprise.PartB2B;
            double ventesTotales = ventesB2C + ventesB2B;

            result.VentesB2C = ventesB2C;
            result.VentesB2B = ventesB2B;
            entreprise.ChiffreAffairesCumule += ventesTotales;

            // 3. TVA collectée sur les ventes (entreprises informelles exonérées)
            double tvaCollectee = entreprise.EstInformel ? 0 : ventesTotales * (tauxTVA / (1.0 + tauxTVA));
            result.TVACollectee = tvaCollectee;
            entreprise.TotalTVACollectee += tvaCollectee;

            // 4. Charges salariales journalières
            double chargesSalariales = (entreprise.NombreEmployes * entreprise.SalaireMoyenMensuel) / 30.0;
            result.ChargesSalariales = chargesSalariales;

            // 4b. Cotisations patronales CNaPS (entreprises formelles uniquement)
            double cotisationsCNaPS = entreprise.EstInformel ? 0 : chargesSalariales * tauxCNaPSPatronale;
            result.CotisationsCNaPS = cotisationsCNaPS;
            entreprise.TotalCotisationsCNaPS += cotisationsCNaPS;

            // 5. Coût des achats B2B (matières premières, services)
            double coutProduction = productionJour * (1.0 - entreprise.MargeBeneficiaire) - chargesSalariales;
            coutProduction = Math.Max(0, coutProduction);
            result.AchatsB2B = coutProduction * entreprise.PartB2B;
            entreprise.TotalAchatsB2B += result.AchatsB2B;

            // 5b. Électricité Jirama (facturée tous les jours si connecté)
            double facteurInflationJour = 1.0 + (tauxInflation / 365.0);
            double kwhEntreprise = consoElecParEmployeKWhJour * entreprise.NombreEmployes;
            double depensesElectricite = Jirama != null ? kwhEntreprise * Jirama.PrixElectriciteArKWh * facteurInflationJour : 0;
            if (Jirama != null && kwhEntreprise > 0)
            {
                Jirama.EnregistrerPaiementEntreprise(depensesElectricite, kwhEntreprise, tauxTVA);
            }
            result.DepensesElectricite = depensesElectricite;

            // 6. Bénéfice avant impôt
            double chargesTotales = chargesSalariales + cotisationsCNaPS + result.AchatsB2B + depensesElectricite;
            double beneficeAvantImpot = ventesTotales - tvaCollectee - chargesTotales;
            result.BeneficeAvantImpot = beneficeAvantImpot;

            entreprise.ChargesCumulees += chargesTotales;

            // 7. Impôt sur les sociétés (IS) — entreprises informelles exonérées
            double impotIS = (!entreprise.EstInformel && beneficeAvantImpot > 0) ? beneficeAvantImpot * tauxIS : 0;
            result.ImpotIS = impotIS;
            entreprise.TotalImpotsSociete += impotIS;

            // 8. Valeur ajoutée = Production - Consommations intermédiaires (achats B2B + électricité)
            double valeurAjoutee = ventesTotales - result.AchatsB2B - depensesElectricite;
            result.ValeurAjoutee = valeurAjoutee;

            // 9. Mise à jour de la trésorerie
            double fluxNet = ventesTotales - tvaCollectee - chargesTotales - impotIS;
            entreprise.Tresorerie += fluxNet;
            result.FluxNetJour = fluxNet;
            result.Tresorerie = entreprise.Tresorerie;

            // 10. Effet du taux directeur sur le coût du crédit (simplifié)
            // Un taux directeur élevé augmente le coût de financement
            double coutFinancementJour = Math.Max(0, entreprise.Tresorerie * -1) * (tauxDirecteur / 365.0);
            if (entreprise.Tresorerie < 0)
            {
                entreprise.Tresorerie -= coutFinancementJour;
                result.CoutFinancement = coutFinancementJour;
            }

            return result;
        }
    }
}
