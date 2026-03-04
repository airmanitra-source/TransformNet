using Household.Module.Models;
using Household.Salary.Distribution.Module.Models;

namespace Household.Salary.Distribution.Module
{
    /// <summary>
    /// Implémentation du module de distribution salariale.
    /// Délègue entièrement au modèle <see cref="HouseholdSalaryDistribution"/>
    /// afin d'éviter toute duplication de logique.
    /// </summary>
    public class HouseholdSalaryDistributionModule : IHouseholdSalaryDistributionModule
    {
        private readonly HouseholdSalaryDistribution _distribution;

        public HouseholdSalaryDistributionModule(
            double salaireMedian  = 170_000,
            double sigma          = 0.85,
            double salairePlancher = 50_000)
        {
            _distribution = new HouseholdSalaryDistribution
            {
                SalaireMedian   = salaireMedian,
                Sigma           = sigma,
                SalairePlancher = salairePlancher
            };
        }

        /// <inheritdoc/>
        public double TirerSalaire(Random random)
            => _distribution.TirerSalaire(random);

        /// <inheritdoc/>
        public ClasseSocioEconomique DeterminerClasse(double salaireMensuel)
            => _distribution.DeterminerClasse(salaireMensuel);

        /// <inheritdoc/>
        public DistributionStats CalculerStats(double[] valeurs)
            => HouseholdSalaryDistribution.CalculerStats(valeurs);
    }
}
