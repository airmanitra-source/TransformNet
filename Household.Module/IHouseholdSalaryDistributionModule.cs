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

        Household.Module.Models.ClasseSocioEconomique DeterminerClasse(double salaireMensuel);

        Household.Module.Models.HouseholdBehavior GetComportementParClasse(
            Household.Module.Models.ClasseSocioEconomique classe,
            Random random);

        Household.Module.Models.DistributionStats CalculerStats(double[] valeurs);
    }
}
