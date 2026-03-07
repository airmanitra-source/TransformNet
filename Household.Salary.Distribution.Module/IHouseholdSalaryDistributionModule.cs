using Household.Module.Models;

namespace Household.Salary.Distribution.Module
{
    public interface IHouseholdSalaryDistributionModule
    {
        void ConfigurerDistributionSalariale(
            double salaireMedian,
            double sigma,
            double salairePlancher,
            double partSecteurInformel);

        /// <summary>
        /// Configure les comportements par classe socio-économique depuis la base de données.
        /// Remplace les magic numbers hardcodés dans <c>ComportementParClasse</c>.
        /// Si non appelé, les valeurs par défaut historiques sont utilisées.
        /// </summary>
        void ConfigurerComportements(IEnumerable<ComportementClasseConfig> configs);

        double TirerSalaire(Random random);

        ClasseSocioEconomique DeterminerClasse(double salaireMensuel);

        HouseholdBehavior GetComportementParClasse(
            ClasseSocioEconomique classe,
            Random random);

        DistributionStats CalculerStats(double[] valeurs);
    }
}
