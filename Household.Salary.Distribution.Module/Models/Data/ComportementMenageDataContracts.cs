namespace Household.Salary.Distribution.Module.Models.Data;

/// <summary>Contrat de lecture du comportement d'une classe socio-économique.</summary>
public interface IComportementMenageReadModel
{
    int Id { get; }
    int ScenarioId { get; }
    int Classe { get; }
    string ClasseLibelle { get; }
    double TauxEpargneMin { get; }
    double TauxEpargneMax { get; }
    double PropensionConsommationMin { get; }
    double PropensionConsommationMax { get; }
    double EpargneInitialeMax { get; }
    double DepensesAlimentairesJourMin { get; }
    double DepensesAlimentairesJourMax { get; }
    double DepensesDiversJourMin { get; }
    double DepensesDiversJourMax { get; }
    double ProbabiliteEmploiMin { get; }
    double ProbabiliteEmploiMax { get; }
    int ModeTransportPreferentiel { get; }
    double DistanceDomicileTravailKmMin { get; }
    double DistanceDomicileTravailKmMax { get; }
    double BudgetSortieWeekendMin { get; }
    double BudgetSortieWeekendMax { get; }
    double BudgetVacancesMin { get; }
    double BudgetVacancesMax { get; }
    double ProbabiliteSortieWeekendMin { get; }
    double ProbabiliteSortieWeekendMax { get; }
    int FrequenceVacancesJours { get; }
    double ProbabiliteVacancesMin { get; }
    double ProbabiliteVacancesMax { get; }
    int DureeVacancesJoursMin { get; }
    int DureeVacancesJoursMax { get; }
    DateTime MisAJourAt { get; }
}

/// <summary>Contrat d'écriture pour configurer le comportement d'une classe.</summary>
public interface IComportementMenageWriteModel
{
    int ScenarioId { get; }
    int Classe { get; }
    double TauxEpargneMin { get; }
    double TauxEpargneMax { get; }
    double PropensionConsommationMin { get; }
    double PropensionConsommationMax { get; }
    double EpargneInitialeMax { get; }
    double DepensesAlimentairesJourMin { get; }
    double DepensesAlimentairesJourMax { get; }
    double DepensesDiversJourMin { get; }
    double DepensesDiversJourMax { get; }
    double ProbabiliteEmploiMin { get; }
    double ProbabiliteEmploiMax { get; }
    int ModeTransportPreferentiel { get; }
    double DistanceDomicileTravailKmMin { get; }
    double DistanceDomicileTravailKmMax { get; }
}
