namespace Company.Module.Models;

/// <summary>
/// Résultat journalier d'une entreprise.
/// </summary>
public class CompanyDailyResult
{
    public double VentesB2C { get; set; }
    public double VentesB2B { get; set; }
    public double ChargesSalariales { get; set; }
    /// <summary>Cotisations patronales CNaPS ce jour (MGA, 18% du salaire brut, formel uniquement)</summary>
    public double CotisationsCNaPS { get; set; }
    public double AchatsB2B { get; set; }
    /// <summary>Dépenses d'électricité Jirama ce jour (MGA, TTC)</summary>
    public double DepensesElectricite { get; set; }
    public double TVACollectee { get; set; }
    public double ImpotIS { get; set; }
    public double BeneficeAvantImpot { get; set; }
    public double FluxNetJour { get; set; }
    public double Tresorerie { get; set; }
    public double CoutFinancement { get; set; }

    /// <summary>
    /// Valeur ajoutée = Production (VentesB2C + VentesB2B) - Consommations intermédiaires (AchatsB2B).
    /// Représente la richesse créée par l'entreprise.
    /// </summary>
    public double ValeurAjoutee { get; set; }
}



