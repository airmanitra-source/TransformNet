using Household.Module.Models;
using MachineLearning.Web.Models;

namespace MachineLearning.Web.Models.Agents.Household;

public class HouseholdBehaviorViewModel : IFromBusinessModel<HouseholdBehavior, HouseholdBehaviorViewModel>
{
    public double TauxEpargne { get; set; }
    public double PropensionConsommation { get; set; }
    public double DepensesAlimentairesJour { get; set; }
    public double DepensesDiversJour { get; set; }
    public double EpargneInitiale { get; set; }
    public double ProbabiliteEmploi { get; set; }
    public ModeTransportViewModel Transport { get; set; } = ModeTransportViewModel.TransportPublic;
    public double DistanceDomicileTravailKm { get; set; } = 10;

    public void FromBusinessModel(HouseholdBehavior model)
    {
        TauxEpargne = model.TauxEpargne;
        PropensionConsommation = model.PropensionConsommation;
        DepensesAlimentairesJour = model.DepensesAlimentairesJour;
        DepensesDiversJour = model.DepensesDiversJour;
        EpargneInitiale = model.EpargneInitiale;
        ProbabiliteEmploi = model.ProbabiliteEmploi;
        Transport = (ModeTransportViewModel)(int)model.Transport;
        DistanceDomicileTravailKm = model.DistanceDomicileTravailKm;
    }
}


