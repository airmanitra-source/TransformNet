using Simulation.Module.Config;
using Simulation.Module.Models.Data;

namespace Simulation.Module;

/// <summary>
/// Contrat de chargement des scénarios depuis la persistance.
/// Implémenté par la couche infrastructure ; injecté dans <see cref="ISimulationModule"/>.
/// </summary>
public interface IScenarioConfigLoader
{
    /// <summary>Retourne la liste des scénarios disponibles.</summary>
    Task<IEnumerable<IScenarioReadModel>> ListScenariosAsync();

    /// <summary>
    /// Charge le <see cref="ScenarioConfig"/> complet pour un scénario donné.
    /// </summary>
    Task<ScenarioConfig> LoadAsync(int scenarioId);
}
