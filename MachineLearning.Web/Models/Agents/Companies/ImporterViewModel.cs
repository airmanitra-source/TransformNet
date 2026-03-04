namespace MachineLearning.Web.Models.Agents.Companies;

public class ImporterViewModel
{
    public ECategorieImportViewModel Categorie { get; set; } = ECategorieImportViewModel.BienConsommation;
    public double ValeurCIFJour { get; set; } = 500_000;
    public double PartReventeImport { get; set; } = 0.60;
    public double MargeReventeImport { get; set; } = 0.25;
    public double TotalImportationsCIF { get; set; }
    public double TotalDroitsDouane { get; set; }
    public double TotalAccise { get; set; }
    public double TotalTVAImport { get; set; }
    public double TotalRedevanceStatistique { get; set; }
}


