namespace Economic.Risk.Module.Models.Data;

/// <summary>Contrat de lecture des paramètres de risque cyclonique.</summary>
public interface ICycloneReadModel
{
    int Id { get; }
    int ScenarioId { get; }
    double ProbabiliteCycloneJourSaison { get; }
    double ProbabiliteCycloneJourHorsSaison { get; }
    int DureeCycloneJoursMin { get; }
    int DureeCycloneJoursMax { get; }
    double BudgetTotalReconstructionBase { get; }
    int DureeReconstructionJoursMin { get; }
    int DureeReconstructionJoursMax { get; }
    double PartMenagesAffectesMin { get; }
    double PartMenagesAffectesMax { get; }
    int DelaiMinEntreDeuxCyclones { get; }
    DateTime MisAJourAt { get; }
}

/// <summary>Contrat d'écriture pour configurer les paramètres cyclone.</summary>
public interface ICycloneWriteModel
{
    int ScenarioId { get; }
    double ProbabiliteCycloneJourSaison { get; }
    double ProbabiliteCycloneJourHorsSaison { get; }
    int DureeCycloneJoursMin { get; }
    int DureeCycloneJoursMax { get; }
    double BudgetTotalReconstructionBase { get; }
    int DureeReconstructionJoursMin { get; }
    int DureeReconstructionJoursMax { get; }
    double PartMenagesAffectesMin { get; }
    double PartMenagesAffectesMax { get; }
    int DelaiMinEntreDeuxCyclones { get; }
}

public record CycloneReadModel(
    int Id, int ScenarioId,
    double ProbabiliteCycloneJourSaison, double ProbabiliteCycloneJourHorsSaison,
    int DureeCycloneJoursMin, int DureeCycloneJoursMax,
    double BudgetTotalReconstructionBase,
    int DureeReconstructionJoursMin, int DureeReconstructionJoursMax,
    double PartMenagesAffectesMin, double PartMenagesAffectesMax,
    int DelaiMinEntreDeuxCyclones,
    DateTime MisAJourAt) : ICycloneReadModel;

public record CycloneWriteModel(
    int ScenarioId,
    double ProbabiliteCycloneJourSaison, double ProbabiliteCycloneJourHorsSaison,
    int DureeCycloneJoursMin, int DureeCycloneJoursMax,
    double BudgetTotalReconstructionBase,
    int DureeReconstructionJoursMin, int DureeReconstructionJoursMax,
    double PartMenagesAffectesMin, double PartMenagesAffectesMax,
    int DelaiMinEntreDeuxCyclones) : ICycloneWriteModel;
