using Company.Module.Models;
namespace Household.Module.Models;

/// <summary>
/// Comportement économique d'un ménage déterminé par sa classe socio-économique.
/// </summary>
public class HouseholdBehavior
{
    public double TauxEpargne { get; set; }
    public double PropensionConsommation { get; set; }
    public double DepensesAlimentairesJour { get; set; }
    public double DepensesDiversJour { get; set; }
    public double EpargneInitiale { get; set; }
    public double ProbabiliteEmploi { get; set; }
    /// <summary>Mode de transport attribué selon la classe socio-économique</summary>
    public ModeTransport Transport { get; set; } = ModeTransport.TransportPublic;
    /// <summary>Distance domicile-travail en km</summary>
    public double DistanceDomicileTravailKm { get; set; } = 10;

    // ─── Loisirs et vacances ───────────────────────────────────────────
    /// <summary>Budget sortie weekend (MGA par sortie : restaurant, loisirs)</summary>
    public double BudgetSortieWeekend { get; set; }

    /// <summary>Budget vacances (MGA par séjour : hôtel, transport, activités)</summary>
    public double BudgetVacances { get; set; }

    /// <summary>Probabilité de base de faire une sortie un jour de weekend (0-1)</summary>
    public double ProbabiliteSortieWeekend { get; set; }

    /// <summary>Fréquence des vacances en jours (~90 pour trimestriel, 0 = jamais)</summary>
    public int FrequenceVacancesJours { get; set; }

    /// <summary>Probabilité de base de partir en vacances quand la période arrive (0-1)</summary>
    public double ProbabiliteVacances { get; set; }

    /// <summary>Durée des vacances en jours (quand le ménage part)</summary>
    public int DureeVacancesJours { get; set; }
}



