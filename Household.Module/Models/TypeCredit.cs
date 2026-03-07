namespace Household.Module.Models;

/// <summary>
/// Type de crédit auquel un ménage a accès.
/// Segmentation du marché du crédit à Madagascar :
///   - Bancaire formel : ~10% des ménages (taux 14-18%)
///   - Microfinance (IMF) : ~25% des ménages (taux 24-36%)
///   - Tontine informelle : ~30% des ménages (taux variable, souvent 0% mais accès limité)
///   - Aucun : ~35% des ménages (exclus du crédit)
/// Source : BCM rapport inclusion financière 2024, CNFI Madagascar.
/// </summary>
public enum TypeCredit
{
    /// <summary>Pas d'accès au crédit (~35% des ménages)</summary>
    Aucun = 0,

    /// <summary>Crédit bancaire formel (~10% des ménages, taux 14-18%)</summary>
    BancaireFormel = 1,

    /// <summary>Microfinance / IMF (~25% des ménages, taux 24-36%)</summary>
    Microfinance = 2,

    /// <summary>Tontine informelle (~30% des ménages, taux ~0% mais montants limités)</summary>
    TontineInformelle = 3
}
