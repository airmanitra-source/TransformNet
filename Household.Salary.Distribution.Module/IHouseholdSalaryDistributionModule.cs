using Household.Module.Models;

namespace Household.Salary.Distribution.Module
{
    /// <summary>
    /// Contrat du module de distribution salariale.
    /// Encapsule le tirage log-normal, la classification socio-économique
    /// et les statistiques agrégées d'une population de salaires.
    /// </summary>
    public interface IHouseholdSalaryDistributionModule
    {
        /// <summary>
        /// Tire un salaire aléatoire selon la distribution log-normale calibrée.
        /// Utilise l'algorithme Box-Muller pour générer une Gaussienne standard.
        /// </summary>
        double TirerSalaire(Random random);

        /// <summary>
        /// Détermine la classe socio-économique d'un ménage d'après son salaire mensuel.
        /// Les seuils sont les quintiles de la distribution log-normale configurée.
        /// </summary>
        ClasseSocioEconomique DeterminerClasse(double salaireMensuel);

        /// <summary>
        /// Calcule les statistiques agrégées (moyenne, médiane, Gini, D9/D1, quintiles)
        /// d'une série de salaires.
        /// </summary>
        DistributionStats CalculerStats(double[] valeurs);
    }
}