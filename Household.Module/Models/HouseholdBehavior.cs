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
}



