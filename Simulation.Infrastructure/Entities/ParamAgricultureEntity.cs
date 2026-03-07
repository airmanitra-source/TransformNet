namespace Simulation.Infrastructure.Entities;

/// <summary>Paramètres du module sécheresse agricole (Kere du Grand Sud).</summary>
public class ParamAgricultureEntity
{
    public int Id { get; set; }
    public int ScenarioId { get; set; }

    public double ProbabiliteSecheresseJourSaison { get; set; } = 0.001;
    public double PartMenagesRurauxAffectes { get; set; } = 0.08;
    public int DureeSecheresseJoursBase { get; set; } = 120;
    public double ReductionProductionAgricole { get; set; } = 0.60;
    public double AideAlimentaireJourParMenage { get; set; } = 3_000;
    public double ProbabiliteMigrationSaison { get; set; } = 0.12;
    public double ValeurAutoconsommationJourBase { get; set; } = 2_500;

    public DateTime MisAJourAt { get; set; }
}
