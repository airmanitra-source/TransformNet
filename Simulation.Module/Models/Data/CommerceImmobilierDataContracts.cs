namespace Simulation.Module.Models.Data;

/// <summary>Contrat de lecture des paramètres du commerce extérieur.</summary>
public interface ICommerceReadModel
{
    int Id { get; }
    int ScenarioId { get; }
    double RemittancesJour { get; }
    double ConsommationRizAnnuelleKgParPersonne { get; }
    double PrixRizLocalKg { get; }
    double PrixRizImporteKg { get; }
    double PartRizImporte { get; }
    DateTime MisAJourAt { get; }
}

/// <summary>Contrat d'écriture pour configurer les paramètres commerce.</summary>
public interface ICommerceWriteModel
{
    int ScenarioId { get; }
    double RemittancesJour { get; }
    double ConsommationRizAnnuelleKgParPersonne { get; }
    double PrixRizLocalKg { get; }
    double PrixRizImporteKg { get; }
    double PartRizImporte { get; }
}

/// <summary>Contrat de lecture des paramètres immobiliers.</summary>
public interface IImmobilierReadModel
{
    int Id { get; }
    int ScenarioId { get; }
    double LoyerImputeJourParMenage { get; }
    double TauxMenagesProprietaires { get; }
    double LoyerJourLocataire { get; }
    double ProbabiliteConstructionMaisonLocataire { get; }
    int DureeConstructionMaisonJours { get; }
    double BudgetConstructionMaisonJour { get; }
    double PartBudgetConstructionBTP { get; }
    double PartBudgetConstructionQuincaillerie { get; }
    double PartBudgetConstructionTransportInformel { get; }
    DateTime MisAJourAt { get; }
}

/// <summary>Contrat d'écriture pour configurer les paramètres immobiliers.</summary>
public interface IImmobilierWriteModel
{
    int ScenarioId { get; }
    double LoyerImputeJourParMenage { get; }
    double TauxMenagesProprietaires { get; }
    double LoyerJourLocataire { get; }
    double ProbabiliteConstructionMaisonLocataire { get; }
    int DureeConstructionMaisonJours { get; }
    double BudgetConstructionMaisonJour { get; }
    double PartBudgetConstructionBTP { get; }
    double PartBudgetConstructionQuincaillerie { get; }
    double PartBudgetConstructionTransportInformel { get; }
}
