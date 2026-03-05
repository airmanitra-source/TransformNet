using Simulation.Module.Config;
using AgentCompany = Company.Module.Models.Company;
using AgentImporter = Company.Module.Models.Importer;
using AgentExporter = Company.Module.Models.Exporter;
using AgentGovernment = Government.Module.Models.Government;

namespace Simulation.Module.Models;

/// <summary>
/// Paramètres d'entrée pour une recalibration mensuelle.
/// Regroupe tout le contexte nécessaire au <see cref="RecalibrationEngine"/>
/// pour comparer les valeurs simulées aux cibles observées et ajuster les paramètres.
/// </summary>
public class RecalibrationContext
{
    /// <summary>Jour courant de simulation (30, 60, 90…).</summary>
    public required int JourCourant { get; init; }

    /// <summary>Numéro du mois (1 = premier mois, 2 = deuxième…).</summary>
    public required int Mois { get; init; }

    /// <summary>Cible mensuelle issue des données macro observées (BCM, DGI, INSTAT).</summary>
    public required MonthlyCalibrationTarget Cible { get; init; }

    /// <summary>Snapshot du jour courant (valeurs simulées à comparer).</summary>
    public required DailySnapshot SnapshotActuel { get; init; }

    /// <summary>Agent État (pour les recettes fiscales cumulées).</summary>
    public required AgentGovernment Etat { get; init; }

    /// <summary>Configuration du scénario en cours (paramètres à ajuster).</summary>
    public required ScenarioConfig Config { get; init; }

    /// <summary>Liste de toutes les entreprises locales (hors importateurs/exportateurs).</summary>
    public required IReadOnlyList<AgentCompany> Entreprises { get; init; }

    /// <summary>Liste des agents importateurs.</summary>
    public required IReadOnlyList<AgentImporter> Importateurs { get; init; }

    /// <summary>Liste des agents exportateurs.</summary>
    public required IReadOnlyList<AgentExporter> Exportateurs { get; init; }

    /// <summary>
    /// Facteur d'échelle simulation / réalité.
    /// Les cibles macro sont à l'échelle réelle ; ce facteur les ramène
    /// à l'échelle de la simulation avant comparaison.
    /// </summary>
    public required double FacteurEchelle { get; init; }
}
