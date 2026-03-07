namespace Price.Module.Models.Data;

/// <summary>Contrat de lecture des paramètres du taux de change dynamique.</summary>
public interface ITauxChangeReadModel
{
    int Id { get; }
    int ScenarioId { get; }
    bool TauxChangeDynamiqueActive { get; }
    double TauxChangeMGAParUSD { get; }
    double ReservesBCMUSD { get; }
    double ElasticiteChangeBalanceCommerciale { get; }
    double PoidsChangePPA { get; }
    double IntensiteInterventionBCM { get; }
    double ReservesMinimalesMoisImports { get; }
    double DepreciationStructurelleAnnuelle { get; }
    double InflationEtrangere { get; }
    double ElasticiteRemittancesChange { get; }
    DateTime MisAJourAt { get; }
}

/// <summary>Contrat d'écriture pour configurer le taux de change.</summary>
public interface ITauxChangeWriteModel
{
    int ScenarioId { get; }
    bool TauxChangeDynamiqueActive { get; }
    double TauxChangeMGAParUSD { get; }
    double ReservesBCMUSD { get; }
    double ElasticiteChangeBalanceCommerciale { get; }
    double PoidsChangePPA { get; }
    double IntensiteInterventionBCM { get; }
    double ReservesMinimalesMoisImports { get; }
    double DepreciationStructurelleAnnuelle { get; }
    double InflationEtrangere { get; }
    double ElasticiteRemittancesChange { get; }
}

public record TauxChangeReadModel(
    int Id, int ScenarioId,
    bool TauxChangeDynamiqueActive, double TauxChangeMGAParUSD, double ReservesBCMUSD,
    double ElasticiteChangeBalanceCommerciale, double PoidsChangePPA,
    double IntensiteInterventionBCM, double ReservesMinimalesMoisImports,
    double DepreciationStructurelleAnnuelle, double InflationEtrangere,
    double ElasticiteRemittancesChange,
    DateTime MisAJourAt) : ITauxChangeReadModel;

public record TauxChangeWriteModel(
    int ScenarioId,
    bool TauxChangeDynamiqueActive, double TauxChangeMGAParUSD, double ReservesBCMUSD,
    double ElasticiteChangeBalanceCommerciale, double PoidsChangePPA,
    double IntensiteInterventionBCM, double ReservesMinimalesMoisImports,
    double DepreciationStructurelleAnnuelle, double InflationEtrangere,
    double ElasticiteRemittancesChange) : ITauxChangeWriteModel;
