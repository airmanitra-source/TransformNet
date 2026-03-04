using Household.Module.Models;
using Company.Module.Models;
namespace Government.Module.Models;

/// <summary>
/// Tranche du barème IRSA (Impôt sur les Revenus Salariaux et Assimilés) de Madagascar.
/// Barème progressif par tranches marginales.
/// </summary>
public class TrancheIRSA
{
    /// <summary>Seuil minimum de la tranche (MGA/mois)</summary>
    public double SeuilMin { get; set; }

    /// <summary>Taux marginal de la tranche</summary>
    public double Taux { get; set; }

    /// <summary>Description de la tranche</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Barème IRSA officiel de Madagascar (Code Général des Impôts).
    /// Tranches mensuelles progressives.
    /// </summary>
    public static List<TrancheIRSA> BaremeMadagascar() =>
    [
        new() { SeuilMin = 0,       Taux = 0.00, Description = "Exonéré (≤ 350 000 MGA)" },
        new() { SeuilMin = 350_001, Taux = 0.05, Description = "5% (350 001 – 400 000)" },
        new() { SeuilMin = 400_001, Taux = 0.10, Description = "10% (400 001 – 500 000)" },
        new() { SeuilMin = 500_001, Taux = 0.15, Description = "15% (500 001 – 600 000)" },
        new() { SeuilMin = 600_001, Taux = 0.20, Description = "20% (> 600 000)" },
    ];
}


