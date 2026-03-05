namespace Company.Module
{
    /// <summary>
    /// Contrat d'exposition des services métier du module Entreprises.
    /// Encapsule les règles de calcul et comportements des agents économiques
    /// (entreprises, importateurs, exportateurs, Jirama).
    /// </summary>
    public interface ICompanyModule
    {
        /// <summary>
        /// Calcule la productivité journalière d'un employé selon son secteur d'activité.
        /// Calibrée sur les données INSTAT/SCN Madagascar.
        /// </summary>
        double GetProductiviteParSecteur(Models.ESecteurActivite secteur);

        /// <summary>
        /// Retourne la trésorerie initiale recommandée pour une entreprise d'un secteur donné.
        /// Représente le fonds de roulement minimal pour démarrer.
        /// </summary>
        double GetTresorerieInitialeParSecteur(Models.ESecteurActivite secteur);

        /// <summary>
        /// Retourne la trésorerie initiale en priorisant la configuration de scénario,
        /// avec fallback sur la grille standard par secteur.
        /// </summary>
        double GetTresorerieInitiale(
            Models.ESecteurActivite secteur,
            Dictionary<Models.ESecteurActivite, double> tresorerieInitialeParSecteurConfig);

        /// <summary>
        /// Calcule le coefficient de droits de douane spécifique à une catégorie d'importation.
        /// Permet une différenciation fiscale par type de bien.
        /// </summary>
        double GetCoefficientDroitsDouaneParCategorie(Models.ECategorieImport categorie);

        /// <summary>
        /// Calcule le coefficient d'accise spécifique à une catégorie d'importation.
        /// Appliqué aux biens sensibles (carburant, alcool, etc.).
        /// </summary>
        double GetCoefficientAcciseParCategorie(Models.ECategorieImport categorie);

        /// <summary>
        /// Retourne le secteur d'activité correspondant à une catégorie d'exportation.
        /// Mapping des catégories INSTAT vers les secteurs locaux.
        /// </summary>
        Models.ESecteurActivite GetSecteurDepusCategorieExport(Models.ECategorieExport categorie);

        /// <summary>
        /// Calcule le coefficient de taxe à l'export selon la catégorie de bien.
        /// Certaines zones franches bénéficient de régimes fiscaux favorables.
        /// </summary>
        double GetCoefficientTaxeExportParCategorie(Models.ECategorieExport categorie);

        /// <summary>
        /// Retourne le FOB total cumulé par catégorie d'export.
        /// </summary>
        Dictionary<Models.ECategorieExport, double> GetFOBParCategorie(IEnumerable<Models.Exporter> exportateurs);

        /// <summary>
        /// Retourne le FOB total cumulé tous exportateurs confondus.
        /// </summary>
        double GetFOBTotal(IEnumerable<Models.Exporter> exportateurs);

        /// <summary>
        /// Retourne le CIF total cumulé par catégorie d'import.
        /// </summary>
        Dictionary<Models.ECategorieImport, double> GetCIFParCategorie(IEnumerable<Models.Importer> importateurs);

        /// <summary>
        /// Simule une journée d'activité pour une entreprise.
        /// Intègre la transmission du choc de prix carburant sur les coûts d'approvisionnement
        /// et les prix de vente (pass-through partiel).
        /// </summary>
        Models.CompanyDailyResult SimulerJournee(
            Models.Company entreprise,
            double demandeConsommationMenages,
            double tauxIS,
            double tauxTVA,
            double tauxInflation,
            double tauxDirecteur,
            bool estJourOuvrable = true,
            Models.Jirama? Jirama = null,
            double consoElecParEmployeKWhJour = 0,
            double tauxCNaPSPatronale = 0,
            double prixCarburantCourant = 0,
            double prixCarburantReference = 0,
            double elasticitePrixParCarburant = 0);
    }
}
