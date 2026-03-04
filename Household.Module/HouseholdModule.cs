using Household.Module.Models;

namespace Household.Module
{
    /// <summary>
    /// Implémentation du module Ménages.
    /// Centralise le comportement économique par classe socio-économique
    /// et la logique d'achat alimentaire.
    /// La distribution salariale (tirage, classification, stats) est déléguée
    /// à <c>HouseholdSalaryDistributionModule</c>.
    /// </summary>
    public class HouseholdModule : IHouseholdModule
    {
        /// <remarks>
        /// NOTE DE CALIBRAGE : les valeurs de <c>DepensesAlimentairesJour</c> ici (15 000 MGA pour
        /// Subsistance) sont sensiblement différentes de celles de
        /// <c>HouseholdSalaryDistribution.ComportementParClasse</c> (2 000-3 500 MGA pour Subsistance),
        /// qui est la source effectivement utilisée à l'initialisation dans
        /// <c>EconomicSimulatorViewModel.Initialiser()</c>. À réconcilier.
        /// </remarks>
        public (double TauxEpargne, double PropensionConsommation, double DepensesAlimentairesJour,
                 double DepensesDiversJour, string Transport, double DistanceDomicileTravailKm,
                 double EpargneInitiale) GetComportementParClasse(ClasseSocioEconomique classe)
        {
            return classe switch
            {
                ClasseSocioEconomique.Subsistance => (
                    TauxEpargne: 0.02,
                    PropensionConsommation: 0.98,
                    DepensesAlimentairesJour: 15_000,
                    DepensesDiversJour: 2_000,
                    Transport: "à pied",
                    DistanceDomicileTravailKm: 2,
                    EpargneInitiale: 5_000
                ),
                ClasseSocioEconomique.InformelBas => (
                    TauxEpargne: 0.05,
                    PropensionConsommation: 0.90,
                    DepensesAlimentairesJour: 20_000,
                    DepensesDiversJour: 5_000,
                    Transport: "moto",
                    DistanceDomicileTravailKm: 5,
                    EpargneInitiale: 25_000
                ),
                ClasseSocioEconomique.FormelBas => (
                    TauxEpargne: 0.15,
                    PropensionConsommation: 0.80,
                    DepensesAlimentairesJour: 25_000,
                    DepensesDiversJour: 10_000,
                    Transport: "moto",
                    DistanceDomicileTravailKm: 8,
                    EpargneInitiale: 100_000
                ),
                ClasseSocioEconomique.FormelQualifie => (
                    TauxEpargne: 0.25,
                    PropensionConsommation: 0.70,
                    DepensesAlimentairesJour: 35_000,
                    DepensesDiversJour: 20_000,
                    Transport: "voiture",
                    DistanceDomicileTravailKm: 12,
                    EpargneInitiale: 500_000
                ),
                ClasseSocioEconomique.Cadre => (
                    TauxEpargne: 0.40,
                    PropensionConsommation: 0.55,
                    DepensesAlimentairesJour: 50_000,
                    DepensesDiversJour: 40_000,
                    Transport: "voiture",
                    DistanceDomicileTravailKm: 15,
                    EpargneInitiale: 2_000_000
                ),
                _ => (0.10, 0.80, 20_000, 5_000, "moto", 5, 50_000)
            };
        }

        /// <summary>
        /// Simule l'achat de produits alimentaires avec comportement réaliste.
        /// - 85 % auprès du secteur informel (prix bas, volatilité haute)
        /// - 15 % auprès du secteur formel (prix stable, TVA incluse)
        /// - Réduction progressive des quantités en cas d'augmentations répétitives
        /// </summary>
        public (double CoutTotal, double CoutInformel, double CoutFormel, double QuantiteReduite)
            AcheteProduitsAlimentaires(
                double depenseAlimentairesJourBase,
                double cumulHaussePrixAlimentaire,
                double elasticiteUtilisateur,
                double revenuDisponible)
        {
            double facteurReduction = CalculerFacteurReductionQuantites(cumulHaussePrixAlimentaire, elasticiteUtilisateur);

            double depenseEffective = depenseAlimentairesJourBase * facteurReduction;

            double coutInformel = depenseEffective * 0.85;
            double coutFormel   = depenseEffective * 0.15 * 1.20; // TVA 20 %

            double coutTotal = Math.Min(coutInformel + coutFormel, revenuDisponible * 0.80);

            return (coutTotal, coutInformel, coutFormel, facteurReduction);
        }

        /// <summary>
        /// Calcule la réduction de quantités achetées suite à des augmentations répétitives de prix.
        /// Courbe logistique : adaptation rapide → plateau (privation).
        /// </summary>
        private static double CalculerFacteurReductionQuantites(
            double cumulHaussePrix,
            double elasticiteUtilisateur)
        {
            if (cumulHaussePrix <= 0) return 1.0;

            double k  = 0.15 / Math.Max(elasticiteUtilisateur, 0.1);
            double x0 = 25.0;

            // Logistique inversée : plus l'inflation, moins on achète
            double facteur = 1.0 / (1.0 + Math.Exp(k * (cumulHaussePrix - x0)));

            return Math.Max(facteur, 0.30); // floor biologique
        }
    }
}
