namespace Household.Education.Module.Models.Data;

/// <summary>Contrat de lecture des paramètres d'éducation.</summary>
public interface IEducationReadModel
{
    int Id { get; }
    int ScenarioId { get; }
    double NombreEnfantsMoyenParMenage { get; }
    double PartEnfantsScolarises { get; }
    int DureeDepenseEducationJours { get; }
    double CoutEducationJournalierParEnfant { get; }
    double PartFormelleDepenseEducation { get; }
    DateTime MisAJourAt { get; }
}

/// <summary>Contrat d'écriture pour configurer les paramètres éducation.</summary>
public interface IEducationWriteModel
{
    int ScenarioId { get; }
    double NombreEnfantsMoyenParMenage { get; }
    double PartEnfantsScolarises { get; }
    int DureeDepenseEducationJours { get; }
    double CoutEducationJournalierParEnfant { get; }
    double PartFormelleDepenseEducation { get; }
}

public record EducationReadModel(
    int Id, int ScenarioId,
    double NombreEnfantsMoyenParMenage, double PartEnfantsScolarises,
    int DureeDepenseEducationJours, double CoutEducationJournalierParEnfant,
    double PartFormelleDepenseEducation,
    DateTime MisAJourAt) : IEducationReadModel;

public record EducationWriteModel(
    int ScenarioId,
    double NombreEnfantsMoyenParMenage, double PartEnfantsScolarises,
    int DureeDepenseEducationJours, double CoutEducationJournalierParEnfant,
    double PartFormelleDepenseEducation) : IEducationWriteModel;
