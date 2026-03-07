using Company.Module.Models;

namespace Household.Module.Models;

/// <summary>
/// Configuration comportementale d'une classe socio-économique, chargée depuis la base de données.
/// Remplace les magic numbers hardcodés dans les modules.
/// Les valeurs Min/Max définissent la plage de variation aléatoire lors de l'initialisation des ménages.
/// </summary>
public class ComportementClasseConfig
{
    public ClasseSocioEconomique Classe { get; set; }

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

    public ModeTransport ModeTransportPreferentiel { get; set; }
    public double DistanceDomicileTravailKmMin { get; set; }
    public double DistanceDomicileTravailKmMax { get; set; }

    // ─── Loisirs ─────────────────────────────────────────────────
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
}
