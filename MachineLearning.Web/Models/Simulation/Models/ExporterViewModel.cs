using MachineLearning.Web.Models.Agents.Companies;

namespace MachineLearning.Web.Models.Simulation.Models;

public class ExporterViewModel
{
    public ECategorieExportViewModel Categorie { get; set; } = ECategorieExportViewModel.Vanille;
    public double ValeurFOBJour { get; set; } = 300_000;
    public double PartExport { get; set; } = 0.70;
    public double TauxChangeMGA { get; set; } = 4_500;
    public double TotalExportationsFOB { get; set; }
    public double TotalTaxeExport { get; set; }
    public double TotalRedevanceExport { get; set; }
    public double TotalDevisesRapatriees { get; set; }
}


