namespace Bank.Module.Models.Data;

/// <summary>Contrat de lecture des paramètres bancaires d'un scénario.</summary>
public interface IBanqueReadModel
{
    int Id { get; }
    int ScenarioId { get; }
    double TauxInteretDepots { get; }
    double TauxInteretCredits { get; }
    double TauxReserveObligatoire { get; }
    double PartDepotsAVue { get; }
    double PartMonnaieCirculationDansM1 { get; }
    double RatioM3SurM2 { get; }
    double CroissanceCreditJour { get; }
    double PartCreditEntreprises { get; }
    double ProbabiliteDefautCreditJour { get; }
    double TauxRecouvrementNPLJour { get; }
    double AvoirsExterieursNetsInitiaux { get; }
    double CreancesNettesEtatInitiales { get; }
    double SCBInitial { get; }
    double IntensiteInterventionBFM { get; }
    double RatioExcedentSCBCible { get; }
    DateTime MisAJourAt { get; }
}

/// <summary>Contrat d'écriture pour configurer les paramètres bancaires.</summary>
public interface IBanqueWriteModel
{
    int ScenarioId { get; }
    double TauxInteretDepots { get; }
    double TauxInteretCredits { get; }
    double TauxReserveObligatoire { get; }
    double PartDepotsAVue { get; }
    double PartMonnaieCirculationDansM1 { get; }
    double RatioM3SurM2 { get; }
    double CroissanceCreditJour { get; }
    double PartCreditEntreprises { get; }
    double ProbabiliteDefautCreditJour { get; }
    double TauxRecouvrementNPLJour { get; }
    double IntensiteInterventionBFM { get; }
    double RatioExcedentSCBCible { get; }
}
