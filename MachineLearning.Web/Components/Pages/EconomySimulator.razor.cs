using Simulation.Module.Config;
using Company.Module.Models;
using Microsoft.AspNetCore.Components;
using Simulation.Module.Models;
using Simulation.Module.Services;

namespace MachineLearning.Web.Components.Pages;

public partial class EconomySimulator : IDisposable
{
    private ScenarioConfig _config = new();
    private List<ScenarioConfig> _scenarios = ScenarioConfig.TousLesScenarios();
    private bool _initialized = false;
    private MacroeconomicScrapedData? _derniereCollecte;
    private InstatTbeData? _derniereCollecteInstat;

    [Inject] private IMacroeconomicDataScraperService ScraperService { get; set; } = default!;
    [Inject] private IInstatTbeScraperService InstatScraperService { get; set; } = default!;

    protected override void OnInitialized()
    {
        if (!_initialized)
        {
            _config = ScenarioConfig.BaseMadagascar();
            _initialized = true;
        }

        Simulator.OnTickCompleted += OnSimulationTick;
    }

    private void OnPropensionSubsistanceInput(ChangeEventArgs e)
    {
        if (TryParseRangeValue(e, out var v))
        {
            _config.PropensionConsommation_Subsistance = v;
            ApplyPropensionToMenages();
        }
    }

    private void OnPropensionInformelBasInput(ChangeEventArgs e)
    {
        if (TryParseRangeValue(e, out var v))
        {
            _config.PropensionConsommation_InformelBas = v;
            ApplyPropensionToMenages();
        }
    }

    private void OnPropensionFormelBasInput(ChangeEventArgs e)
    {
        if (TryParseRangeValue(e, out var v))
        {
            _config.PropensionConsommation_FormelBas = v;
            ApplyPropensionToMenages();
        }
    }

    private void OnPropensionFormelQualifieInput(ChangeEventArgs e)
    {
        if (TryParseRangeValue(e, out var v))
        {
            _config.PropensionConsommation_FormelQualifie = v;
            ApplyPropensionToMenages();
        }
    }

    private void OnPropensionCadreInput(ChangeEventArgs e)
    {
        if (TryParseRangeValue(e, out var v))
        {
            _config.PropensionConsommation_Cadre = v;
            ApplyPropensionToMenages();
        }
    }

    private static bool TryParseRangeValue(ChangeEventArgs e, out double value)
    {
        value = 0;
        if (e?.Value == null) return false;
        var s = e.Value.ToString();
        // Normalize decimal separator (handle locales that use comma)
        s = s.Replace(',', '.');
        return double.TryParse(s, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out value);
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
        _ = Simulator.DemarrerAsync();
        await InvokeAsync(StateHasChanged);
    }

    private async Task DemarrerRapide()
    {
        Simulator.Initialiser(_config);
        _ = Simulator.ExecuterRapideAsync(30);
        await InvokeAsync(StateHasChanged);
    }

    private void Arreter()
    {
        Simulator.Arreter();
    }

    private void OnSimulationTick()
    {
        _ = InvokeAsync(StateHasChanged);
    }

    private double Progression()
    {
        if (_config.DureeJours == 0) return 0;
        return (double)Simulator.JourCourant / _config.DureeJours * 100;
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
        ESecteurActivite.HotellerieTourisme => "🏨",
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

    private void ApplyPropensionToMenages()
    {
        // Appliquer immédiatement la configuration courante aux ménages existants
        try
        {
            Simulator.AppliquerPropensionConsommationParClasse(_config);
        }
        catch
        {
            // silence : si le simulateur n'est pas initialisé ou erreur, continuer et forcer le rendu
        }
        finally
        {
            // Toujours redessiner l'UI pour refléter la nouvelle valeur dans _config
            _ = InvokeAsync(StateHasChanged);
        }
    }

    public void Dispose()
    {
        Simulator.OnTickCompleted -= OnSimulationTick;
        Simulator.Arreter();
    }

    /// <summary>
    /// Collecte les agrégats macroéconomiques depuis la Banque Mondiale pour l'année demandée.
    /// Appelé par le composant RecalibrationParameters via EventCallback.
    /// </summary>
    private async Task CollecterDonneesMacro(int annee)
    {
        _derniereCollecte = await ScraperService.CollecterDonneesAsync("MDG", annee);
        await InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Applique les données scrapées au scénario en cours :
    /// 1. Met à jour les paramètres du ScenarioConfig (inflation, change, réserves, etc.)
    /// 2. Génère 12 cibles mensuelles avec profils saisonniers Madagascar
    /// 3. Active la recalibration mensuelle
    /// </summary>
    private async Task AppliquerDonneesMacro()
    {
        if (_derniereCollecte == null || !_derniereCollecte.EstExploitable) return;

        // 1. Mettre à jour les paramètres du scénario
        ScraperService.AppliquerAuScenario(_derniereCollecte, _config);

        // 2. Générer les cibles mensuelles saisonnalisées
        double facteurEchelle = _config.NombreMenages / ScenarioConfig.NombreMenagesReference;
        var cibles = ScraperService.GenererCiblesMensuelles(_derniereCollecte, facteurEchelle);

        _config.CiblesMensuelles = cibles;
        _config.RecalibrationMensuelleActivee = true;

        await InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Collecte les données du TBE INSTAT (PDF).
    /// </summary>
    private async Task CollecterInstat()
    {
        _derniereCollecteInstat = await InstatScraperService.CollecterAsync();
        await InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Applique les données TBE INSTAT aux cibles mensuelles :
    /// Remplace les estimations WB par les valeurs réelles observées.
    /// </summary>
    private async Task AppliquerInstat()
    {
        if (_derniereCollecteInstat == null || !_derniereCollecteInstat.EstExploitable) return;

        // Déterminer l'année cible (la plus récente dans les données)
        int annee = _derniereCollecteInstat.Mois.Max(m => m.Annee);

        // S'assurer qu'on a des cibles pour 12 mois
        if (_config.CiblesMensuelles.Count == 0)
        {
            string[] nomsMois = ["", "Janvier", "Février", "Mars", "Avril", "Mai", "Juin",
                "Juillet", "Août", "Septembre", "Octobre", "Novembre", "Décembre"];
            for (int i = 1; i <= 12; i++)
            {
                _config.CiblesMensuelles.Add(new MonthlyCalibrationTarget
                {
                    Mois = i,
                    Label = $"{nomsMois[i]} {annee}"
                });
            }
        }

        InstatScraperService.AppliquerAuxCibles(_derniereCollecteInstat, _config.CiblesMensuelles, annee);
        _config.RecalibrationMensuelleActivee = true;

        await InvokeAsync(StateHasChanged);
    }
}
