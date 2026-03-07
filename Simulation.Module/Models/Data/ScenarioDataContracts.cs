namespace Simulation.Module.Models.Data;

/// <summary>Contrat de lecture d'un scénario depuis la base de données.</summary>
public interface IScenarioReadModel
{
    int Id { get; }
    string Nom { get; }
    string Description { get; }
    int Version { get; }
    bool EstBase { get; }
    bool EstActif { get; }
    string CreePar { get; }
    DateTime CreeAt { get; }
    DateTime MisAJourAt { get; }
}

/// <summary>Contrat d'écriture pour créer ou mettre à jour un scénario.</summary>
public interface IScenarioWriteModel
{
    string Nom { get; }
    string Description { get; }
    bool EstBase { get; }
    string CreePar { get; }
}

public record ScenarioReadModel(
    int Id,
    string Nom,
    string Description,
    int Version,
    bool EstBase,
    bool EstActif,
    string CreePar,
    DateTime CreeAt,
    DateTime MisAJourAt) : IScenarioReadModel;

public record ScenarioWriteModel(
    string Nom,
    string Description,
    bool EstBase = false,
    string CreePar = "user") : IScenarioWriteModel;
