namespace Company.Module.Models.Data;

/// <summary>Contrat de lecture des paramètres globaux des entreprises.</summary>
public interface IEntreprisesReadModel
{
    int Id { get; }
    int ScenarioId { get; }
    double PartEntreprisesAgricoles { get; }
    double PartEntreprisesConstruction { get; }
    double PartEntreprisesHotellerieTourisme { get; }
    double MargeBeneficiaireEntreprise { get; }
    double ProductiviteParEmployeJourDefaut { get; }
    double PartB2B { get; }
    double FacteurProductiviteInformelMin { get; }
    double FacteurProductiviteInformelMax { get; }
    int SeuilJoursStressTresorerie { get; }
    int SeuilJoursDemandeExcedentaire { get; }
    double SalaireMoyenMensuelDefaut { get; }
    DateTime MisAJourAt { get; }
}

/// <summary>Contrat d'écriture pour configurer les paramètres entreprises.</summary>
public interface IEntreprisesWriteModel
{
    int ScenarioId { get; }
    double PartEntreprisesAgricoles { get; }
    double PartEntreprisesConstruction { get; }
    double PartEntreprisesHotellerieTourisme { get; }
    double MargeBeneficiaireEntreprise { get; }
    double ProductiviteParEmployeJourDefaut { get; }
    double PartB2B { get; }
    double FacteurProductiviteInformelMin { get; }
    double FacteurProductiviteInformelMax { get; }
    int SeuilJoursStressTresorerie { get; }
    int SeuilJoursDemandeExcedentaire { get; }
    double SalaireMoyenMensuelDefaut { get; }
}

/// <summary>Contrat de lecture des paramètres d'un secteur d'activité.</summary>
public interface ISecteurActiviteReadModel
{
    int Id { get; }
    int ScenarioId { get; }
    int Secteur { get; }
    string SecteurLibelle { get; }
    double ProductiviteJourMoyenne { get; }
    double ProductiviteJourBasse { get; }
    double TresorerieInitiale { get; }
    int NombreEmployesDefaut { get; }
    DateTime MisAJourAt { get; }
}

/// <summary>Contrat d'écriture pour configurer un secteur d'activité.</summary>
public interface ISecteurActiviteWriteModel
{
    int ScenarioId { get; }
    int Secteur { get; }
    double ProductiviteJourMoyenne { get; }
    double ProductiviteJourBasse { get; }
    double TresorerieInitiale { get; }
    int NombreEmployesDefaut { get; }
}
