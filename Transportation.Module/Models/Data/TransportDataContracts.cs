namespace Transportation.Module.Models.Data;

/// <summary>Contrat de lecture des paramètres de transport.</summary>
public interface ITransportReadModel
{
    int Id { get; }
    int ScenarioId { get; }
    double PartInformelTransportPublic { get; }
    double PartFormelCarburant { get; }
    double PartInformelEntretien { get; }
    double CoutTaxiBe { get; }
    double EntretienVoitureJour { get; }
    double EntretienFractionRevenuVoiture { get; }
    double ConsoMotoLitrePour100km { get; }
    double ConsoVoitureLitrePour100km { get; }
    double CoutTransportPaiementJirama { get; }
    DateTime MisAJourAt { get; }
}

/// <summary>Contrat d'écriture pour configurer les paramètres transport.</summary>
public interface ITransportWriteModel
{
    int ScenarioId { get; }
    double PartInformelTransportPublic { get; }
    double PartFormelCarburant { get; }
    double PartInformelEntretien { get; }
    double CoutTaxiBe { get; }
    double EntretienVoitureJour { get; }
    double ConsoMotoLitrePour100km { get; }
    double ConsoVoitureLitrePour100km { get; }
    double CoutTransportPaiementJirama { get; }
}

public record TransportReadModel(
    int Id, int ScenarioId,
    double PartInformelTransportPublic, double PartFormelCarburant, double PartInformelEntretien,
    double CoutTaxiBe, double EntretienVoitureJour, double EntretienFractionRevenuVoiture,
    double ConsoMotoLitrePour100km, double ConsoVoitureLitrePour100km,
    double CoutTransportPaiementJirama,
    DateTime MisAJourAt) : ITransportReadModel;

public record TransportWriteModel(
    int ScenarioId,
    double PartInformelTransportPublic, double PartFormelCarburant, double PartInformelEntretien,
    double CoutTaxiBe, double EntretienVoitureJour,
    double ConsoMotoLitrePour100km, double ConsoVoitureLitrePour100km,
    double CoutTransportPaiementJirama) : ITransportWriteModel;
