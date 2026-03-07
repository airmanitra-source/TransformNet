namespace Household.Salary.Distribution.Module.Models.Data;

/// <summary>Contrat de lecture des paramètres de distribution salariale.</summary>
public interface IDistributionSalarialeReadModel
{
    int Id { get; }
    int ScenarioId { get; }
    double SalaireMedian { get; }
    double Sigma { get; }
    double SalairePlancher { get; }
    double SalairePlafond { get; }
    double PartSecteurInformel { get; }
    DateTime MisAJourAt { get; }
}

/// <summary>Contrat d'écriture pour configurer la distribution salariale.</summary>
public interface IDistributionSalarialeWriteModel
{
    int ScenarioId { get; }
    double SalaireMedian { get; }
    double Sigma { get; }
    double SalairePlancher { get; }
    double SalairePlafond { get; }
    double PartSecteurInformel { get; }
}

public record DistributionSalarialeReadModel(
    int Id,
    int ScenarioId,
    double SalaireMedian,
    double Sigma,
    double SalairePlancher,
    double SalairePlafond,
    double PartSecteurInformel,
    DateTime MisAJourAt) : IDistributionSalarialeReadModel;

public record DistributionSalarialeWriteModel(
    int ScenarioId,
    double SalaireMedian,
    double Sigma,
    double SalairePlancher,
    double SalairePlafond,
    double PartSecteurInformel) : IDistributionSalarialeWriteModel;
