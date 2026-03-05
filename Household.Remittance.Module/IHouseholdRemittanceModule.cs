namespace Household.Remittance.Module;

public interface IHouseholdRemittanceModule
{
    double CalculerRemittanceParMenage(double remittancesJour, double facteurEchelle, int nombreMenages);

    (double Remittance, double EpargneTotale) AppliquerRemittance(double epargneActuelle, double remittanceParMenage);
}
