namespace Household.HealthExpenditure.Module.Models;

public class HealthExpenseResult
{
    public bool EstMalade { get; set; }
    public double ProbabiliteMaladie { get; set; }
    public double DepenseTotale { get; set; }
    public double PartFormelle { get; set; }
    public double PartInformelle { get; set; }
}
