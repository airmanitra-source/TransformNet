using Simulation.Module.Config;

namespace Simulation.Infrastructure;

/// <summary>
/// Contrat pour le chargement de données macroéconomiques historiques
/// depuis des fichiers CSV et leur conversion en cibles de recalibration mensuelles.
///
/// Le format CSV attendu :
///   Mois;Label;M3;RecettesFiscales;ImportsCIF;ExportsFOB;DevisesTourisme;AffiliésCNaPS
///
/// Convention :
///   - Valeurs monétaires en milliards MGA (sauf affiliés CNaPS en nombre absolu)
///   - Cellules vides = pas de cible pour ce mois (null)
///   - Séparateur : point-virgule (;)
///   - Première ligne = en-tête (ignorée)
///
/// Sources recommandées pour Madagascar :
///   - M3 : BCM bulletin mensuel des statistiques monétaires
///   - Recettes fiscales : DGI / TOFE mensuel du Ministère des Finances
///   - Imports/Exports : INSTAT Tableaux 32-33 du commerce extérieur
///   - Devises tourisme : BCM balance des paiements / Ministère du Tourisme
///   - Affiliés CNaPS : CNaPS rapport mensuel d'affiliation
/// </summary>
public interface IHistoricalDataLoader
{
    /// <summary>
    /// Charge des données macro mensuelles depuis un fichier CSV
    /// et les convertit en cibles de recalibration.
    /// </summary>
    /// <param name="cheminCsv">Chemin vers le fichier CSV.</param>
    /// <param name="facteurEchelle">Facteur d'échelle simulation / réalité.</param>
    /// <returns>Liste de cibles mensuelles ordonnées par mois.</returns>
    List<MonthlyCalibrationTarget> ChargerDepuisCsv(string cheminCsv, double facteurEchelle);

    /// <summary>
    /// Charge des données depuis un contenu CSV en mémoire (pour Blazor sans accès fichier).
    /// </summary>
    /// <param name="contenuCsv">Contenu brut du fichier CSV.</param>
    /// <param name="facteurEchelle">Facteur d'échelle simulation / réalité.</param>
    /// <returns>Liste de cibles mensuelles ordonnées par mois.</returns>
    List<MonthlyCalibrationTarget> ChargerDepuisContenu(string contenuCsv, double facteurEchelle);
}

/// <summary>
/// Implémentation du chargeur de données historiques macro CSV → MonthlyCalibrationTarget.
/// </summary>
public class HistoricalDataLoader : IHistoricalDataLoader
{
    public List<MonthlyCalibrationTarget> ChargerDepuisCsv(string cheminCsv, double facteurEchelle)
    {
        if (!File.Exists(cheminCsv))
            return [];

        string contenu = File.ReadAllText(cheminCsv);
        return ChargerDepuisContenu(contenu, facteurEchelle);
    }

    public List<MonthlyCalibrationTarget> ChargerDepuisContenu(string contenuCsv, double facteurEchelle)
    {
        var cibles = new List<MonthlyCalibrationTarget>();
        var lignes = contenuCsv
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .Skip(1); // skip header

        foreach (var ligne in lignes)
        {
            var cols = ligne.Split(';');
            if (cols.Length < 2) continue;

            if (!int.TryParse(cols[0].Trim(), out int mois))
                continue;

            var cible = new MonthlyCalibrationTarget
            {
                Mois = mois,
                Label = cols.Length > 1 ? cols[1].Trim() : $"Mois {mois}",
            };

            if (cols.Length > 2)
                cible.M3Cible = ParseMilliardsMGA(cols[2]);
            if (cols.Length > 3)
                cible.RecettesFiscalesCumuleesCible = ParseMilliardsMGA(cols[3]);
            if (cols.Length > 4)
                cible.ImportationsCIFCumuleesCible = ParseMilliardsMGA(cols[4]);
            if (cols.Length > 5)
                cible.ExportationsFOBCumuleesCible = ParseMilliardsMGA(cols[5]);
            if (cols.Length > 6)
                cible.DevisesTourismeCible = ParseMilliardsMGA(cols[6]);
            if (cols.Length > 7)
                cible.NouveauxAffiliesCNaPSCible = ParseNullableInt(cols[7]);

            cibles.Add(cible);
        }

        return cibles.OrderBy(c => c.Mois).ToList();
    }

    /// <summary>
    /// Parse une valeur en milliards MGA. Retourne null si vide ou invalide.
    /// Ex: "29500" → 29 500 000 000 000 MGA (29 500 Mds).
    /// </summary>
    private static double? ParseMilliardsMGA(string valeur)
    {
        if (string.IsNullOrWhiteSpace(valeur))
            return null;

        if (!double.TryParse(valeur.Trim(), System.Globalization.CultureInfo.InvariantCulture, out var v))
            return null;

        return v * 1_000_000_000; // milliards → unités
    }

    private static int? ParseNullableInt(string valeur)
    {
        if (string.IsNullOrWhiteSpace(valeur))
            return null;

        return int.TryParse(valeur.Trim(), out var v) ? v : null;
    }
}
