namespace Simulation.Module.Models;

/// <summary>
/// Rapport complet de validation macro d'une simulation.
/// Compare les agrégats simulés aux données de référence INSTAT/BCM/FMI
/// et produit un diagnostic quantifié avec scoring.
///
/// Le rapport est généré automatiquement à la fin de chaque simulation
/// (ou à la demande) et est stocké dans SimulationResult.ValidationReport.
///
/// Système de scoring :
///   ✅ Excellent : écart ≤ 5%
///   ☑️  Bon      : écart ≤ 15%
///   ⚠️  Moyen    : écart ≤ 30%
///   ❌ Mauvais  : écart > 30%
///
/// Score global = moyenne pondérée des scores individuels (0-100).
/// </summary>
public class MacroValidationReport
{
    /// <summary>Date de génération du rapport.</summary>
    public DateTime DateGeneration { get; set; } = DateTime.Now;

    /// <summary>Nombre de jours simulés au moment de la validation.</summary>
    public int JoursSimules { get; set; }

    /// <summary>Données de référence utilisées.</summary>
    public string SourceReference { get; set; } = "";

    /// <summary>Année de référence.</summary>
    public int AnneeReference { get; set; }

    /// <summary>
    /// Score global de validité (0-100).
    /// 90+ = modèle très fiable | 70-89 = fiable | 50-69 = approximatif | &lt;50 = à recalibrer.
    /// </summary>
    public double ScoreGlobal { get; set; }

    /// <summary>Verdict synthétique.</summary>
    public string Verdict { get; set; } = "";

    /// <summary>
    /// Indicateurs de validation individuels, triés par importance.
    /// </summary>
    public List<MacroValidationItem> Indicateurs { get; set; } = [];

    /// <summary>
    /// Alertes critiques : incohérences majeures détectées.
    /// </summary>
    public List<string> Alertes { get; set; } = [];

    /// <summary>
    /// Recommandations d'ajustement des paramètres.
    /// </summary>
    public List<string> Recommandations { get; set; } = [];
}

/// <summary>
/// Un indicateur individuel de validation macro.
/// Compare une grandeur simulée à une valeur de référence.
/// </summary>
public class MacroValidationItem
{
    /// <summary>Nom de l'indicateur (ex: "PIB annualisé").</summary>
    public string Nom { get; set; } = "";

    /// <summary>Catégorie (Production, Commerce, Monétaire, Fiscal, Inégalités, Énergie).</summary>
    public string Categorie { get; set; } = "";

    /// <summary>Valeur simulée (annualisée si simulation < 365j).</summary>
    public double ValeurSimulee { get; set; }

    /// <summary>Valeur de référence (données observées).</summary>
    public double ValeurReference { get; set; }

    /// <summary>Unité de mesure (MGA, %, ratio, GWh…).</summary>
    public string Unite { get; set; } = "MGA";

    /// <summary>Écart en % : (simulé - référence) / référence × 100.</summary>
    public double EcartPourcent => ValeurReference != 0
        ? (ValeurSimulee - ValeurReference) / ValeurReference * 100.0
        : 0;

    /// <summary>Écart absolu en %.</summary>
    public double EcartAbsoluPourcent => Math.Abs(EcartPourcent);

    /// <summary>
    /// Score individuel (0-100).
    ///   100 = parfait (écart 0%)
    ///   80+ = excellent (≤5%)
    ///   60+ = bon (≤15%)
    ///   40+ = moyen (≤30%)
    ///   &lt;40 = mauvais (>30%)
    /// </summary>
    public double Score { get; set; }

    /// <summary>
    /// Verdict visuel : ✅ ☑️ ⚠️ ❌
    /// </summary>
    public string Icone => EcartAbsoluPourcent switch
    {
        <= 5 => "✅",
        <= 15 => "☑️",
        <= 30 => "⚠️",
        _ => "❌"
    };

    /// <summary>
    /// Sens de l'écart : "sur-estimé" ou "sous-estimé".
    /// </summary>
    public string Sens => EcartPourcent >= 0 ? "sur-estimé" : "sous-estimé";

    /// <summary>Poids dans le score global (0-1). Les indicateurs PIB/fiscal pèsent plus.</summary>
    public double Poids { get; set; } = 1.0;

    /// <summary>
    /// Recommandation de paramètre à ajuster si l'écart est trop grand.
    /// </summary>
    public string? Recommandation { get; set; }
}
