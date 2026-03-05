using MachineLearning.Web.Models.Simulation;
using MachineLearning.Web.Models.Simulation.Config;
using Microsoft.AspNetCore.Components;

namespace MachineLearning.Web.Components.Pages;

public partial class AIDSGrowthSimulator
{
    [Inject] private EconomicSimulatorViewModel Simulator { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private AIDSGrowthSimulatorViewModel _model = new();
    private bool _showAdvanced = false;
    private int _horizonPreset = 365;
    private ScenarioConfigViewModel? _scenarioGenere;

    /// <summary>Indique si les résultats de simulation sont disponibles pour import.</summary>
    private bool SimulationDisponible =>
        Simulator.Result.EstTerminee && Simulator.Result.Snapshots.Count >= 2;

    private void SetHorizon(int jours)
    {
        _horizonPreset = jours;
        _model.HorizonJours = jours;
    }

    private void LancerEstimation()
    {
        // Normaliser les parts budgétaires
        double somme = _model.PartsBudgetaires.Sum();
        if (somme > 0 && Math.Abs(somme - 1.0) > 0.01)
        {
            for (int i = 0; i < _model.N; i++)
                _model.PartsBudgetaires[i] /= somme;
        }

        _model.Estimer();
    }

    private void Reinitialiser()
    {
        _model = new AIDSGrowthSimulatorViewModel();
        _horizonPreset = 365;
    }

    private static string FormatMGA(double montant)
    {
        if (Math.Abs(montant) >= 1_000_000_000_000)
            return $"{montant / 1_000_000_000_000:F2} Tds MGA";
        if (Math.Abs(montant) >= 1_000_000_000)
            return $"{montant / 1_000_000_000:F2} Mds MGA";
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

    private static string GetBadgeClass(ETypeRecommandation type) => type switch
    {
        ETypeRecommandation.Croissance => "bg-success",
        ETypeRecommandation.Decroissance => "bg-warning text-dark",
        ETypeRecommandation.Alerte => "bg-danger",
        ETypeRecommandation.Structurel => "bg-info",
        _ => "bg-secondary"
    };

    private static string GetBadgeLabel(ETypeRecommandation type) => type switch
    {
        ETypeRecommandation.Croissance => "📈 Croissance",
        ETypeRecommandation.Decroissance => "📉 Engel",
        ETypeRecommandation.Alerte => "⚠️ Alerte",
        ETypeRecommandation.Structurel => "🔧 Structurel",
        _ => "ℹ️ Info"
    };

    private void OnPartBudgetaireChanged(int index, ChangeEventArgs e)
    {
        if (double.TryParse(e.Value?.ToString(), System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out double val))
        {
            _model.PartsBudgetaires[index] = val;
        }
    }

    private void OnPrixActuelChanged(int index, ChangeEventArgs e)
    {
        if (double.TryParse(e.Value?.ToString(), System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out double val))
        {
            _model.IndicesPrix[index] = val;
        }
    }

    private void OnPrixProjeteChanged(int index, ChangeEventArgs e)
    {
        if (double.TryParse(e.Value?.ToString(), System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out double val))
        {
            _model.IndicesPrixProjectes[index] = val;
        }
    }

    // ═══════════════════════════════════════════
    //  INTÉGRATION : Simulation ↔ AIDS
    // ═══════════════════════════════════════════

    /// <summary>
    /// Importe les parts budgétaires, M3 et indices de prix
    /// depuis la dernière simulation terminée.
    /// </summary>
    private void ImporterDepuisSimulation()
    {
        if (!SimulationDisponible) return;

        _model = new AIDSGrowthSimulatorViewModel();
        _model.ChargerDepuisSimulation(Simulator.Result);
        _horizonPreset = _model.HorizonJours;
        _scenarioGenere = null;
    }

    /// <summary>
    /// Génère un ScenarioConfigViewModel à partir des projections AIDS
    /// et navigue vers le simulateur économique avec ce scénario pré-rempli.
    /// </summary>
    private void ExporterVersSimulation()
    {
        if (!_model.EstEstime) return;
        _scenarioGenere = _model.GenererScenarioConfig();
    }

    /// <summary>
    /// Navigue vers le simulateur avec le scénario AIDS pré-rempli.
    /// Le scénario est ajouté à la liste des scénarios disponibles.
    /// </summary>
    private void LancerSimulationAIDS()
    {
        if (_scenarioGenere == null) return;
        Navigation.NavigateTo("/economie");
    }
}
