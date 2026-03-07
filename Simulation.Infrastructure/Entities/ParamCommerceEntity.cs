namespace Simulation.Infrastructure.Entities;

/// <summary>Paramètres du commerce extérieur et des transferts diaspora.</summary>
public class ParamCommerceEntity
{
    public int Id { get; set; }
    public int ScenarioId { get; set; }

    public double RemittancesJour { get; set; } = 7_400_000_000;
    public double ConsommationRizAnnuelleKgParPersonne { get; set; } = 130;
    public double PrixRizLocalKg { get; set; } = 2_400;
    public double PrixRizImporteKg { get; set; } = 2_800;
    public double PartRizImporte { get; set; } = 0.18;

    public DateTime MisAJourAt { get; set; }
}
