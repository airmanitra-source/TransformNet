using Household.Module.Models;
using Household.Salary.Distribution.Module.Models;

namespace Household.Salary.Distribution.Module
{
    /// <summary>
    /// Implémentation du module de distribution salariale.
    /// Délègue entièrement au modèle <see cref="HouseholdSalaryDistribution"/>
    /// afin d'éviter toute duplication de logique.
    /// Les comportements par classe peuvent être configurés depuis la BD
    /// via <see cref="ConfigurerComportements"/>.
    /// </summary>
    public class HouseholdSalaryDistributionModule : IHouseholdSalaryDistributionModule
    {
        private readonly HouseholdSalaryDistribution _distribution;
        private Dictionary<ClasseSocioEconomique, ComportementClasseConfig>? _configsParClasse;

        public HouseholdSalaryDistributionModule()
        {
            // Valeurs neutres — ConfigurerDistributionSalariale() est obligatoirement
            // appelé par SimulationModule.Initialiser() avant tout usage.
            _distribution = new HouseholdSalaryDistribution
            {
                SalaireMedian    = 0,
                Sigma            = 0,
                SalairePlancher  = 0
            };
        }

        /// <inheritdoc/>
        public double TirerSalaire(Random random)
            => _distribution.TirerSalaire(random);

        /// <inheritdoc/>
        public void ConfigurerDistributionSalariale(
            double salaireMedian,
            double sigma,
            double salairePlancher,
            double partSecteurInformel)
        {
            _distribution.SalaireMedian = salaireMedian;
            _distribution.Sigma = sigma;
            _distribution.SalairePlancher = salairePlancher;
            _distribution.PartSecteurInformel = partSecteurInformel;
        }

        /// <inheritdoc/>
        public void ConfigurerComportements(IEnumerable<ComportementClasseConfig> configs)
        {
            _configsParClasse = configs.ToDictionary(c => c.Classe);
        }

        /// <inheritdoc/>
        public ClasseSocioEconomique DeterminerClasse(double salaireMensuel)
            => _distribution.DeterminerClasse(salaireMensuel);

        /// <inheritdoc/>
        public HouseholdBehavior GetComportementParClasse(ClasseSocioEconomique classe, Random random)
        {
            // Si des paramètres BD sont configurés, les utiliser au lieu des magic numbers
            if (_configsParClasse != null && _configsParClasse.TryGetValue(classe, out var cfg))
                return ComportementDepuisConfig(cfg, random);

            // Fallback : valeurs hardcodées historiques
            return HouseholdSalaryDistribution.ComportementParClasse(classe, random);
        }

        /// <inheritdoc/>
        public DistributionStats CalculerStats(double[] valeurs)
            => HouseholdSalaryDistribution.CalculerStats(valeurs);

        /// <summary>
        /// Construit un <see cref="HouseholdBehavior"/> à partir des paramètres BD
        /// en tirant des valeurs aléatoires dans les plages [Min, Max].
        /// </summary>
        private static HouseholdBehavior ComportementDepuisConfig(ComportementClasseConfig cfg, Random random)
        {
            return new HouseholdBehavior
            {
                TauxEpargne = cfg.TauxEpargneMin + random.NextDouble() * (cfg.TauxEpargneMax - cfg.TauxEpargneMin),
                PropensionConsommation = cfg.PropensionConsommationMin + random.NextDouble() * (cfg.PropensionConsommationMax - cfg.PropensionConsommationMin),
                DepensesAlimentairesJour = cfg.DepensesAlimentairesJourMin + random.NextDouble() * (cfg.DepensesAlimentairesJourMax - cfg.DepensesAlimentairesJourMin),
                DepensesDiversJour = cfg.DepensesDiversJourMin + random.NextDouble() * (cfg.DepensesDiversJourMax - cfg.DepensesDiversJourMin),
                EpargneInitiale = random.NextDouble() * cfg.EpargneInitialeMax,
                ProbabiliteEmploi = cfg.ProbabiliteEmploiMin + random.NextDouble() * (cfg.ProbabiliteEmploiMax - cfg.ProbabiliteEmploiMin),
                Transport = cfg.ModeTransportPreferentiel,
                DistanceDomicileTravailKm = cfg.DistanceDomicileTravailKmMin + random.NextDouble() * (cfg.DistanceDomicileTravailKmMax - cfg.DistanceDomicileTravailKmMin),
                BudgetSortieWeekend = cfg.BudgetSortieWeekendMin + random.NextDouble() * (cfg.BudgetSortieWeekendMax - cfg.BudgetSortieWeekendMin),
                BudgetVacances = cfg.BudgetVacancesMin + random.NextDouble() * (cfg.BudgetVacancesMax - cfg.BudgetVacancesMin),
                ProbabiliteSortieWeekend = cfg.ProbabiliteSortieWeekendMin + random.NextDouble() * (cfg.ProbabiliteSortieWeekendMax - cfg.ProbabiliteSortieWeekendMin),
                FrequenceVacancesJours = cfg.FrequenceVacancesJours,
                ProbabiliteVacances = cfg.ProbabiliteVacancesMin + random.NextDouble() * (cfg.ProbabiliteVacancesMax - cfg.ProbabiliteVacancesMin),
                DureeVacancesJours = cfg.DureeVacancesJoursMin + random.Next(Math.Max(1, cfg.DureeVacancesJoursMax - cfg.DureeVacancesJoursMin + 1)),
            };
        }
    }
}
