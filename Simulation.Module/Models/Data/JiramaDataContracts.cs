namespace Simulation.Module.Models.Data;

/// <summary>Contrat de lecture des paramètres JIRAMA (électricité + eau).</summary>
public interface IJiramaReadModel
{
    int Id { get; }
    int ScenarioId { get; }
    double PrixElectriciteArKWh { get; }
    double ConsommationElecMenageKWhJour { get; }
    double ConsommationElecParEmployeKWhJour { get; }
    double ConsommationElecEtatKWhJour { get; }
    double PartProductionHydraulique { get; }
    double TauxPertesDistribution { get; }
    double PartConsommationElecMenages { get; }
    double TarifEauJourMenage { get; }
    double TauxAccesEau { get; }
    double TauxAccesElectricite { get; }
    DateTime MisAJourAt { get; }
}

/// <summary>Contrat d'écriture pour configurer les paramètres JIRAMA.</summary>
public interface IJiramaWriteModel
{
    int ScenarioId { get; }
    double PrixElectriciteArKWh { get; }
    double ConsommationElecMenageKWhJour { get; }
    double ConsommationElecParEmployeKWhJour { get; }
    double ConsommationElecEtatKWhJour { get; }
    double PartProductionHydraulique { get; }
    double TauxPertesDistribution { get; }
    double TarifEauJourMenage { get; }
    double TauxAccesEau { get; }
    double TauxAccesElectricite { get; }
}
