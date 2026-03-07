using Company.Module.Models;

namespace Household.Module
{
    /// <summary>
    /// Contrat d'exposition des services métier du module Ménages.
    /// Encapsule les règles de comportement économique des ménages par classe socio-économique.
    /// La distribution salariale est gérée par <c>IHouseholdSalaryDistributionModule</c>.
    /// </summary>
    public interface IHouseholdModule
    {
        /// <summary>
        /// Configure les comportements par classe socio-économique depuis la base de données.
        /// Remplace les magic numbers hardcodés dans <c>GetComportementParClasse</c>.
        /// Si non appelé, les valeurs par défaut historiques sont utilisées.
        /// </summary>
        void ConfigurerComportements(IEnumerable<Models.ComportementClasseConfig> configs);

        /// <summary>
        /// Configure les paramètres d'achat alimentaire depuis la base de données.
        /// </summary>
        void ConfigurerParametresAchat(double partInformelAlimentaire, double partFormelAlimentaire, double tauxTVAFormel);

        /// <summary>
        /// Retourne le comportement économique (taux d'épargne, propension conso, dépenses)
        /// caractéristique d'une classe socio-économique.
        /// </summary>
        (double TauxEpargne, double PropensionConsommation, double DepensesAlimentairesJour,
         double DepensesDiversJour, string Transport, double DistanceDomicileTravailKm,
         double EpargneInitiale) GetComportementParClasse(Models.ClasseSocioEconomique classe);

        /// <summary>
        /// Simule l'achat de produits alimentaires avec répartition informel/formel.
        /// Gère la réduction de quantités en cas d'augmentations répétitives de prix.
        /// </summary>
        (double CoutTotal, double CoutInformel, double CoutFormel, double QuantiteReduite)
            AcheteProduitsAlimentaires(
                double depenseAlimentairesJourBase,
                double cumulHaussePrixAlimentaire,
                double elasticiteUtilisateur,
                double revenuDisponible);

        /// <summary>
        /// Simule une journée de comportement économique pour un ménage.
        /// </summary>
        Models.DailyHouseholdResult SimulerJournee(
            Models.Household menage,
            double impotIRSAJournalier,
            double tauxEffectifIRSA,
            double tauxInflation,
            double tauxTVA,
            double prixCarburant,
            Jirama? Jirama,
            bool estJourDeTravail = true,
            int jourCourant = 1,
            double coutTransportJirama = 0,
            double prixCarburantReference = 5_500,
            double elasticitePrixParCarburant = 0.70,
            double volatiliteAleatoireMarche = 0.10,
            double elasticiteComportementMenage = 0.65,
            double partRevenuAlimentaireNormale = 0.40,
            double depensesEducation = 0,
            double depensesSante = 0,
            double depensesLoyerLocatif = 0,
            double depensesConstructionMaison = 0,
            double depensesConstructionBTP = 0,
            double depensesConstructionQuincaillerie = 0,
            double depensesConstructionTransportInformel = 0,
            Random? random = null);
    }
}
