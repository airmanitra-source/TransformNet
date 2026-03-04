using Household.Module.Models;
using MachineLearning.Web.Models;

namespace MachineLearning.Web.Models.Agents.Household;

public class DistributionStatsViewModel : IFromBusinessModel<DistributionStats, DistributionStatsViewModel>
{
    public double Moyenne { get; set; }
    public double Mediane { get; set; }
    public double Min { get; set; }
    public double Max { get; set; }
    public double EcartType { get; set; }
    public double Gini { get; set; }
    public double RatioD9D1 { get; set; }
    public double Q1Moyenne { get; set; }
    public double Q2Moyenne { get; set; }
    public double Q3Moyenne { get; set; }
    public double Q4Moyenne { get; set; }
    public double Q5Moyenne { get; set; }

    public void FromBusinessModel(DistributionStats model)
    {
        Moyenne = model.Moyenne;
        Mediane = model.Mediane;
        Min = model.Min;
        Max = model.Max;
        EcartType = model.EcartType;
        Gini = model.Gini;
        RatioD9D1 = model.RatioD9D1;
        Q1Moyenne = model.Q1Moyenne;
        Q2Moyenne = model.Q2Moyenne;
        Q3Moyenne = model.Q3Moyenne;
        Q4Moyenne = model.Q4Moyenne;
        Q5Moyenne = model.Q5Moyenne;
    }
}


