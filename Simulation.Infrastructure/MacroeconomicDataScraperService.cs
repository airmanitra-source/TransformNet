using System.Net.Http.Json;
using System.Text.Json;
using Simulation.Module.Config;
using Simulation.Module.Models;
using Simulation.Module.Services;

namespace Simulation.Infrastructure;

/// <summary>
/// Implémentation du service de collecte automatique des agrégats macroéconomiques.
/// Scrape l'API ouverte de la Banque Mondiale (WDI) et déduit les paramètres du modèle.
///
/// API Banque Mondiale : https://api.worldbank.org/v2/
///   Format : /country/{code}/indicator/{indicateur}?date={annee}&amp;format=json
///   Aucune clé API requise (données publiques).
///
/// Indicateurs collectés (codes WDI) :
///   NY.GDP.MKTP.CN  — PIB nominal (LCU)
///   NY.GDP.MKTP.CD  — PIB nominal (USD)
///   NY.GDP.MKTP.KD.ZG — Croissance PIB réel (%)
///   NY.GDP.PCAP.CN  — PIB/hab (LCU)
///   FP.CPI.TOTL.ZG  — Inflation IPC (%)
///   NE.EXP.GNFS.CD  — Exportations (USD)
///   NE.IMP.GNFS.CD  — Importations (USD)
///   PA.NUS.FCRF     — Taux de change officiel (LCU/USD)
///   FI.RES.TOTL.CD  — Réserves totales (USD)
///   FM.LBL.BMNY.GD.ZS — M2/PIB (%)
///   BX.TRF.PWKR.CD.DT — Remittances reçues (USD)
///   GC.TAX.TOTL.GD.ZS — Pression fiscale (% PIB)
///   SP.POP.TOTL     — Population totale
///   SP.POP.GROW     — Croissance démographique (%)
///   SP.URB.TOTL.IN.ZS — Population urbaine (%)
///   SL.AGR.EMPL.ZS  — Emploi agricole (%)
///   EG.ELC.ACCS.ZS  — Accès électricité (%)
///   DT.ODA.ODAT.CD  — Aide publique au développement (USD)
///   ST.INT.RCPT.CD  — Recettes touristiques internationales (USD)
/// </summary>
public class MacroeconomicDataScraperService : IMacroeconomicDataScraperService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://api.worldbank.org/v2";

    // Mapping indicateur WDI → propriété du modèle
    private static readonly Dictionary<string, string> Indicateurs = new()
    {
        ["NY.GDP.MKTP.CN"]    = nameof(MacroeconomicScrapedData.PIBNominalAnnuelLCU),
        ["NY.GDP.MKTP.CD"]    = nameof(MacroeconomicScrapedData.PIBNominalAnnuelUSD),
        ["NY.GDP.MKTP.KD.ZG"] = nameof(MacroeconomicScrapedData.CroissancePIBReel),
        ["NY.GDP.PCAP.CN"]    = nameof(MacroeconomicScrapedData.PIBParHabitantLCU),
        ["FP.CPI.TOTL.ZG"]    = nameof(MacroeconomicScrapedData.TauxInflationAnnuel),
        ["NE.EXP.GNFS.CD"]    = nameof(MacroeconomicScrapedData.ExportationsUSD),
        ["NE.IMP.GNFS.CD"]    = nameof(MacroeconomicScrapedData.ImportationsUSD),
        ["PA.NUS.FCRF"]       = nameof(MacroeconomicScrapedData.TauxChangeOfficielLCUParUSD),
        ["FI.RES.TOTL.CD"]    = nameof(MacroeconomicScrapedData.ReservesTotalesUSD),
        ["FM.LBL.BMNY.GD.ZS"] = nameof(MacroeconomicScrapedData.RatioM2PIB),
        ["BX.TRF.PWKR.CD.DT"] = nameof(MacroeconomicScrapedData.RemittancesRecuesUSD),
        ["GC.TAX.TOTL.GD.ZS"] = nameof(MacroeconomicScrapedData.PressionFiscalePIB),
        ["SP.POP.TOTL"]       = nameof(MacroeconomicScrapedData.PopulationTotale),
        ["SP.POP.GROW"]       = nameof(MacroeconomicScrapedData.CroissanceDemographique),
        ["SP.URB.TOTL.IN.ZS"] = nameof(MacroeconomicScrapedData.PartPopulationUrbaine),
        ["SL.AGR.EMPL.ZS"]    = nameof(MacroeconomicScrapedData.PartEmploiAgriculture),
        ["EG.ELC.ACCS.ZS"]    = nameof(MacroeconomicScrapedData.TauxAccesElectricite),
        ["DT.ODA.ODAT.CD"]    = nameof(MacroeconomicScrapedData.AidePubliqueDevUSD),
        ["ST.INT.RCPT.CD"]    = nameof(MacroeconomicScrapedData.RecettesTouristiquesUSD),
    };

    public MacroeconomicDataScraperService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress ??= new Uri(BaseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    /// <inheritdoc />
    public async Task<MacroeconomicScrapedData> CollecterDonneesAsync(
        string codePays = "MDG",
        int? annee = null,
        CancellationToken cancellationToken = default)
    {
        int anneeRecherche = annee ?? DateTime.UtcNow.Year - 1;
        // La Banque Mondiale a souvent 1-2 ans de retard → chercher sur une plage élargie
        string plageAnnees = $"{anneeRecherche - 4}:{anneeRecherche}";

        var result = new MacroeconomicScrapedData
        {
            CodePays = codePays,
            Annee = anneeRecherche,
            DateCollecte = DateTime.UtcNow,
        };

        // Requêtes individuelles par indicateur (le batch `;` n'est pas supporté par l'API WB v2)
        await CollecterIndicateursIndividuellement(result, codePays, plageAnnees, cancellationToken);

        if (result.EstExploitable)
            result.Sources.Add($"World Bank WDI ({result.Annee})");

        // Convertir les pourcentages WB (ex: 8.5) en fractions (0.085) pour les taux
        NormaliserPourcentages(result);

        return result;
    }

    /// <summary>
    /// Fallback : interroge chaque indicateur individuellement si le lot échoue.
    /// </summary>
    private async Task CollecterIndicateursIndividuellement(
        MacroeconomicScrapedData result,
        string codePays,
        string plageAnnees,
        CancellationToken ct)
    {
        foreach (var (code, _) in Indicateurs)
        {
            try
            {
                var url = $"/v2/country/{codePays}/indicator/{code}?date={plageAnnees}&format=json&per_page=10";
                var response = await _httpClient.GetAsync(url, ct);
                if (!response.IsSuccessStatusCode)
                {
                    result.Erreurs.Add($"{code}: HTTP {response.StatusCode}");
                    continue;
                }

                var json = await response.Content.ReadAsStringAsync(ct);
                ParseWorldBankSingleResponse(json, code, result);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
            {
                result.Erreurs.Add($"{code}: {ex.Message}");
            }
        }
    }

    private void ParseWorldBankSingleResponse(string json, string indicatorCode, MacroeconomicScrapedData result)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        if (root.ValueKind != JsonValueKind.Array || root.GetArrayLength() < 2)
            return;

        var dataArray = root[1];
        if (dataArray.ValueKind != JsonValueKind.Array)
            return;

        // Prendre la valeur non-null la plus récente
        double? bestValue = null;
        int bestYear = 0;

        foreach (var item in dataArray.EnumerateArray())
        {
            if (item.GetProperty("value").ValueKind == JsonValueKind.Null)
                continue;
            if (!item.GetProperty("value").TryGetDouble(out double valeur))
                continue;
            if (!int.TryParse(item.GetProperty("date").GetString(), out int annee))
                continue;

            if (annee > bestYear)
            {
                bestYear = annee;
                bestValue = valeur;
            }
        }

        if (bestValue.HasValue)
        {
            AffecterValeur(result, indicatorCode, bestValue.Value);
        }
        else
        {
            result.Erreurs.Add($"{indicatorCode}: aucune valeur disponible sur la plage");
        }
    }

    /// <summary>
    /// Affecte une valeur WDI à la propriété correspondante du modèle.
    /// </summary>
    private static void AffecterValeur(MacroeconomicScrapedData result, string indicatorCode, double valeur)
    {
        switch (indicatorCode)
        {
            case "NY.GDP.MKTP.CN":    result.PIBNominalAnnuelLCU = valeur; break;
            case "NY.GDP.MKTP.CD":    result.PIBNominalAnnuelUSD = valeur; break;
            case "NY.GDP.MKTP.KD.ZG": result.CroissancePIBReel = valeur; break;
            case "NY.GDP.PCAP.CN":    result.PIBParHabitantLCU = valeur; break;
            case "FP.CPI.TOTL.ZG":    result.TauxInflationAnnuel = valeur; break;
            case "NE.EXP.GNFS.CD":    result.ExportationsUSD = valeur; break;
            case "NE.IMP.GNFS.CD":    result.ImportationsUSD = valeur; break;
            case "PA.NUS.FCRF":       result.TauxChangeOfficielLCUParUSD = valeur; break;
            case "FI.RES.TOTL.CD":    result.ReservesTotalesUSD = valeur; break;
            case "FM.LBL.BMNY.GD.ZS": result.RatioM2PIB = valeur; break;
            case "BX.TRF.PWKR.CD.DT": result.RemittancesRecuesUSD = valeur; break;
            case "GC.TAX.TOTL.GD.ZS": result.PressionFiscalePIB = valeur; break;
            case "SP.POP.TOTL":       result.PopulationTotale = valeur; break;
            case "SP.POP.GROW":       result.CroissanceDemographique = valeur; break;
            case "SP.URB.TOTL.IN.ZS": result.PartPopulationUrbaine = valeur; break;
            case "SL.AGR.EMPL.ZS":    result.PartEmploiAgriculture = valeur; break;
            case "EG.ELC.ACCS.ZS":    result.TauxAccesElectricite = valeur; break;
            case "DT.ODA.ODAT.CD":    result.AidePubliqueDevUSD = valeur; break;
            case "ST.INT.RCPT.CD":    result.RecettesTouristiquesUSD = valeur; break;
        }
    }

    /// <summary>
    /// Les valeurs WB sont en pourcentage brut (ex: 8.5 pour 8.5%).
    /// Convertit en fraction (0.085) pour les taux du modèle.
    /// </summary>
    private static void NormaliserPourcentages(MacroeconomicScrapedData data)
    {
        if (data.CroissancePIBReel.HasValue)
            data.CroissancePIBReel /= 100.0;
        if (data.TauxInflationAnnuel.HasValue)
            data.TauxInflationAnnuel /= 100.0;
        if (data.RatioM2PIB.HasValue)
            data.RatioM2PIB /= 100.0;
        if (data.PressionFiscalePIB.HasValue)
            data.PressionFiscalePIB /= 100.0;
        if (data.CroissanceDemographique.HasValue)
            data.CroissanceDemographique /= 100.0;
        if (data.PartPopulationUrbaine.HasValue)
            data.PartPopulationUrbaine /= 100.0;
        if (data.PartEmploiAgriculture.HasValue)
            data.PartEmploiAgriculture /= 100.0;
        if (data.TauxAccesElectricite.HasValue)
            data.TauxAccesElectricite /= 100.0;
    }

    /// <inheritdoc />
    public void AppliquerAuScenario(MacroeconomicScrapedData data, ScenarioConfig config)
    {
        if (!data.EstExploitable) return;

        double tauxChange = data.TauxChangeOfficielLCUParUSD ?? config.TauxChangeMGAParUSD;

        // ── Inflation ──────────────────────────────────────────────────────
        if (data.TauxInflationAnnuel.HasValue)
        {
            config.TauxInflation = data.TauxInflationAnnuel.Value;
            config.InflationAncrage = Math.Max(0.03, data.TauxInflationAnnuel.Value * 0.75);
        }

        // ── Taux de change ─────────────────────────────────────────────────
        if (data.TauxChangeOfficielLCUParUSD.HasValue)
        {
            config.TauxChangeMGAParUSD = data.TauxChangeOfficielLCUParUSD.Value;
        }

        // ── Réserves de change ─────────────────────────────────────────────
        if (data.ReservesTotalesUSD.HasValue)
        {
            config.ReservesBCMUSD = data.ReservesTotalesUSD.Value;
        }

        // ── Remittances (transferts diaspora) ──────────────────────────────
        // Convertir annuel USD → journalier MGA
        if (data.RemittancesRecuesUSD.HasValue)
        {
            config.RemittancesJour = (data.RemittancesRecuesUSD.Value * tauxChange) / 365.0;
        }

        // ── Aide internationale ────────────────────────────────────────────
        // Convertir annuel USD → journalier MGA
        if (data.AidePubliqueDevUSD.HasValue)
        {
            config.AideInternationaleJour = (data.AidePubliqueDevUSD.Value * tauxChange) / 365.0;
        }

        // ── Croissance démographique ───────────────────────────────────────
        if (data.CroissanceDemographique.HasValue)
        {
            config.TauxCroissanceDemographiqueAnnuel = data.CroissanceDemographique.Value;
        }

        // ── Urbanisation ───────────────────────────────────────────────────
        if (data.PartPopulationUrbaine.HasValue)
        {
            config.PartMenagesUrbains = data.PartPopulationUrbaine.Value;
        }

        // ── Accès électricité ──────────────────────────────────────────────
        if (data.TauxAccesElectricite.HasValue)
        {
            config.TauxAccesElectricite = data.TauxAccesElectricite.Value;
            // Répartir urbain/rural (~65%/8% est le ratio structurel Madagascar)
            double ratioUrbainRural = 0.65 / 0.08; // 8.125
            double tauxGlobal = data.TauxAccesElectricite.Value;
            double partUrbain = config.PartMenagesUrbains;
            // tauxGlobal = partUrbain × tauxUrbain + (1-partUrbain) × tauxRural
            // tauxUrbain = ratioUrbainRural × tauxRural
            double tauxRural = tauxGlobal / (partUrbain * ratioUrbainRural + (1 - partUrbain));
            config.TauxAccesElectriciteRural = Math.Clamp(tauxRural, 0, 1);
            config.TauxAccesElectriciteUrbain = Math.Clamp(tauxRural * ratioUrbainRural, 0, 1);
        }

        // ── Structure sectorielle → part entreprises agricoles ─────────────
        if (data.PartEmploiAgriculture.HasValue)
        {
            config.PartEntreprisesAgricoles = Math.Clamp(data.PartEmploiAgriculture.Value, 0.10, 0.60);
        }

        // ── Croissance PIB potentiel ───────────────────────────────────────
        if (data.CroissancePIBReel.HasValue)
        {
            config.CroissancePIBPotentielAnnuel = data.CroissancePIBReel.Value;
        }

        // ── Pression fiscale → dépenses publiques ──────────────────────────
        if (data.PressionFiscalePIB.HasValue && data.PIBNominalAnnuelLCU.HasValue)
        {
            double recettesFiscalesAnnuelles = data.PIBNominalAnnuelLCU.Value * data.PressionFiscalePIB.Value;
            // Les dépenses publiques ≈ recettes fiscales + aide (solde ~ -2% PIB)
            config.DepensesPubliquesJour = recettesFiscalesAnnuelles / 365.0;
        }

        // ── Données de référence pour la validation macro ──────────────────
        if (data.PIBNominalAnnuelLCU.HasValue)
            config.DonneesReference.PIBNominalAnnuel = data.PIBNominalAnnuelLCU.Value;
        if (data.TauxInflationAnnuel.HasValue)
            config.DonneesReference.TauxInflationAnnuel = data.TauxInflationAnnuel.Value;
        if (data.ExportationsUSD.HasValue)
            config.DonneesReference.ExportationsFOBAnnuelles = data.ExportationsUSD.Value * tauxChange;
        if (data.ImportationsUSD.HasValue)
            config.DonneesReference.ImportationsCIFAnnuelles = data.ImportationsUSD.Value * tauxChange;
        if (data.ReservesTotalesUSD.HasValue)
            config.DonneesReference.TauxDirecteur = config.TauxDirecteur;

        config.DonneesReference.Annee = data.Annee;
        config.DonneesReference.Source = $"World Bank WDI auto-scraped ({data.DateCollecte:yyyy-MM-dd})";
    }

    /// <inheritdoc />
    public List<MonthlyCalibrationTarget> GenererCiblesMensuelles(
        MacroeconomicScrapedData data,
        double facteurEchelle)
    {
        var cibles = new List<MonthlyCalibrationTarget>();
        if (!data.EstExploitable) return cibles;

        double tauxChange = data.TauxChangeOfficielLCUParUSD ?? 4_500;

        // Profils saisonniers Madagascar (poids mensuels normalisés à 12)
        // Basés sur les données INSTAT/BCM : imports plus élevés en Q1 (cyclones),
        // exports soutenus en Q3 (vanille, litchis), tourisme en Q3-Q4.
        double[] profilImports   = [1.10, 1.15, 1.05, 0.95, 0.90, 0.85, 0.90, 0.95, 1.00, 1.05, 1.05, 1.05];
        double[] profilExports   = [0.80, 0.75, 0.85, 0.90, 1.00, 1.10, 1.20, 1.15, 1.10, 1.05, 0.95, 0.85]; // vanille pic Q3
        double[] profilM3        = [0.98, 0.97, 0.98, 0.99, 1.00, 1.00, 1.01, 1.01, 1.02, 1.02, 1.03, 1.03]; // croissance progressive
        double[] profilTourisme  = [0.70, 0.60, 0.65, 0.80, 0.90, 1.00, 1.30, 1.40, 1.40, 1.20, 1.10, 0.95]; // haute saison juil-sept
        double[] profilCNaPS     = [1.20, 0.90, 0.95, 1.00, 1.05, 1.00, 0.90, 0.85, 1.10, 1.15, 1.00, 0.90]; // embauches pic janv + Q4

        // Convertir les agrégats annuels USD → LCU mensuels cumulés
        double? importsCIFAnnuelLCU = data.ImportationsUSD.HasValue
            ? data.ImportationsUSD.Value * tauxChange * facteurEchelle
            : null;
        double? exportsFOBAnnuelLCU = data.ExportationsUSD.HasValue
            ? data.ExportationsUSD.Value * tauxChange * facteurEchelle
            : null;

        // M3 : estimer à partir du ratio M2/PIB si disponible
        double? m3Annuel = data.RatioM2PIB.HasValue && data.PIBNominalAnnuelLCU.HasValue
            ? data.PIBNominalAnnuelLCU.Value * data.RatioM2PIB.Value * 1.10 * facteurEchelle // M3 ≈ M2 × 1.10
            : null;

        // Recettes fiscales annuelles LCU
        double? recettesFiscalesAnnuelLCU = data.PressionFiscalePIB.HasValue && data.PIBNominalAnnuelLCU.HasValue
            ? data.PIBNominalAnnuelLCU.Value * data.PressionFiscalePIB.Value * facteurEchelle
            : null;

        // Recettes touristiques annuelles LCU (WB: ST.INT.RCPT.CD en USD → MGA)
        double? tourismeLCUAnnuel = data.RecettesTouristiquesUSD.HasValue
            ? data.RecettesTouristiquesUSD.Value * tauxChange * facteurEchelle
            : null;

        // Nouveaux affiliés CNaPS annuels : estimation structurelle
        // Madagascar ~350k emplois formels, croissance ~2-3%/an → ~7k-10k nouveaux/an
        // Ajusté par la croissance PIB réel si disponible
        int? nouveauxCNaPSAnnuel = null;
        if (data.PopulationTotale.HasValue)
        {
            // Population active formelle ≈ 1.2% de la population totale (ratio structurel Madagascar)
            double emploisFormels = data.PopulationTotale.Value * 0.012;
            double tauxCroissance = data.CroissancePIBReel ?? 0.04; // croissance réelle ou 4% par défaut
            nouveauxCNaPSAnnuel = (int)(emploisFormels * Math.Max(0.02, tauxCroissance) * facteurEchelle);
        }

        string[] nomsMois = ["Janvier", "Février", "Mars", "Avril", "Mai", "Juin",
                             "Juillet", "Août", "Septembre", "Octobre", "Novembre", "Décembre"];

        double cumulImports = 0, cumulExports = 0, cumulFiscal = 0;

        for (int mois = 1; mois <= 12; mois++)
        {
            int idx = mois - 1;

            // Calculer la part mensuelle avec profil saisonnier
            double partMoisImports = profilImports[idx] / profilImports.Sum();
            double partMoisExports = profilExports[idx] / profilExports.Sum();
            double partMoisFiscal = 1.0 / 12.0; // fiscalité plus régulière
            double partMoisTourisme = profilTourisme[idx] / profilTourisme.Sum();
            double partMoisCNaPS = profilCNaPS[idx] / profilCNaPS.Sum();

            if (importsCIFAnnuelLCU.HasValue)
                cumulImports += importsCIFAnnuelLCU.Value * partMoisImports;
            if (exportsFOBAnnuelLCU.HasValue)
                cumulExports += exportsFOBAnnuelLCU.Value * partMoisExports;
            if (recettesFiscalesAnnuelLCU.HasValue)
                cumulFiscal += recettesFiscalesAnnuelLCU.Value * partMoisFiscal;

            var cible = new MonthlyCalibrationTarget
            {
                Mois = mois,
                Label = $"{nomsMois[idx]} {data.Annee}",
                ImportationsCIFCumuleesCible = importsCIFAnnuelLCU.HasValue ? cumulImports : null,
                ExportationsFOBCumuleesCible = exportsFOBAnnuelLCU.HasValue ? cumulExports : null,
                RecettesFiscalesCumuleesCible = recettesFiscalesAnnuelLCU.HasValue ? cumulFiscal : null,
                M3Cible = m3Annuel.HasValue ? m3Annuel.Value * profilM3[idx] : null,
                DevisesTourismeCible = tourismeLCUAnnuel.HasValue
                    ? tourismeLCUAnnuel.Value * partMoisTourisme
                    : null,
                NouveauxAffiliesCNaPSCible = nouveauxCNaPSAnnuel.HasValue
                    ? (int)Math.Round(nouveauxCNaPSAnnuel.Value * partMoisCNaPS)
                    : null,
            };

            cibles.Add(cible);
        }

        return cibles;
    }
}
