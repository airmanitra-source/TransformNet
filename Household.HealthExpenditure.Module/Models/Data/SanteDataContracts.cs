namespace Household.HealthExpenditure.Module.Models.Data;

/// <summary>Contrat de lecture des paramètres de dépenses de santé.</summary>
public interface ISanteReadModel
{
    int Id { get; }
    int ScenarioId { get; }
    double TauxOccupationHopitaux { get; }
    double CoutConsultationBase { get; }
    double CoutHospitalisationBase { get; }
    double PartFormelleDepenseSante { get; }
    double ProbabiliteHospitalisationBase { get; }
    DateTime MisAJourAt { get; }
}

/// <summary>Contrat d'écriture pour configurer les paramètres santé.</summary>
public interface ISanteWriteModel
{
    int ScenarioId { get; }
    double TauxOccupationHopitaux { get; }
    double CoutConsultationBase { get; }
    double CoutHospitalisationBase { get; }
    double PartFormelleDepenseSante { get; }
    double ProbabiliteHospitalisationBase { get; }
}

public record SanteReadModel(
    int Id, int ScenarioId,
    double TauxOccupationHopitaux, double CoutConsultationBase,
    double CoutHospitalisationBase, double PartFormelleDepenseSante,
    double ProbabiliteHospitalisationBase,
    DateTime MisAJourAt) : ISanteReadModel;

public record SanteWriteModel(
    int ScenarioId,
    double TauxOccupationHopitaux, double CoutConsultationBase,
    double CoutHospitalisationBase, double PartFormelleDepenseSante,
    double ProbabiliteHospitalisationBase) : ISanteWriteModel;
