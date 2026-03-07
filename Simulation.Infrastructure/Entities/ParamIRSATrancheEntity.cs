namespace Simulation.Infrastructure.Entities;

/// <summary>Tranche du barème IRSA pour un scénario donné.</summary>
public class ParamIRSATrancheEntity
{
    public int Id { get; set; }
    public int ScenarioId { get; set; }
    public int Ordre { get; set; }
    public double SeuilMin { get; set; }
    public double Taux { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime MisAJourAt { get; set; }
}
