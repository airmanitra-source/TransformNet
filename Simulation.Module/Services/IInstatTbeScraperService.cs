using Simulation.Module.Config;
using Simulation.Module.Models;

namespace Simulation.Module.Services;

/// <summary>
/// Service de scraping du Tableau de Bord de l'Économie (TBE) publié par l'INSTAT Madagascar.
///
/// Le TBE est un PDF trimestriel (~40 pages) contenant des séries mensuelles
/// couvrant les indicateurs clés de l'économie malgache :
///   - M3, crédit à l'économie (Tableau 31 — source BFM)
///   - Recettes fiscales / TOFE (Tableau 29 — source DGT/MEF)
///   - Exportations FOB (Tableau 32 — source DSE/INSTAT)
///   - Importations CIF (Tableau 33 — source DSE/INSTAT)
///   - Tourisme : arrivées et devises (Tableau 34 — source MTA)
///   - Nouveaux affiliés CNaPS (Tableau 26 — source CNaPS)
///
/// URL type : https://www.instat.mg/documents/upload/main/INSTAT_TBE60_10-2025.pdf
///
/// Workflow :
///   1. CollecterAsync() → InstatTbeData (télécharge le dernier TBE, parse le PDF)
///   2. AppliquerAuxCibles(data, cibles) → enrichit les MonthlyCalibrationTarget existants
/// </summary>
public interface IInstatTbeScraperService
{
    /// <summary>
    /// Télécharge et parse le dernier TBE disponible sur instat.mg.
    /// </summary>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Données structurées extraites du TBE.</returns>
    Task<InstatTbeData> CollecterAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Enrichit les cibles mensuelles existantes avec les données réelles du TBE.
    /// Remplace les estimations WB par les valeurs observées INSTAT/BCM/DGT.
    /// </summary>
    /// <param name="tbeData">Données extraites du TBE.</param>
    /// <param name="cibles">Cibles mensuelles à enrichir (modifiées en place).</param>
    /// <param name="annee">Année cible pour filtrer les mois du TBE.</param>
    void AppliquerAuxCibles(InstatTbeData tbeData, List<MonthlyCalibrationTarget> cibles, int annee);
}
