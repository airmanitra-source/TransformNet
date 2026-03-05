using Company.Module.Models;
using Household.Module.Models;
using BankAgent = Bank.Module.Models.Bank;

namespace Bank.Module;

public interface IBankModule
{
    /// <summary>
    /// Calcule les dépôts totaux (épargne des ménages + trésorerie des entreprises) et estime M3.
    /// </summary>
    void CalculerBilansBancaires(BankAgent banque, IEnumerable<Household.Module.Models.Household> menages, IEnumerable<Company.Module.Models.Company> entreprises);

    /// <summary>
    /// Simule la création de monnaie par l'octroi de crédits aux agents économiques.
    /// Augmente la trésorerie des entreprises et l'épargne des ménages.
    /// </summary>
    void SimulerOctroiCredit(BankAgent banque, IEnumerable<Company.Module.Models.Company> entreprises, IEnumerable<Household.Module.Models.Household> menages, double tauxCroissanceCreditJour, Random random);
}
