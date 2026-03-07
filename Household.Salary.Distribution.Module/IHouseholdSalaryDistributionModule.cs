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

        double TirerSalaire(Random random);

        ClasseSocioEconomique DeterminerClasse(double salaireMensuel);

        HouseholdBehavior GetComportementParClasse(
            ClasseSocioEconomique classe,
            Random random);

        DistributionStats CalculerStats(double[] valeurs);
    }
}
