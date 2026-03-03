namespace MachineLearning.Web.Models.Agents.Household;

/// <summary>
/// Statistiques descriptives d'une distribution de salaires.
/// </summary>
public class DistributionStats
{
    public double Moyenne { get; set; }
    public double Mediane { get; set; }
    public double Min { get; set; }
    public double Max { get; set; }
    public double EcartType { get; set; }
    public double Gini { get; set; }
    public double RatioD9D1 { get; set; }

    // Moyennes par quintile
    public double Q1Moyenne { get; set; }
    public double Q2Moyenne { get; set; }
    public double Q3Moyenne { get; set; }
    public double Q4Moyenne { get; set; }
    public double Q5Moyenne { get; set; }
}
