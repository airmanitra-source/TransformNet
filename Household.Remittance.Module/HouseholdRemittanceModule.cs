namespace Household.Remittance.Module;

public class HouseholdRemittanceModule : IHouseholdRemittanceModule
{
    public double CalculerRemittanceParMenage(double remittancesJour, double facteurEchelle, int nombreMenages)
    {
        return (remittancesJour * facteurEchelle) / Math.Max(nombreMenages, 1);
    }

    public (double Remittance, double EpargneTotale) AppliquerRemittance(double epargneActuelle, double remittanceParMenage)
    {
        return (remittanceParMenage, epargneActuelle + remittanceParMenage);
    }
}
