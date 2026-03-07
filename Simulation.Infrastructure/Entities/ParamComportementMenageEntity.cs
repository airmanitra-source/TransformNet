namespace Simulation.Infrastructure.Entities;

/// <summary>
/// Comportement économique d'une classe socio-économique dans un scénario.
/// Classe : 0=Subsistance, 1=InformelBas, 2=FormelBas, 3=FormelQualifie, 4=Cadre.
/// </summary>
public class ParamComportementMenageEntity
{
    public int Id { get; set; }
    public int ScenarioId { get; set; }
    public int Classe { get; set; }
    public string ClasseLibelle { get; set; } = string.Empty;

    public double TauxEpargneMin { get; set; }
    public double TauxEpargneMax { get; set; }
    public double PropensionConsommationMin { get; set; }
    public double PropensionConsommationMax { get; set; }
    public double EpargneInitialeMax { get; set; }

    public double DepensesAlimentairesJourMin { get; set; }
    public double DepensesAlimentairesJourMax { get; set; }
    public double DepensesDiversJourMin { get; set; }
    public double DepensesDiversJourMax { get; set; }

    public double ProbabiliteEmploiMin { get; set; }
    public double ProbabiliteEmploiMax { get; set; }

    /// <summary>0=APied, 1=TransportPublic, 2=Moto, 3=Voiture</summary>
    public int ModeTransportPreferentiel { get; set; }
    public double DistanceDomicileTravailKmMin { get; set; }
    public double DistanceDomicileTravailKmMax { get; set; }

    public double BudgetSortieWeekendMin { get; set; }
    public double BudgetSortieWeekendMax { get; set; }
    public double BudgetVacancesMin { get; set; }
    public double BudgetVacancesMax { get; set; }
    public double ProbabiliteSortieWeekendMin { get; set; }
    public double ProbabiliteSortieWeekendMax { get; set; }
    public int FrequenceVacancesJours { get; set; }
    public double ProbabiliteVacancesMin { get; set; }
    public double ProbabiliteVacancesMax { get; set; }
    public int DureeVacancesJoursMin { get; set; }
    public int DureeVacancesJoursMax { get; set; }

    public DateTime MisAJourAt { get; set; }
}
