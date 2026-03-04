namespace MachineLearning.Web.Models.Agents.Companies;

public class DailyImporterResultViewModel
{
    public double ValeurCIF { get; set; }
    public double DroitsDouane { get; set; }
    public double Accise { get; set; }
    public double TVAImport { get; set; }
    public double RedevanceStatistique { get; set; }
    public double CoutTotalImport { get; set; }
    public double ReventeImport { get; set; }
    public double RecettesDouanieres => DroitsDouane + Accise + TVAImport + RedevanceStatistique;
}


