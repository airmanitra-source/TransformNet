namespace MachineLearning.Web.Models.Agents.Companies;

public class DailyExporterResultViewModel
{
    public double ValeurFOB { get; set; }
    public double TaxeExport { get; set; }
    public double RedevanceExport { get; set; }
    public double DevisesRapatriees { get; set; }
    public double RecettesFiscalesExport => TaxeExport + RedevanceExport;
}


