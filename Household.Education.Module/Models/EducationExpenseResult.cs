namespace Household.Education.Module.Models;

public class EducationExpenseResult
{
    public int NombreEnfantsScolarises { get; set; }
    public int DureeDepenseJours { get; set; }
    public bool EstPeriodeDepense { get; set; }
    public double DepenseTotale { get; set; }
    public double PartFormelle { get; set; }
    public double PartInformelle { get; set; }
}
