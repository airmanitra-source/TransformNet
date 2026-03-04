using MachineLearning.Web.Models;
using HhModel = Household.Module.Models.Household;

namespace MachineLearning.Web.Models.Agents.Household;

public class HouseholdViewModel : IFromBusinessModel<HhModel, HouseholdViewModel>
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public double SalaireMensuel { get; set; }
    public double SalaireJournalier { get; set; }
    public double Epargne { get; set; }
    public double TauxEpargne { get; set; }
    public double PropensionConsommation { get; set; }
    public double DepensesAlimentairesJour { get; set; }
    public double DepensesDiversJour { get; set; }
    public double DepensesRizJour { get; set; }
    public double DepensesEauJour { get; set; }
    public double DepensesElectriciteJour { get; set; }
    public bool AccesEau { get; set; }
    public bool AccesElectricite { get; set; }
    public ModeTransportViewModel Transport { get; set; }
    public double DistanceDomicileTravailKm { get; set; }
    public bool EstEmploye { get; set; }
    public double TotalConsomme { get; set; }
    public double TotalImpotsPaye { get; set; }
    public double TotalTransport { get; set; }
    public double TotalDepensesRiz { get; set; }
    public double TotalDepensesJirama { get; set; }

    public void FromBusinessModel(HhModel model)
    {
        Id = model.Id;
        Name = model.Name;
        SalaireMensuel = model.SalaireMensuel;
        SalaireJournalier = model.SalaireJournalier;
        Epargne = model.Epargne;
        TauxEpargne = model.TauxEpargne;
        PropensionConsommation = model.PropensionConsommation;
        DepensesAlimentairesJour = model.DepensesAlimentairesJour;
        DepensesDiversJour = model.DepensesDiversJour;
        DepensesRizJour = model.DepensesRizJour;
        DepensesEauJour = model.DepensesEauJour;
        DepensesElectriciteJour = model.DepensesElectriciteJour;
        AccesEau = model.AccesEau;
        AccesElectricite = model.AccesElectricite;
        Transport = (ModeTransportViewModel)(int)model.Transport;
        DistanceDomicileTravailKm = model.DistanceDomicileTravailKm;
        EstEmploye = model.EstEmploye;
        TotalConsomme = model.TotalConsomme;
        TotalImpotsPaye = model.TotalImpotsPaye;
        TotalTransport = model.TotalTransport;
        TotalDepensesRiz = model.TotalDepensesRiz;
        TotalDepensesJirama = model.TotalDepensesJirama;
    }
}


