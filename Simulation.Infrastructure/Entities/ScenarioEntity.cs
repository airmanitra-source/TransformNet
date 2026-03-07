namespace Simulation.Infrastructure.Entities;

/// <summary>Entité représentant un scénario de simulation.</summary>
public class ScenarioEntity
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Version { get; set; } = 1;
    public bool EstBase { get; set; }
    public bool EstActif { get; set; } = true;
    public string CreePar { get; set; } = "system";
    public DateTime CreeAt { get; set; }
    public DateTime MisAJourAt { get; set; }
}
