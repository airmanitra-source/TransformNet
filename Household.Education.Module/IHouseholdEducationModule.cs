using Household.Education.Module.Models;

namespace Household.Education.Module;

public interface IHouseholdEducationModule
{
    EducationExpenseResult CalculerDepenseEducation(
        Household.Module.Models.Household menage,
        int jourCourant,
        double coutJournalierParEnfant,
        double tauxInflation,
        double partFormelle,
        double facteurReductionBudget,
        Random random);
}
