using Household.HealthExpenditure.Module.Models;

namespace Household.HealthExpenditure.Module;

public interface IHouseholdHealthExpenditureModule
{
    HealthExpenseResult SimulerDepenseSante(
        Household.Module.Models.Household menage,
        double tauxOccupationHopitaux,
        double coutConsultationBase,
        double coutHospitalisationBase,
        double tauxInflation,
        double partFormelle,
        Random random);
}
