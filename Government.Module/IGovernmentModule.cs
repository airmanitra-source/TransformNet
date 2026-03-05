namespace Government.Module
{
    public interface IGovernmentModule
    {
        /// <summary>
        /// Calcule l'IRSA mensuel selon le barème progressif malgache.
        /// L'impôt est calculé par tranches marginales.
        /// </summary>
        double CalculerIRSAMensuel(Models.Government government, double salaireMensuelBrut);
        double CalculerIRSAJournalier(Models.Government government, double salaireMensuelBrut);
        double TauxEffectifIRSA(Models.Government government, double salaireMensuelBrut);

        /// <summary>
        /// Simule une journée de consolidation budgétaire pour l'État.
        /// </summary>
        Models.DailyGovernmentResult SimulerJournee(
            Models.Government government,
            List<Household.Module.Models.DailyHouseholdResult> resultsMenages,
            List<Company.Module.Models.CompanyDailyResult> resultsEntreprises,
            List<Company.Module.Models.DailyImporterResult> resultsImportateurs,
            List<Company.Module.Models.DailyExporterResult> resultsExportateurs,
            Company.Module.Models.Jirama? Jirama = null,
            double consoElecEtatKWhJour = 0,
            double aideInternationaleJour = 0,
            double subventionJiramaJour = 0,
            double masseSalarialeFonctionnairesJour = 0,
            double tauxReinvestissementPrive = 0,
            double depensesCapitalJour = 0,
            double interetsDetteJour = 0);

        /// <summary>
        /// Réconcilie les approches du PIB (demande / valeur ajoutée / revenus)
        /// et retourne les agrégats macroéconomiques du jour.
        /// </summary>
        Models.PibComputationResult CalculerPIB(
            List<Household.Module.Models.DailyHouseholdResult> resultsMenages,
            List<Company.Module.Models.CompanyDailyResult> tousResultsEntreprises,
            List<Company.Module.Models.DailyImporterResult> resultsImportateurs,
            List<Company.Module.Models.DailyExporterResult> resultsExportateurs,
            Company.Module.Models.Jirama jirama,
            Models.DailyGovernmentResult resultEtat,
            int jourCourant);
    }
}