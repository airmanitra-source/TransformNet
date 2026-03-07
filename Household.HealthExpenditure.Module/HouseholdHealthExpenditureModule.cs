using Household.HealthExpenditure.Module.Models;
using Household.Module.Models;

namespace Household.HealthExpenditure.Module;

public class HouseholdHealthExpenditureModule : IHouseholdHealthExpenditureModule
{
    public HealthExpenseResult SimulerDepenseSante(
        Household.Module.Models.Household menage,
        double tauxOccupationHopitaux,
        double coutConsultationBase,
        double coutHospitalisationBase,
        double tauxInflation,
        double partFormelle,
        Random random)
    {
        double occupationNormalisee = Math.Clamp(tauxOccupationHopitaux, 0.0, 1.0);
        double baseRisque = menage.Classe switch
        {
            ClasseSocioEconomique.Subsistance => 0.010,
            ClasseSocioEconomique.InformelBas => 0.008,
            ClasseSocioEconomique.FormelBas => 0.006,
            ClasseSocioEconomique.FormelQualifie => 0.005,
            ClasseSocioEconomique.Cadre => 0.004,
            _ => 0.006
        };

        double surchargeEnfants = menage.NombreEnfants * 0.0008;
        double probabiliteMaladie = Math.Clamp(baseRisque + surchargeEnfants + occupationNormalisee * 0.012, 0.0, 0.35);
        bool estMalade = random.NextDouble() < probabiliteMaladie;

        var result = new HealthExpenseResult
        {
            EstMalade = estMalade,
            ProbabiliteMaladie = probabiliteMaladie
        };

        if (!estMalade)
        {
            return result;
        }

        double inflationJour = 1.0 + (tauxInflation / 365.0);
        bool hospitalisation = random.NextDouble() < (0.10 + occupationNormalisee * 0.20);
        double coutBase = hospitalisation ? coutHospitalisationBase : coutConsultationBase;
        double multiplicateurClasse = menage.Classe switch
        {
            ClasseSocioEconomique.Subsistance => 0.70,
            ClasseSocioEconomique.InformelBas => 0.85,
            ClasseSocioEconomique.FormelBas => 1.00,
            ClasseSocioEconomique.FormelQualifie => 1.20,
            ClasseSocioEconomique.Cadre => 1.50,
            _ => 1.0
        };

        double depense = coutBase * inflationJour * multiplicateurClasse;
        result.DepenseTotale = depense;
        result.PartFormelle = depense * Math.Clamp(partFormelle, 0.0, 1.0);
        result.PartInformelle = depense - result.PartFormelle;

        return result;
    }
}
