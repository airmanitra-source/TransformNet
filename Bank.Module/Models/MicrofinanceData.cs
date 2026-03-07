namespace Bank.Module.Models;

/// <summary>
/// Données agrégées du secteur de la microfinance et des tontines informelles.
/// Complète le modèle bancaire pour capturer la segmentation du crédit à Madagascar :
///   - Crédit bancaire formel : ~10% des ménages, taux 14-18%
///   - Microfinance (IMF) : ~25% des ménages, taux 24-36%
///   - Tontines informelles : ~30% des ménages
///   - Exclus du crédit : ~35% des ménages
/// Source : BCM rapport inclusion financière 2024, CNFI Madagascar.
/// </summary>
public class MicrofinanceData
{
    // ═══════════════════════════════════════════════════════════════
    //  MICROFINANCE (IMF)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Encours total des crédits microfinance (MGA).</summary>
    public double EncoursCreditMicrofinance { get; set; }

    /// <summary>Nombre de crédits microfinance actifs.</summary>
    public int NbCreditsMicrofinanceActifs { get; set; }

    /// <summary>Crédits microfinance accordés ce jour (MGA).</summary>
    public double CreditsMicrofinanceJour { get; set; }

    /// <summary>Intérêts perçus sur les crédits microfinance ce jour (MGA).</summary>
    public double InteretsMicrofinanceJour { get; set; }

    /// <summary>Intérêts microfinance cumulés depuis le début (MGA).</summary>
    public double InteretsMicrofinanceCumules { get; set; }

    /// <summary>
    /// Taux d'intérêt annuel de la microfinance (24-36%).
    /// CNFI Madagascar : taux effectif global moyen ~30%.
    /// </summary>
    public double TauxInteretMicrofinanceAnnuel { get; set; } = 0.30;

    /// <summary>
    /// Taux de NPL de la microfinance (~5-8%).
    /// Meilleur que le bancaire grâce au suivi de proximité.
    /// </summary>
    public double RatioNPLMicrofinance { get; set; }

    // ═══════════════════════════════════════════════════════════════
    //  TONTINES INFORMELLES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Encours total des tontines informelles (MGA).</summary>
    public double EncoursTontines { get; set; }

    /// <summary>Nombre de tontines actives.</summary>
    public int NbTontinesActives { get; set; }

    /// <summary>Montant mobilisé par les tontines ce jour (MGA).</summary>
    public double MobilisationTontinesJour { get; set; }

    /// <summary>Montant mobilisé cumulé des tontines (MGA).</summary>
    public double MobilisationTontinesCumulees { get; set; }
}
