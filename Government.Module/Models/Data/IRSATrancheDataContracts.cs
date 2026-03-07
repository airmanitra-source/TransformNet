namespace Government.Module.Models.Data;

/// <summary>Contrat de lecture d'une tranche IRSA depuis la base de données.</summary>
public interface IIRSATrancheReadModel
{
    int Id { get; }
    int ScenarioId { get; }
    int Ordre { get; }
    double SeuilMin { get; }
    double Taux { get; }
    string Description { get; }
    DateTime MisAJourAt { get; }
}

/// <summary>Contrat d'écriture pour créer/modifier une tranche IRSA.</summary>
public interface IIRSATrancheWriteModel
{
    int ScenarioId { get; }
    int Ordre { get; }
    double SeuilMin { get; }
    double Taux { get; }
    string Description { get; }
}

public record IRSATrancheReadModel(
    int Id, int ScenarioId, int Ordre,
    double SeuilMin, double Taux, string Description,
    DateTime MisAJourAt) : IIRSATrancheReadModel;

public record IRSATrancheWriteModel(
    int ScenarioId, int Ordre,
    double SeuilMin, double Taux,
    string Description) : IIRSATrancheWriteModel;
