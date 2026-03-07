namespace Price.Module.Models.Data;

/// <summary>Contrat de lecture des paramètres du modèle d'inflation.</summary>
public interface IInflationReadModel
{
    int Id { get; }
    int ScenarioId { get; }
    double TauxInflationInitial { get; }
    bool InflationEndogeneActivee { get; }
    double NAIRU { get; }
    double CoefficientPhillips { get; }
    double ElasticiteCarburantInflation { get; }
    double ElasticiteImportInflation { get; }
    double ElasticiteChangeInflation { get; }
    double ElasticiteSalairesInflation { get; }
    double CoefficientMonetaire { get; }
    double PoidsAnticipationsAdaptatives { get; }
    double PoidsAncrageInflation { get; }
    DateTime MisAJourAt { get; }
}

/// <summary>Contrat d'écriture pour configurer le modèle d'inflation.</summary>
public interface IInflationWriteModel
{
    int ScenarioId { get; }
    double TauxInflationInitial { get; }
    bool InflationEndogeneActivee { get; }
    double NAIRU { get; }
    double CoefficientPhillips { get; }
    double ElasticiteCarburantInflation { get; }
    double ElasticiteImportInflation { get; }
    double ElasticiteChangeInflation { get; }
    double ElasticiteSalairesInflation { get; }
    double CoefficientMonetaire { get; }
    double PoidsAnticipationsAdaptatives { get; }
    double PoidsAncrageInflation { get; }
}

public record InflationReadModel(
    int Id, int ScenarioId,
    double TauxInflationInitial, bool InflationEndogeneActivee,
    double NAIRU, double CoefficientPhillips,
    double ElasticiteCarburantInflation, double ElasticiteImportInflation,
    double ElasticiteChangeInflation, double ElasticiteSalairesInflation,
    double CoefficientMonetaire,
    double PoidsAnticipationsAdaptatives, double PoidsAncrageInflation,
    DateTime MisAJourAt) : IInflationReadModel;

public record InflationWriteModel(
    int ScenarioId,
    double TauxInflationInitial, bool InflationEndogeneActivee,
    double NAIRU, double CoefficientPhillips,
    double ElasticiteCarburantInflation, double ElasticiteImportInflation,
    double ElasticiteChangeInflation, double ElasticiteSalairesInflation,
    double CoefficientMonetaire,
    double PoidsAnticipationsAdaptatives, double PoidsAncrageInflation) : IInflationWriteModel;
