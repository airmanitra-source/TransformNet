using Simulation.Module.Config;
using Simulation.Module.Models;

namespace Simulation.Module.Services;

/// <summary>
/// Service de collecte automatique des agrégats macroéconomiques depuis des sources publiques.
/// Remplace l'upload CSV manuel en allant chercher directement les données
/// sur les API ouvertes (Banque Mondiale WDI, FMI WEO) pour populer
/// les paramètres du modèle de simulation.
///
/// Sources utilisées :
///   - World Bank Open Data API (https://api.worldbank.org/v2/) : PIB, inflation,
///     commerce extérieur, population, taux de change, réserves, remittances, aide.
///   - IMF World Economic Outlook (si disponible) : projections, dette publique.
///
/// Workflow typique :
///   1. CollecterDonneesAsync("MDG", 2024) → MacroeconomicScrapedData
///   2. AppliquerAuScenario(data, config)   → ScenarioConfig mis à jour
///   3. ChargerCiblesMensuelles(data)       → List&lt;MonthlyCalibrationTarget&gt;
/// </summary>
public interface IMacroeconomicDataScraperService
{
    /// <summary>
    /// Collecte les agrégats macroéconomiques depuis les API publiques
    /// pour un pays et une année donnés.
    /// </summary>
    /// <param name="codePays">Code ISO 3166-1 alpha-3 (ex: "MDG" pour Madagascar).</param>
    /// <param name="annee">Année de référence (ex: 2024). Si null, utilise la dernière disponible.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Données macroéconomiques scrapées.</returns>
    Task<MacroeconomicScrapedData> CollecterDonneesAsync(
        string codePays = "MDG",
        int? annee = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Applique les données scrapées au scénario de simulation.
    /// Met à jour les paramètres du modèle (taux d'inflation, taux de change,
    /// réserves, aide internationale, etc.) pour que la simulation
    /// reflète les fondamentaux macroéconomiques observés.
    /// </summary>
    /// <param name="data">Données macroéconomiques collectées.</param>
    /// <param name="config">Configuration du scénario à mettre à jour.</param>
    void AppliquerAuScenario(MacroeconomicScrapedData data, ScenarioConfig config);

    /// <summary>
    /// Génère des cibles de recalibration mensuelles à partir des données annuelles.
    /// Ventile les agrégats annuels en 12 cibles mensuelles en appliquant
    /// un profil saisonnier (soudure, récolte, cyclones) pour Madagascar.
    /// </summary>
    /// <param name="data">Données macroéconomiques annuelles collectées.</param>
    /// <param name="facteurEchelle">Facteur d'échelle simulation/réalité.</param>
    /// <returns>12 cibles mensuelles pour la recalibration automatique.</returns>
    List<MonthlyCalibrationTarget> GenererCiblesMensuelles(
        MacroeconomicScrapedData data,
        double facteurEchelle);
}
