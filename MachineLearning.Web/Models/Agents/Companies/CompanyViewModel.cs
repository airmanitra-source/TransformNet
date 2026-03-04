using CompanyModel = Company.Module.Models.Company;

namespace MachineLearning.Web.Models.Agents.Companies;

public class CompanyViewModel : IFromBusinessModel<CompanyModel, CompanyViewModel>
{
    public string Name { get; set; } = string.Empty;
    public ESecteurActiviteViewModel SecteurActivite { get; set; } = ESecteurActiviteViewModel.Services;
    public bool EstInformel { get; set; }
    public double Tresorerie { get; set; } = 5_000_000;
    public double ChiffreAffairesCumule { get; set; }
    public double ChargesCumulees { get; set; }
    public int NombreEmployes { get; set; } = 10;
    public double SalaireMoyenMensuel { get; set; } = 200_000;
    public double ProductiviteParEmployeJour { get; set; } = 15_000;
    public double MargeBeneficiaire { get; set; } = 0.20;
    public double PartB2B { get; set; } = 0.30;
    public double TotalAchatsB2B { get; set; }
    public double TotalImpotsSociete { get; set; }
    public double TotalTVACollectee { get; set; }
    public double TotalCotisationsCNaPS { get; set; }

    public void FromBusinessModel(CompanyModel model)
    {
        Name = model.Name;
        SecteurActivite = (ESecteurActiviteViewModel)(int)model.SecteurActivite;
        EstInformel = model.EstInformel;
        Tresorerie = model.Tresorerie;
        ChiffreAffairesCumule = model.ChiffreAffairesCumule;
        ChargesCumulees = model.ChargesCumulees;
        NombreEmployes = model.NombreEmployes;
        SalaireMoyenMensuel = model.SalaireMoyenMensuel;
        ProductiviteParEmployeJour = model.ProductiviteParEmployeJour;
        MargeBeneficiaire = model.MargeBeneficiaire;
        PartB2B = model.PartB2B;
        TotalAchatsB2B = model.TotalAchatsB2B;
        TotalImpotsSociete = model.TotalImpotsSociete;
        TotalTVACollectee = model.TotalTVACollectee;
        TotalCotisationsCNaPS = model.TotalCotisationsCNaPS;
    }
}


