namespace MachineLearning.Web.Models.Agents.Household;

public class HouseholdSalaryDistributionViewModel
{
    public double SalaireMedian { get; set; } = 170_000;
    public double Sigma { get; set; } = 0.85;
    public double SalairePlancher { get; set; } = 200_000;
    public double SalairePlafond { get; set; } = 10_000_000;
    public double PartSecteurInformel { get; set; } = 0.85;
    public double Mu => Math.Log(SalaireMedian);
    public double MoyenneTheorique => Math.Exp(Mu + Sigma * Sigma / 2.0);
    public double GiniTheorique => 2.0 * NormalCDF(Sigma / Math.Sqrt(2.0)) - 1.0;

    /// <summary>
    /// Fonction de distribution normale cumulative (CDF)
    /// </summary>
    private static double NormalCDF(double x)
    {
        return 0.5 * (1.0 + Erf(x / Math.Sqrt(2.0)));
    }

    /// <summary>
    /// Fonction d'erreur (error function) - approximation d'Abramowitz et Stegun
    /// </summary>
    private static double Erf(double x)
    {
        // Constantes pour l'approximation
        double a1 = 0.254829592;
        double a2 = -0.284496736;
        double a3 = 1.421413741;
        double a4 = -1.453152027;
        double a5 = 1.061405429;
        double p = 0.3275911;

        // Sauvegarder le signe de x
        int sign = x < 0 ? -1 : 1;
        x = Math.Abs(x);

        // Approximation d'Abramowitz et Stegun, formule 7.1.26
        double t = 1.0 / (1.0 + p * x);
        double y = 1.0 - (((((a5 * t + a4) * t) + a3) * t + a2) * t + a1) * t * Math.Exp(-x * x);

        return sign * y;
    }
}




