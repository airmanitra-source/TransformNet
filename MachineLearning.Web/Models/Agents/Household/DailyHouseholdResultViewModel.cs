using Household.Module.Models;

namespace MachineLearning.Web.Models.Agents.Household;

public class DailyHouseholdResultViewModel : IFromBusinessModel<DailyHouseholdResult, DailyHouseholdResultViewModel>
{
    public double RevenuBrut { get; set; }
    public double Remittance { get; set; }
    public double LoyerImpute { get; set; }
    public double CotisationCNaPSSalariale { get; set; }
    public double ImpotIR { get; set; }
    public double TauxEffectifIR { get; set; }
    public double DepensesTransport { get; set; }
    public double TVAPayee { get; set; }
    public double Consommation { get; set; }
    public double DepensesRiz { get; set; }
    public double DepensesEau { get; set; }
    public double DepensesElectricite { get; set; }
    public double DepensesTransportJirama { get; set; }
    public double EpargneJour { get; set; }
    public double EpargneTotale { get; set; }
    public double FacteurChocPrix { get; set; }
    public double ReductionQuantiteAlimentaire { get; set; }
    public double DepensesAlimentairesSimulee { get; set; }
    public double DepensesAlimentaires { get; set; }
    public double DepensesAlimentairesInformel { get; set; }
    public double DepensesAlimentairesFormel { get; set; }

    public void FromBusinessModel(DailyHouseholdResult model)
    {
        RevenuBrut = model.RevenuBrut;
        Remittance = model.Remittance;
        LoyerImpute = model.LoyerImpute;
        CotisationCNaPSSalariale = model.CotisationCNaPSSalariale;
        ImpotIR = model.ImpotIR;
        TauxEffectifIR = model.TauxEffectifIR;
        DepensesTransport = model.DepensesTransport;
        TVAPayee = model.TVAPayee;
        Consommation = model.Consommation;
        DepensesRiz = model.DepensesRiz;
        DepensesEau = model.DepensesEau;
        DepensesElectricite = model.DepensesElectricite;
        DepensesTransportJirama = model.DepensesTransportJirama;
        EpargneJour = model.EpargneJour;
        EpargneTotale = model.EpargneTotale;
        FacteurChocPrix = model.FacteurChocPrix;
        ReductionQuantiteAlimentaire = model.ReductionQuantiteAlimentaire;
        DepensesAlimentairesSimulee = model.DepensesAlimentairesSimulee;
        DepensesAlimentaires = model.DepensesAlimentaires;
        DepensesAlimentairesInformel = model.DepensesAlimentairesInformel;
        DepensesAlimentairesFormel = model.DepensesAlimentairesFormel;
    }
}


