namespace Household.Leisure.Spending.Module.Models.Data;

/// <summary>Contrat de lecture des paramètres de loisirs d'une classe socio-économique.</summary>
public interface ILoisirsReadModel
{
    int Id { get; }
    int ScenarioId { get; }
    int Classe { get; }
    string ClasseLibelle { get; }
    double BudgetSortieWeekendMin { get; }
    double BudgetSortieWeekendMax { get; }
    double BudgetVacancesMin { get; }
    double BudgetVacancesMax { get; }
    double ProbabiliteSortieWeekendMin { get; }
    double ProbabiliteSortieWeekendMax { get; }
    double ProbabiliteVacancesMin { get; }
    double ProbabiliteVacancesMax { get; }
    int FrequenceVacancesJours { get; }
    int DureeVacancesJoursMin { get; }
    int DureeVacancesJoursMax { get; }
    double SensibiliteInflation { get; }
    double SeuilInflationReaction { get; }
    DateTime MisAJourAt { get; }
}

/// <summary>Contrat d'écriture pour configurer les loisirs par classe.</summary>
public interface ILoisirsWriteModel
{
    int ScenarioId { get; }
    int Classe { get; }
    double BudgetSortieWeekendMin { get; }
    double BudgetSortieWeekendMax { get; }
    double BudgetVacancesMin { get; }
    double BudgetVacancesMax { get; }
    double ProbabiliteSortieWeekendMin { get; }
    double ProbabiliteSortieWeekendMax { get; }
    double ProbabiliteVacancesMin { get; }
    double ProbabiliteVacancesMax { get; }
    int FrequenceVacancesJours { get; }
    int DureeVacancesJoursMin { get; }
    int DureeVacancesJoursMax { get; }
    double SensibiliteInflation { get; }
    double SeuilInflationReaction { get; }
}
