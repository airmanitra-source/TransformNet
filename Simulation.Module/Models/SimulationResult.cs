using Simulation.Module.Config;

namespace Simulation.Module.Models;

public class SimulationResult
{
    public ScenarioConfig Scenario { get; set; } = new();
    public List<DailySnapshot> Snapshots { get; set; } = [];
    public int JoursSimules { get; set; }
    public bool EstTerminee { get; set; }
    public bool EnCours { get; set; }
    public double EpargneFinale => Snapshots.Count > 0 ? Snapshots[^1].EpargneTotaleMenages : 0;
    public double RecettesFiscalesFinales => Snapshots.Count > 0 ? Snapshots[^1].RecettesFiscalesTotales : 0;
    public double DettePubliqueFinale => Snapshots.Count > 0 ? Snapshots[^1].DettePublique : 0;
    public double PIBFinal => Snapshots.Count > 0 ? Snapshots[^1].PIBProxy : 0;

    /// <summary>Journal des recalibrations mensuelles appliquées pendant la simulation.</summary>
    public List<CalibrationEvent> CalibrationEvents { get; set; } = [];
}




