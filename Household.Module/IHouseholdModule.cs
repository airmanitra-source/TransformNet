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
        /// Calcule les dépenses de loisirs du jour (sortie weekend ou vacances).
        /// Les ménages aisés réduisent les loisirs en priorité quand les prix augmentent,
        /// tandis que les ménages pauvres réduisent plutôt l'alimentaire.
        /// Les dépenses du ménage sont directement comptabilisées comme ventes de
        /// la compagnie hôtellerie/tourisme passée en paramètre.
        /// </summary>
        /// <param name="compagnieTourisme">
        /// Compagnie du secteur HotellerieTourisme choisie aléatoirement pour ce ménage.
        /// Ses ventes (ChiffreAffairesCumule, Tresorerie) sont incrémentées du montant dépensé.
        /// Null si aucune compagnie tourisme disponible → les dépenses ne sont pas comptabilisées.
        /// </param>
        /// <returns>
        /// DepensesLoisirs : montant dépensé (=ventes pour la compagnie tourisme),
        /// FacteurReduction : facteur appliqué (0-1) pour le suivi,
        /// EstEnSortie : true si sortie weekend,
        /// EstEnVacances : true si vacances.
        /// </returns>
        (double DepensesLoisirs, double FacteurReduction, bool EstEnSortie, bool EstEnVacances)
            CalculerDepensesLoisirs(
                Models.ClasseSocioEconomique classe,
                double budgetSortieWeekend,
                double budgetVacances,
                double probabiliteSortieWeekend,
                double probabiliteVacances,
                bool estWeekend,
                bool estPeriodeVacances,
                bool estEnVacancesEnCours,
                double cumulHaussePrix,
                double revenuDisponible,
                Company.Module.Models.Company? compagnieTourisme,
                Random random);

        /// <summary>
        /// Simule une journée de comportement économique pour un ménage.
        /// </summary>
        DailyHouseholdResult SimulerJournee(
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
            Random? random = null);
    }
}
