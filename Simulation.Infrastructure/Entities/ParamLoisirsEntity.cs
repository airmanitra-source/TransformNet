namespace Simulation.Infrastructure.Entities;

/// <summary>Paramètres de loisirs par classe socio-économique.</summary>
public class ParamLoisirsEntity
{
    public int Id { get; set; }
    public int ScenarioId { get; set; }
    public int Classe { get; set; }
    public string ClasseLibelle { get; set; } = string.Empty;

    public double BudgetSortieWeekendMin { get; set; }
    public double BudgetSortieWeekendMax { get; set; }
    public double BudgetVacancesMin { get; set; }
    public double BudgetVacancesMax { get; set; }
    public double ProbabiliteSortieWeekendMin { get; set; }
    public double ProbabiliteSortieWeekendMax { get; set; }
    public double ProbabiliteVacancesMin { get; set; }
    public double ProbabiliteVacancesMax { get; set; }
    public int FrequenceVacancesJours { get; set; }
    public int DureeVacancesJoursMin { get; set; }
    public int DureeVacancesJoursMax { get; set; }
    public double SensibiliteInflation { get; set; } = 0.15;
    public double SeuilInflationReaction { get; set; } = 8.0;

    public DateTime MisAJourAt { get; set; }
}
