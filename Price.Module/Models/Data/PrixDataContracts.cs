namespace Price.Module.Models.Data;

/// <summary>Contrat de lecture des paramètres du module Prix.</summary>
public interface IPrixReadModel
{
    int Id { get; }
    int ScenarioId { get; }
    double PrixCarburantLitre { get; }
    double PrixCarburantReference { get; }
    double ElasticitePrixParCarburant { get; }
    double VolatiliteAleatoireMarche { get; }
    double PartRevenuAlimentaire { get; }
    double ElasticiteComportementMenage { get; }
    DateTime MisAJourAt { get; }
}

/// <summary>Contrat d'écriture pour configurer les paramètres Prix.</summary>
public interface IPrixWriteModel
{
    int ScenarioId { get; }
    double PrixCarburantLitre { get; }
    double PrixCarburantReference { get; }
    double ElasticitePrixParCarburant { get; }
    double VolatiliteAleatoireMarche { get; }
    double PartRevenuAlimentaire { get; }
    double ElasticiteComportementMenage { get; }
}

public record PrixReadModel(
    int Id, int ScenarioId,
    double PrixCarburantLitre, double PrixCarburantReference,
    double ElasticitePrixParCarburant, double VolatiliteAleatoireMarche,
    double PartRevenuAlimentaire, double ElasticiteComportementMenage,
    DateTime MisAJourAt) : IPrixReadModel;

public record PrixWriteModel(
    int ScenarioId,
    double PrixCarburantLitre, double PrixCarburantReference,
    double ElasticitePrixParCarburant, double VolatiliteAleatoireMarche,
    double PartRevenuAlimentaire, double ElasticiteComportementMenage) : IPrixWriteModel;
