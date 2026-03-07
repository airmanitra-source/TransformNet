namespace Simulation.Infrastructure.Entities;

/// <summary>Paramètres macroéconomiques globaux d'un scénario.</summary>
public class ParamMacroEntity
{
    public int Id { get; set; }
    public int ScenarioId { get; set; }

    public int DureeJours { get; set; } = 365;
    public int NombreMenages { get; set; } = 100_000;
    public int NombreEntreprises { get; set; } = 50_000;
    public double PartMenagesUrbains { get; set; } = 0.30;

    public double AideInternationaleJour { get; set; } = 3_704_000_000;
    public double SubventionJiramaJour { get; set; } = 1_370_000_000;
    public double DepensesCapitalJour { get; set; } = 13_526_000_000;
    public double InteretsDetteJour { get; set; } = 1_678_000_000;
    public double DettePubliqueInitiale { get; set; } = 29_250_000_000_000;
    public double DepensesPubliquesJour { get; set; } = 3_218_000_000;
    public double TauxRedistribution { get; set; } = 0.15;

    public int NombreFonctionnaires { get; set; } = 350_000;
    public double SalaireMoyenFonctionnaireMensuel { get; set; } = 863_000;

    public double TauxReinvestissementPrive { get; set; } = 0.25;
    public bool InvestissementProductifActive { get; set; } = true;
    public double TauxDepreciationCapitalAnnuel { get; set; } = 0.07;
    public double SeuilUtilisationInvestissement { get; set; } = 0.70;
    public double ElasticiteCapitalProductivite { get; set; } = 0.08;
    public bool InputOutputActivee { get; set; } = true;
    public double ValeurAutoconsommationJourBase { get; set; } = 2_500;

    public DateTime MisAJourAt { get; set; }
}
