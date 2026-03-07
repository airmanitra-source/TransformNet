using Microsoft.Extensions.DependencyInjection;
using Simulation.Module.Config;
using Simulation.Module.Models;

namespace Simulation.Infrastructure.Services;

/// <summary>
/// Résultat d'une analyse de sensibilité pour un paramètre.
/// Mesure l'impact d'une perturbation ΔP sur le score global de validation.
/// </summary>
public class SensitivityResult
{
    /// <summary>Nom du paramètre perturbé (ex: "ProductiviteParEmploye").</summary>
    public string Parametre { get; set; } = "";

    /// <summary>Perturbation appliquée en % (ex: +10, -10).</summary>
    public double PerturbationPourcent { get; set; }

    /// <summary>Score global baseline (sans perturbation).</summary>
    public double ScoreBase { get; set; }

    /// <summary>Score global après perturbation.</summary>
    public double ScorePerturbe { get; set; }

    /// <summary>Variation du score = ScorePerturbe - ScoreBase.</summary>
    public double DeltaScore => ScorePerturbe - ScoreBase;

    /// <summary>
    /// Élasticité = ΔScore / ΔParamètre%.
    /// Mesure la sensibilité du score au paramètre.
    /// Plus |Elasticite| est grand, plus le paramètre est critique.
    /// </summary>
    public double Elasticite => PerturbationPourcent != 0
        ? DeltaScore / PerturbationPourcent
        : 0;
}

/// <summary>
/// Contrat pour l'analyse de sensibilité paramétrique.
///
/// Principe : exécute N simulations rapides en perturbant chaque paramètre clé
/// de ±X% et mesure l'impact sur le score global de validation macro.
///
/// Cas d'usage :
///   - Identifier les 5 paramètres les plus influents avant calibration manuelle
///   - Vérifier qu'un paramètre non observable (ex: PartSecteurInformel) n'a pas
///     un impact disproportionné sur les résultats
///   - Prioriser les efforts de collecte de données terrain
/// </summary>
public interface ISensitivityAnalysisService
{
    /// <summary>
    /// Exécute l'analyse de sensibilité sur les paramètres clés.
    /// </summary>
    /// <param name="configBase">Configuration baseline (scénario de référence).</param>
    /// <param name="perturbationPourcent">Amplitude de perturbation (ex: 10 = ±10%).</param>
    /// <param name="dureeSimulationJours">Durée de chaque simulation rapide (jours).</param>
    /// <returns>Résultats triés par impact décroissant (|Elasticite|).</returns>
    List<SensitivityResult> Analyser(
        ScenarioConfig configBase,
        double perturbationPourcent = 10.0,
        int dureeSimulationJours = 90);
}

/// <summary>
/// Implémentation de l'analyse de sensibilité par perturbation paramétrique.
///
/// Pour chaque paramètre clé :
///   1. Exécuter la simulation baseline → score S₀
///   2. Perturber le paramètre de +X% → simulation → score S₊
///   3. Perturber le paramètre de -X% → simulation → score S₋
///   4. Calculer l'élasticité = (S₊ - S₋) / (2 × X%)
///
/// Les paramètres testés sont ceux qui pilotent les 5 catégories
/// de validation macro : Production, Commerce, Fiscal, Monétaire, Inégalités.
/// </summary>
public class SensitivityAnalysisService : ISensitivityAnalysisService
{
    private readonly IServiceProvider _serviceProvider;

