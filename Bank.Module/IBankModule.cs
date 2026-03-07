using Company.Module.Models;
using Household.Module.Models;
using BankAgent = Bank.Module.Models.Bank;

namespace Bank.Module;

public interface IBankModule
{
    /// <summary>
    /// Calcule les dépôts totaux (épargne des ménages + trésorerie des entreprises),
    /// décompose en dépôts à vue / à terme, et recalcule les agrégats M0, M1, M2, M3.
    /// Inclut le multiplicateur monétaire et les contreparties (FA, crédit intérieur).
    /// </summary>
    void CalculerBilansBancaires(
        BankAgent banque,
        IEnumerable<Household.Module.Models.Household> menages,
        IEnumerable<Company.Module.Models.Company> entreprises,
        double partDepotsAVue,
        double partMonnaieCirculationDansM1,
        double ratioM3SurM2,
        double reservesBCMUSD,
        double tauxChangeMGAParUSD,
        double dettePublique);

    /// <summary>
    /// Calcule le SCB (Solde en Compte des Banques) et la liquidité bancaire.
    /// Le SCB = RO + Excédent. Affecté par les flux de FA et les interventions BFM.
    /// L'excédent (SCB-RO) contraint la capacité d'octroi de crédit.
    /// Source : BFM rapport conjoncturel trimestriel, tableau "Liquidité bancaire".
    /// </summary>
    void CalculerLiquiditeBancaire(
        BankAgent banque,
        double balanceCommercialeJour,
        double remittancesJour,
        double tauxChangeMGAParUSD,
        double intensiteInterventionBFM,
        double ratioExcedentCible,
        Random random);

    /// <summary>
    /// Simule la création de monnaie par l'octroi de crédits aux agents économiques.
    /// Utilise le multiplicateur monétaire : crédits potentiels = M0 × (1/r - 1).
    /// </summary>
    void SimulerOctroiCredit(
        BankAgent banque,
        IEnumerable<Company.Module.Models.Company> entreprises,
        IEnumerable<Household.Module.Models.Household> menages,
        double tauxCroissanceCreditJour,
        double partCreditEntreprises,
        Random random);

    /// <summary>
    /// Calcule et distribue les intérêts : rémunération des dépôts et coût des crédits.
    /// Les intérêts sur dépôts sont crédités à l'épargne des ménages éligibles.
    /// Les intérêts sur crédits augmentent l'encours et alimentent la marge bancaire.
    /// </summary>
    void CalculerInterets(
        BankAgent banque,
        IEnumerable<Household.Module.Models.Household> menages,
        double tauxInteretDepotsAnnuel,
        double tauxInteretCreditsAnnuel);

    /// <summary>
    /// Simule les défauts de remboursement (NPL) et les recouvrements.
    /// Les NPL réduisent l'encours de crédits sains et augmentent les provisions.
    /// </summary>
    void SimulerNPL(
        BankAgent banque,
        double probabiliteDefautJour,
        double tauxRecouvrementJour,
        Random random);

    /// <summary>
    /// Simule le crédit segmenté : bancaire formel, microfinance (IMF), tontines informelles.
    /// Sépare le crédit bancaire (10% des ménages) de la microfinance (IMF, taux 24-36%)
    /// et des tontines informelles.
    /// Source : BCM rapport inclusion financière 2024, CNFI Madagascar.
    /// </summary>
    void SimulerCreditSegmente(
        BankAgent banque,
        IEnumerable<Household.Module.Models.Household> menages,
        double tauxInteretMicrofinanceAnnuel,
        double plafondCreditMicrofinance,
        double plafondTontine,
        double probabiliteOctroiMicrofinanceJour,
        Random random);
}
