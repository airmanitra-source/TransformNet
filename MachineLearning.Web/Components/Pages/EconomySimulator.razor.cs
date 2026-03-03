using MachineLearning.Web.Models.Agents.Companies;
using MachineLearning.Web.Models.Simulation;
using MachineLearning.Web.Models.Simulation.Config;
using Microsoft.AspNetCore.Components;

namespace MachineLearning.Web.Components.Pages;

public partial class EconomySimulator : IDisposable
{
    private ScenarioConfig _config = new();
    private List<ScenarioConfig> _scenarios = ScenarioConfig.TousLesScenarios();
    private bool _initialized = false;

    protected override void OnInitialized()
    {
        if (!_initialized)
        {
            _config = ScenarioConfig.BaseMadagascar();
            _initialized = true;
        }

        Simulator.OnTickCompleted += OnSimulationTick;
    }

    private void OnScenarioChange(ChangeEventArgs e)
    {
        var name = e.Value?.ToString();
        var scenario = _scenarios.FirstOrDefault(s => s.Name == name);
        if (scenario != null)
        {
            // Conserver l'horizon temporel choisi
            int duree = _config.DureeJours;
            _config = scenario;
            _config.DureeJours = duree;
        }
    }

    private void SetDuree(int jours)
    {
        _config.DureeJours = jours;
    }

    private async Task DemarrerTempsReel()
    {
        Simulator.Initialiser(_config);
        await Simulator.DemarrerAsync();
    }

    private async Task DemarrerRapide()
    {
        Simulator.Initialiser(_config);
        await Simulator.ExecuterRapideAsync(30);
    }

    private void Arreter()
    {
        Simulator.Arreter();
    }

    private void OnSimulationTick()
    {
        InvokeAsync(StateHasChanged);
    }

    private double Progression()
    {
        if (_config.DureeJours == 0) return 0;
        return (double)Simulator.JourCourant / _config.DureeJours * 100;
    }

    private List<DailySnapshot> GetSampledSnapshots()
    {
        var snapshots = Simulator.Result.Snapshots;
        if (snapshots.Count <= 30) return snapshots;

        // Échantillonner pour garder ~30 lignes
        int step = snapshots.Count / 30;
        var sampled = new List<DailySnapshot>();
        for (int i = 0; i < snapshots.Count; i += step)
        {
            sampled.Add(snapshots[i]);
        }
        // Toujours inclure le dernier
        if (sampled[^1] != snapshots[^1])
            sampled.Add(snapshots[^1]);
        return sampled;
    }

    private static string FormatMGA(double montant)
    {
        if (Math.Abs(montant) >= 1_000_000_000)
            return $"{montant / 1_000_000_000:F2} Mrd MGA";
        if (Math.Abs(montant) >= 1_000_000)
            return $"{montant / 1_000_000:F2} M MGA";
        if (Math.Abs(montant) >= 1_000)
            return $"{montant / 1_000:F1} k MGA";
        return $"{montant:F0} MGA";
    }

    private static string FormatTemps(int jours)
    {
        if (jours >= 365)
        {
            int ans = jours / 365;
            int moisRestants = (jours % 365) / 30;
            return moisRestants > 0 ? $"{ans} an(s) {moisRestants} mois" : $"{ans} an(s)";
        }
        if (jours >= 30)
            return $"{jours / 30} mois";
        return $"{jours} jour(s)";
    }

    private static string FormatPourcent(double finale, double initiale)
    {
        if (initiale == 0) return "N/A";
        double variation = (finale - initiale) / Math.Abs(initiale) * 100;
        string signe = variation >= 0 ? "+" : "";
        return $"{signe}{variation:F1}%";
    }

    /// <summary>
    /// Estime le Gini théorique à partir du sigma configuré.
    /// Gini = 2Φ(σ/√2) - 1 pour une loi log-normale.
    /// </summary>
    private double GiniEstime()
    {
        double x = _config.SalaireSigma / Math.Sqrt(2.0);
        double t = 1.0 / (1.0 + 0.2316419 * Math.Abs(x));
        double d = 0.3989422804014327;
        double p = d * Math.Exp(-x * x / 2.0) *
                   (t * (0.3193815 + t * (-0.3565638 + t * (1.781478 + t * (-1.8212560 + t * 1.3302744)))));
        double cdf = x >= 0 ? 1.0 - p : p;
        return 2.0 * cdf - 1.0;
    }

    /// <summary>
    /// Calcule l'écart en % entre deux valeurs de PIB.
    /// </summary>
    private static string CalculerEcart(double pib1, double pib2)
    {
        if (pib1 == 0 || pib2 == 0) return "N/A";
        double ecart = Math.Abs(pib1 - pib2) / ((pib1 + pib2) / 2.0) * 100.0;
        return ecart.ToString("F1");
    }

    private void SetTresorerie(ESecteurActivite secteur, ChangeEventArgs e)
    {
        if (double.TryParse(e.Value?.ToString(), out double val))
            _config.TresorerieInitialeParSecteur[secteur] = val;
    }

    private static string GetSecteurIcon(ESecteurActivite secteur) => secteur switch
    {
        ESecteurActivite.Agriculture => "🌾",
        ESecteurActivite.Textiles => "🧵",
        ESecteurActivite.Commerces => "🏪",
        ESecteurActivite.Services => "🔧",
        ESecteurActivite.SecteurMinier => "⛏️",
        ESecteurActivite.Construction => "🏗️",
        _ => "🏢"
    };

    private static double GetTresorerieMin(ESecteurActivite secteur) => secteur switch
    {
        ESecteurActivite.Agriculture => 100_000,
        ESecteurActivite.SecteurMinier => 5_000_000,
        _ => 500_000
    };

    private static double GetTresorerieMax(ESecteurActivite secteur) => secteur switch
    {
        ESecteurActivite.Agriculture => 5_000_000,
        ESecteurActivite.SecteurMinier => 200_000_000,
        _ => 30_000_000
    };

    private static double GetTresorerieStep(ESecteurActivite secteur) => secteur switch
    {
        ESecteurActivite.Agriculture => 50_000,
        ESecteurActivite.SecteurMinier => 1_000_000,
        _ => 500_000
    };

    public void Dispose()
    {
        Simulator.OnTickCompleted -= OnSimulationTick;
        Simulator.Arreter();
    }
}
