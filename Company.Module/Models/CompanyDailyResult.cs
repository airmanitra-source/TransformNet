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

    /// <summary>
    /// Facteur de choc de prix appliqué ce jour (1.0 = aucun choc).
    /// Reflète la transmission du prix du carburant aux coûts et prix de l'entreprise.
    /// </summary>
    public double FacteurChocPrix { get; set; } = 1.0;

    // ─── Investissement productif (FBCF) ──────────────────────────────────────

    /// <summary>Investissement productif réalisé ce jour (MGA).</summary>
    public double InvestissementJour { get; set; }

    /// <summary>Dépréciation du capital ce jour (MGA).</summary>
    public double DepreciationCapitalJour { get; set; }

    /// <summary>Stock de capital en fin de journée (MGA).</summary>
    public double StockCapital { get; set; }

    /// <summary>Taux d'utilisation de la capacité de production (0-2+).</summary>
    public double TauxUtilisationCapacite { get; set; }
}



