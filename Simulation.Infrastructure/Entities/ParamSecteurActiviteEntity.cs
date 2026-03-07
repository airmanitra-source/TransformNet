namespace Simulation.Infrastructure.Entities;

/// <summary>Paramètres par secteur d'activité (productivité, trésorerie initiale).</summary>
public class ParamSecteurActiviteEntity
{
    public int Id { get; set; }
    public int ScenarioId { get; set; }
    /// <summary>ESecteurActivite ordinal (0=Agriculture..6=HotellerieTourisme)</summary>
    public int Secteur { get; set; }
    public string SecteurLibelle { get; set; } = string.Empty;

    public double ProductiviteJourMoyenne { get; set; }
    public double ProductiviteJourBasse { get; set; }
    public double TresorerieInitiale { get; set; }
    public int NombreEmployesDefaut { get; set; } = 10;

    public DateTime MisAJourAt { get; set; }
}
