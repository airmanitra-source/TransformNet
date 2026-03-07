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
    /// <summary>Probabilité qu'une entreprise de ce secteur soit informelle (0-1).</summary>
    public double ProbabiliteInformel { get; set; } = 0.70;
    /// <summary>Marge bénéficiaire spécifique à ce secteur (override de MargeBeneficiaireEntreprise global).</summary>
    public double MargeBeneficiaire { get; set; } = 0.20;

    public DateTime MisAJourAt { get; set; }
}
