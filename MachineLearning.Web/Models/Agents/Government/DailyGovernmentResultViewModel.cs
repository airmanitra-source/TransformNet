using Government.Module.Models;
using MachineLearning.Web.Models;

namespace MachineLearning.Web.Models.Agents.Government;

public class DailyGovernmentResultViewModel : IFromBusinessModel<DailyGovernmentResult, DailyGovernmentResultViewModel>
{
    public double RecettesIR { get; set; }
    public double RecettesIS { get; set; }
    public double RecettesTVA { get; set; }
    public double TVAJirama { get; set; }
    public double AideInternationale { get; set; }
    public double CotisationsCNaPS { get; set; }
    public double RecettesTotales { get; set; }
    public double DepensesPubliques { get; set; }
    public double DepensesElectriciteEtat { get; set; }
    public double SubventionsJirama { get; set; }
    public double SalairesFonctionnaires { get; set; }
    public double FBCF { get; set; }
    public double DepensesCapital { get; set; }
    public double ConsommationFinaleEtat { get; set; }
    public double TransfertsSociaux { get; set; }
    public double SoldeJour { get; set; }
    public double SoldeCumule { get; set; }
    public double DettePublique { get; set; }
    public double InteretsDette { get; set; }
    public double DroitsDouane { get; set; }
    public double Accise { get; set; }
    public double TVAImport { get; set; }
    public double TaxeExport { get; set; }
    public double RecettesDouanieres { get; set; }
    public double ImportationsCIF { get; set; }
    public double ExportationsFOB { get; set; }
    public double BalanceCommerciale { get; set; }

    public void FromBusinessModel(DailyGovernmentResult model)
    {
        RecettesIR = model.RecettesIR;
        RecettesIS = model.RecettesIS;
        RecettesTVA = model.RecettesTVA;
        TVAJirama = model.TVAJirama;
        AideInternationale = model.AideInternationale;
        CotisationsCNaPS = model.CotisationsCNaPS;
        RecettesTotales = model.RecettesTotales;
        DepensesPubliques = model.DepensesPubliques;
        DepensesElectriciteEtat = model.DepensesElectriciteEtat;
        SubventionsJirama = model.SubventionsJirama;
        SalairesFonctionnaires = model.SalairesFonctionnaires;
        FBCF = model.FBCF;
        DepensesCapital = model.DepensesCapital;
        ConsommationFinaleEtat = model.ConsommationFinaleEtat;
        TransfertsSociaux = model.TransfertsSociaux;
        SoldeJour = model.SoldeJour;
        SoldeCumule = model.SoldeCumule;
        DettePublique = model.DettePublique;
        InteretsDette = model.InteretsDette;
        DroitsDouane = model.DroitsDouane;
        Accise = model.Accise;
        TVAImport = model.TVAImport;
        TaxeExport = model.TaxeExport;
        RecettesDouanieres = model.RecettesDouanieres;
        ImportationsCIF = model.ImportationsCIF;
        ExportationsFOB = model.ExportationsFOB;
        BalanceCommerciale = model.BalanceCommerciale;
    }
}