    public SensitivityAnalysisService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Paramètres clés à tester avec leur getter/setter sur ScenarioConfig.
    /// </summary>
    private static readonly List<(string Nom, Func<ScenarioConfig, double> Get, Action<ScenarioConfig, double> Set)> ParametresCles =
    [
        // Production
        ("ProductiviteParEmploye", c => c.ProductiviteParEmploye, (c, v) => c.ProductiviteParEmploye = v),
        ("NombreEntreprises", c => c.NombreEntreprises, (c, v) => c.NombreEntreprises = Math.Max(1, (int)v)),
        ("MargeBeneficiaireEntreprise", c => c.MargeBeneficiaireEntreprise, (c, v) => c.MargeBeneficiaireEntreprise = v),

        // Commerce extérieur
        ("PartEntreprisesAgricoles", c => c.PartEntreprisesAgricoles, (c, v) => c.PartEntreprisesAgricoles = v),

        // Fiscal
        ("PartSecteurInformel", c => c.PartSecteurInformel, (c, v) => c.PartSecteurInformel = Math.Clamp(v, 0, 1)),
        ("TauxTVA", c => c.TauxTVA, (c, v) => c.TauxTVA = v),
        ("TauxIS", c => c.TauxIS, (c, v) => c.TauxIS = v),
        ("DepensesPubliquesJour", c => c.DepensesPubliquesJour, (c, v) => c.DepensesPubliquesJour = v),
        ("NombreFonctionnaires", c => c.NombreFonctionnaires, (c, v) => c.NombreFonctionnaires = Math.Max(1, (int)v)),

        // Monétaire
        ("CroissanceCreditJour", c => c.CroissanceCreditJour, (c, v) => c.CroissanceCreditJour = v),
        ("TauxReserveObligatoire", c => c.TauxReserveObligatoire, (c, v) => c.TauxReserveObligatoire = Math.Clamp(v, 0, 1)),
        ("TauxDirecteur", c => c.TauxDirecteur, (c, v) => c.TauxDirecteur = v),

        // Inégalités
        ("SalaireMedian", c => c.SalaireMedian, (c, v) => c.SalaireMedian = v),
        ("SalaireSigma", c => c.SalaireSigma, (c, v) => c.SalaireSigma = v),

        // Inflation
        ("PrixCarburantLitre", c => c.PrixCarburantLitre, (c, v) => c.PrixCarburantLitre = v),
        ("ElasticiteCarburantInflation", c => c.ElasticiteCarburantInflation, (c, v) => c.ElasticiteCarburantInflation = v),
        ("ElasticiteSalairesInflation", c => c.ElasticiteSalairesInflation, (c, v) => c.ElasticiteSalairesInflation = v),

        // Remittances
        ("RemittancesJour", c => c.RemittancesJour, (c, v) => c.RemittancesJour = v),
        ("ElasticiteRemittancesChange", c => c.ElasticiteRemittancesChange, (c, v) => c.ElasticiteRemittancesChange = v),
    ];

    public List<SensitivityResult> Analyser(
        ScenarioConfig configBase,
        double perturbationPourcent = 10.0,
        int dureeSimulationJours = 90)
    {
        var resultats = new List<SensitivityResult>();

        // Exécuter la simulation baseline
        double scoreBase = ExecuterSimulationRapide(configBase, dureeSimulationJours);

        foreach (var (nom, get, set) in ParametresCles)
        {
            double valeurBase = get(configBase);
            if (valeurBase == 0) continue; // Pas de perturbation possible

            // Perturbation positive (+X%)
            var configPlus = ClonerConfig(configBase);
            set(configPlus, valeurBase * (1.0 + perturbationPourcent / 100.0));
            configPlus.DureeJours = dureeSimulationJours;
            double scorePlus = ExecuterSimulationRapide(configPlus, dureeSimulationJours);

            resultats.Add(new SensitivityResult
            {
                Parametre = nom,
                PerturbationPourcent = perturbationPourcent,
                ScoreBase = scoreBase,
                ScorePerturbe = scorePlus,
            });

            // Perturbation négative (-X%)
            var configMoins = ClonerConfig(configBase);
            set(configMoins, valeurBase * (1.0 - perturbationPourcent / 100.0));
            configMoins.DureeJours = dureeSimulationJours;
            double scoreMoins = ExecuterSimulationRapide(configMoins, dureeSimulationJours);

            resultats.Add(new SensitivityResult
            {
                Parametre = nom,
                PerturbationPourcent = -perturbationPourcent,
                ScoreBase = scoreBase,
                ScorePerturbe = scoreMoins,
            });
        }

        return resultats
            .OrderByDescending(r => Math.Abs(r.Elasticite))
            .ToList();
    }

    private double ExecuterSimulationRapide(ScenarioConfig config, int dureeJours)
    {
        using var scope = _serviceProvider.CreateScope();
        var simulation = scope.ServiceProvider.GetRequiredService<Module.ISimulationModule>();
        var validation = scope.ServiceProvider.GetRequiredService<IMacroValidationModule>();

        config.DureeJours = dureeJours;
        simulation.Initialiser(config);
        simulation.SimulerNJoursSync(dureeJours);

        var rapport = validation.Valider(simulation.Result, config, config.DonneesReference);
        return rapport.ScoreGlobal;
    }

    /// <summary>
    /// Clone une configuration en copiant toutes les propriétés simples.
    /// Note : les dictionnaires (FOBCalibresJour, etc.) sont copiés par référence
    /// (suffisant pour l'analyse de sensibilité sur les paramètres scalaires).
    /// </summary>
    private static ScenarioConfig ClonerConfig(ScenarioConfig source)
    {
        // Utiliser la sérialisation JSON pour un clone profond simple
        var json = System.Text.Json.JsonSerializer.Serialize(source);
        return System.Text.Json.JsonSerializer.Deserialize<ScenarioConfig>(json) ?? new();
    }
}
