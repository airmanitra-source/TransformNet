using Household.Module.Models;

namespace Household.Leisure.Spending.Module
{
    public interface IHouseholdLeisureSpendingModule
    {
        (double DepensesLoisirs, double FacteurReduction, bool EstEnSortie, bool EstEnVacances)
            CalculerDepensesLoisirs(
                ClasseSocioEconomique classe,
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
    }
}
