namespace Government.Module.Models;

public class GovernmentViewModel
{
    public List<TrancheIRSA> TranchesIRSA { get; set; } = TrancheIRSA.BaremeMadagascar();
    public double TauxIS { get; set; } = 0.20;
    public double TauxTVA { get; set; } = 0.20;
    public double TauxDirecteur { get; set; } = 0.09;
    public double TauxInflation { get; set; } = 0.08;
    public double TotalRecettesIR { get; set; }
    public double TotalRecettesIS { get; set; }
    public double TotalRecettesTVA { get; set; }
    public double TotalDroitsDouane { get; set; }
    public double TotalAccise { get; set; }
    public double TotalTVAImport { get; set; }
    public double TotalTaxeExport { get; set; }
    public double TotalRedevances { get; set; }
    public double TotalRecettesDouanieres => TotalDroitsDouane + TotalAccise + TotalTVAImport + TotalTaxeExport + TotalRedevances;
    public double TotalRecettesFiscales => TotalRecettesIR + TotalRecettesIS + TotalRecettesTVA + TotalRecettesDouanieres;
    public double TotalAideInternationale { get; set; }
    public double TotalSubventionsJirama { get; set; }
    public double TotalCotisationsCNaPS { get; set; }
    public double TotalDepensesPubliques { get; set; }
    public double TotalSalairesFonctionnaires { get; set; }
    public double DepensesPubliquesJour { get; set; } = 500_000;
    public double TauxRedistribution { get; set; } = 0.15;
    public double SoldeBudgetaire { get; set; }
    public double TotalDepensesElectricite { get; set; }
    public double DettePublique { get; set; }
}


