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
            { ESecteurActivite.Construction, 40_000 }     // BTP calibré sur PIB
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
            { ESecteurActivite.Construction, 15_000_000 }
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
    }
}
