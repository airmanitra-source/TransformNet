using Company.Module.Models;
using MachineLearning.Web.Models;

namespace MachineLearning.Web.Models.Agents.Companies;

public class CompanyDailyResultViewModel : IFromBusinessModel<CompanyDailyResult, CompanyDailyResultViewModel>
{
    public double VentesB2C { get; set; }
    public double VentesB2B { get; set; }
    public double ChargesSalariales { get; set; }
    public double CotisationsCNaPS { get; set; }
    public double AchatsB2B { get; set; }
    public double DepensesElectricite { get; set; }
    public double TVACollectee { get; set; }
    public double ImpotIS { get; set; }
    public double BeneficeAvantImpot { get; set; }
    public double FluxNetJour { get; set; }
    public double Tresorerie { get; set; }
    public double CoutFinancement { get; set; }
    public double ValeurAjoutee { get; set; }

    public void FromBusinessModel(CompanyDailyResult model)
    {
        VentesB2C = model.VentesB2C;
        VentesB2B = model.VentesB2B;
        ChargesSalariales = model.ChargesSalariales;
        CotisationsCNaPS = model.CotisationsCNaPS;
        AchatsB2B = model.AchatsB2B;
        DepensesElectricite = model.DepensesElectricite;
        TVACollectee = model.TVACollectee;
        ImpotIS = model.ImpotIS;
        BeneficeAvantImpot = model.BeneficeAvantImpot;
        FluxNetJour = model.FluxNetJour;
        Tresorerie = model.Tresorerie;
        CoutFinancement = model.CoutFinancement;
        ValeurAjoutee = model.ValeurAjoutee;
    }
}


