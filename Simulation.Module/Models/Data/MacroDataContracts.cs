namespace Simulation.Module.Models.Data;

/// <summary>Contrat de lecture des paramètres macroéconomiques globaux.</summary>
public interface IMacroReadModel
{
    int Id { get; }
    int ScenarioId { get; }
    int DureeJours { get; }
    int NombreMenages { get; }
    int NombreEntreprises { get; }
    double PartMenagesUrbains { get; }
    double AideInternationaleJour { get; }
    double SubventionJiramaJour { get; }
    double DepensesCapitalJour { get; }
    double InteretsDetteJour { get; }
    double DettePubliqueInitiale { get; }
    double DepensesPubliquesJour { get; }
    double TauxRedistribution { get; }
    int NombreFonctionnaires { get; }
    double SalaireMoyenFonctionnaireMensuel { get; }
    double TauxReinvestissementPrive { get; }
    bool InvestissementProductifActive { get; }
    double TauxDepreciationCapitalAnnuel { get; }
    double SeuilUtilisationInvestissement { get; }
    double ElasticiteCapitalProductivite { get; }
    bool InputOutputActivee { get; }
    DateTime MisAJourAt { get; }
}

/// <summary>Contrat d'écriture pour configurer les paramètres macro.</summary>
public interface IMacroWriteModel
{
    int ScenarioId { get; }
    int DureeJours { get; }
    int NombreMenages { get; }
    int NombreEntreprises { get; }
    double PartMenagesUrbains { get; }
    double AideInternationaleJour { get; }
    double SubventionJiramaJour { get; }
    double DepensesCapitalJour { get; }
    double InteretsDetteJour { get; }
    double DettePubliqueInitiale { get; }
    double DepensesPubliquesJour { get; }
    double TauxRedistribution { get; }
    int NombreFonctionnaires { get; }
    double SalaireMoyenFonctionnaireMensuel { get; }
    double TauxReinvestissementPrive { get; }
    bool InvestissementProductifActive { get; }
    bool InputOutputActivee { get; }
}
