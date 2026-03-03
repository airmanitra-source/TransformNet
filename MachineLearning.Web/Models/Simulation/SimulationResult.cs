using MachineLearning.Web.Models.Simulation.Config;

namespace MachineLearning.Web.Models.Simulation;

/// <summary>
/// Résultat complet d'une simulation.
/// </summary>
public class SimulationResult
{
    public ScenarioConfig Scenario { get; set; } = new();
    public List<DailySnapshot> Snapshots { get; set; } = [];
    public int JoursSimules { get; set; }
    public bool EstTerminee { get; set; }
    public bool EnCours { get; set; }

    // --- Résumé final ---
    public double EpargneFinale => Snapshots.Count > 0 ? Snapshots[^1].EpargneTotaleMenages : 0;
    public double RecettesFiscalesFinales => Snapshots.Count > 0 ? Snapshots[^1].RecettesFiscalesTotales : 0;
    public double DettePubliqueFinale => Snapshots.Count > 0 ? Snapshots[^1].DettePublique : 0;
    public double PIBFinal => Snapshots.Count > 0 ? Snapshots[^1].PIBProxy : 0;
}
