namespace Simulation.Infrastructure.Entities;

/// <summary>Paramètres des chocs cycloniques stochastiques.</summary>
public class ParamCycloneEntity
{
    public int Id { get; set; }
    public int ScenarioId { get; set; }

    public double ProbabiliteCycloneJourSaison { get; set; } = 0.003;
    public double ProbabiliteCycloneJourHorsSaison { get; set; } = 0.0002;
    public int DureeCycloneJoursMin { get; set; } = 3;
    public int DureeCycloneJoursMax { get; set; } = 7;
    public double BudgetTotalReconstructionBase { get; set; } = 650_000;
    public int DureeReconstructionJoursMin { get; set; } = 45;
    public int DureeReconstructionJoursMax { get; set; } = 120;
    public double PartMenagesAffectesMin { get; set; } = 0.05;
    public double PartMenagesAffectesMax { get; set; } = 0.30;
    public int DelaiMinEntreDeuxCyclones { get; set; } = 30;

    public DateTime MisAJourAt { get; set; }
}
