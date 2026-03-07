using Simulation.Module.Config;
using Company.Module.Models;
using Microsoft.AspNetCore.Components;
using Simulation.Module.Models;

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

    /// <summary>
    /// Formate un montant en milliards d'Ariary (ex: "13,53 Mds MGA").
    /// Utilisé pour les paramètres budgétaires TOFE.
    /// </summary>
    private static string FormatMds(double montant)
    {
        if (Math.Abs(montant) >= 1_000_000_000_000)
            return $"{montant / 1_000_000_000_000:F1} Tds MGA";
        if (Math.Abs(montant) >= 1_000_000_000)
            return $"{montant / 1_000_000_000:F2} Mds MGA";
        if (Math.Abs(montant) >= 1_000_000)
            return $"{montant / 1_000_000:F1} M MGA";
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

    // ═══════════════════════════════════════════
    //  RECALIBRATION MENSUELLE
    // ═══════════════════════════════════════════

    private void AjouterCibleMensuelle()
    {
        int prochainMois = _config.CiblesMensuelles.Count > 0
            ? _config.CiblesMensuelles.Max(c => c.Mois) + 1
            : 1;

        _config.CiblesMensuelles.Add(new MonthlyCalibrationTarget
        {
            Mois = prochainMois,
            Label = $"Mois {prochainMois}"
        });
    }

    private void SupprimerCibleMensuelle(int mois)
    {
        _config.CiblesMensuelles.RemoveAll(c => c.Mois == mois);
    }

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
            InvokeAsync(StateHasChanged);
        }
    }

    // Sliders bind directly to _config properties; ApplyPropensionToMenages() button applies values to households.

    private static void SetCibleM3(MonthlyCalibrationTarget cible, ChangeEventArgs e)
    {
        if (double.TryParse(e.Value?.ToString(), out double val) && val > 0)
            cible.M3Cible = val * 1_000_000_000;
        else
            cible.M3Cible = null;
    }

    private static void SetCibleFiscale(MonthlyCalibrationTarget cible, ChangeEventArgs e)
    {
        if (double.TryParse(e.Value?.ToString(), out double val) && val > 0)
            cible.RecettesFiscalesCumuleesCible = val * 1_000_000_000;
        else
            cible.RecettesFiscalesCumuleesCible = null;
    }

    private static void SetCibleExports(MonthlyCalibrationTarget cible, ChangeEventArgs e)
    {
        if (double.TryParse(e.Value?.ToString(), out double val) && val > 0)
            cible.ExportationsFOBCumuleesCible = val * 1_000_000_000;
        else
            cible.ExportationsFOBCumuleesCible = null;
    }

    private static void SetCibleImports(MonthlyCalibrationTarget cible, ChangeEventArgs e)
    {
        if (double.TryParse(e.Value?.ToString(), out double val) && val > 0)
            cible.ImportationsCIFCumuleesCible = val * 1_000_000_000;
        else
            cible.ImportationsCIFCumuleesCible = null;
    }

    private static void SetCibleTourisme(MonthlyCalibrationTarget cible, ChangeEventArgs e)
    {
        if (double.TryParse(e.Value?.ToString(), out double val) && val > 0)
            cible.DevisesTourismeCible = val * 1_000_000_000;
        else
            cible.DevisesTourismeCible = null;
    }

    private static void SetCibleCNaPS(MonthlyCalibrationTarget cible, ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out int val) && val > 0)
            cible.NouveauxAffiliesCNaPSCible = val;
        else
            cible.NouveauxAffiliesCNaPSCible = null;
    }

    private static string FormatSmallNumber(double val)
    {
        if (Math.Abs(val) < 0.001)
            return val.ToString("E2");
        if (Math.Abs(val) >= 1_000_000_000)
            return $"{val / 1_000_000_000:F2} Mds";
        if (Math.Abs(val) >= 1_000_000)
            return $"{val / 1_000_000:F1} M";
        return val.ToString("F4");
    }

    public void Dispose()
    {
        Simulator.OnTickCompleted -= OnSimulationTick;
        Simulator.Arreter();
    }
}
