namespace MachineLearning.Web.Models.Agents.Companies;

/// <summary>
/// Résultat journalier d'un importateur (hérite du résultat entreprise + données douanières).
/// </summary>
public class DailyImporterResult : CompanyDailyResult
{
    public double ValeurCIF { get; set; }
    public double DroitsDouane { get; set; }
    public double Accise { get; set; }
    public double TVAImport { get; set; }
    public double RedevanceStatistique { get; set; }
    public double CoutTotalImport { get; set; }
    public double ReventeImport { get; set; }

    /// <summary>Total recettes douanières générées par cet importateur ce jour</summary>
    public double RecettesDouanieres => DroitsDouane + Accise + TVAImport + RedevanceStatistique;
}
