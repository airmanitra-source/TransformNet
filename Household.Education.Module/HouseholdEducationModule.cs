using Household.Education.Module.Models;
using Household.Module.Models;

namespace Household.Education.Module;

public class HouseholdEducationModule : IHouseholdEducationModule
{
    public EducationExpenseResult CalculerDepenseEducation(
        Household.Module.Models.Household menage,
        int jourCourant,
        double coutJournalierParEnfant,
        double tauxInflation,
        double partFormelle,
        double facteurReductionBudget,
        Random random)
    {
        var result = new EducationExpenseResult
        {
            NombreEnfantsScolarises = menage.NombreEnfantsScolarises,
            DureeDepenseJours = menage.DureeDepenseEducationJours,
            EstPeriodeDepense = menage.NombreEnfantsScolarises > 0
                && menage.DureeDepenseEducationJours > 0
                && jourCourant <= menage.DureeDepenseEducationJours
        };

        if (!result.EstPeriodeDepense)
        {
            return result;
        }

        double inflationJour = 1.0 + (tauxInflation / 365.0);
        double modulationClasse = menage.Classe switch
        {
            ClasseSocioEconomique.Subsistance => 0.65,
            ClasseSocioEconomique.InformelBas => 0.80,
            ClasseSocioEconomique.FormelBas => 1.00,
            ClasseSocioEconomique.FormelQualifie => 1.20,
            ClasseSocioEconomique.Cadre => 1.45,
            _ => 1.0
        };

        double depense = result.NombreEnfantsScolarises
            * coutJournalierParEnfant
            * inflationJour
            * modulationClasse
            * Math.Clamp(facteurReductionBudget, 0.35, 1.50);

        result.DepenseTotale = depense;
        result.PartFormelle = depense * Math.Clamp(partFormelle, 0.0, 1.0);
        result.PartInformelle = depense - result.PartFormelle;

        return result;
    }
}
