namespace Simulation.Infrastructure.Entities;

/// <summary>Paramètres du module Prix (carburant, élasticités).</summary>
public class ParamPrixEntity
{
    public int Id { get; set; }
    public int ScenarioId { get; set; }

    public double PrixCarburantLitre { get; set; } = 5_500;
    public double PrixCarburantReference { get; set; } = 5_500;
    public double ElasticitePrixParCarburant { get; set; } = 0.70;
    public double VolatiliteAleatoireMarche { get; set; } = 0.10;
    public double PartRevenuAlimentaire { get; set; } = 0.40;
    public double ElasticiteComportementMenage { get; set; } = 0.65;

    public DateTime MisAJourAt { get; set; }
}
