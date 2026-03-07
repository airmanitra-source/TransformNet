using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Simulation.Module.Config;
using Simulation.Module.Models;
using Simulation.Module.Services;
using UglyToad.PdfPig;

namespace Simulation.Infrastructure;

/// <summary>
/// Scraper du TBE INSTAT Madagascar.
/// Télécharge le PDF depuis instat.mg, extrait le texte page par page avec PdfPig,
/// puis parse les tableaux structurés pour en extraire les séries mensuelles.
///
/// Structure du TBE (ex: N°60, Octobre 2025, 42 pages) :
///   Table des matières page 3 → identifie les numéros de page par tableau.
///   Tableau 26 (p.30) : Nouveaux affiliés CNaPS
///   Tableau 29 (p.33) : TOFE — Recettes fiscales
///   Tableau 31 (p.35) : Indicateurs monétaires (M3, crédits, taux de change)
///   Tableau 32 (p.36) : Exportations FOB (millions MGA)
///   Tableau 33 (p.37) : Importations CIF (millions MGA)
///   Tableau 34 (p.38) : Tourisme (arrivées, devises DTS)
/// </summary>
public partial class InstatTbeScraperService : IInstatTbeScraperService
{
    private readonly HttpClient _httpClient;

    // Mois abrégés FR utilisés dans le TBE
    private static readonly Dictionary<string, int> MoisAbreviation = new(StringComparer.OrdinalIgnoreCase)
    {
        ["janv"] = 1, ["jan"] = 1,
        ["févr"] = 2, ["fevr"] = 2, ["fev"] = 2, ["f\u00e9vr"] = 2,
        ["mars"] = 3, ["mar"] = 3,
        ["avr"] = 4, ["avril"] = 4,
        ["mai"] = 5,
        ["juin"] = 6,
        ["juil"] = 7, ["juillet"] = 7,
        ["août"] = 8, ["aout"] = 8, ["ao\u00fbt"] = 8,
        ["sept"] = 9,
        ["oct"] = 10, ["octobre"] = 10,
        ["nov"] = 11, ["novembre"] = 11,
        ["déc"] = 12, ["dec"] = 12, ["d\u00e9c"] = 12,
    };

    public InstatTbeScraperService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(60);
    }

    /// <inheritdoc />
    public async Task<InstatTbeData> CollecterAsync(CancellationToken cancellationToken = default)
    {
        var result = new InstatTbeData { DateCollecte = DateTime.UtcNow };

        try
        {
            // 1. Trouver l'URL du dernier TBE
            var pdfUrl = await TrouverDernierTbePdfUrlAsync(cancellationToken);
            if (string.IsNullOrEmpty(pdfUrl))
            {
                result.Erreurs.Add("Impossible de trouver le lien PDF du dernier TBE sur instat.mg");
                return result;
            }
            result.UrlSource = pdfUrl;

            // Extraire le numéro TBE et la période depuis l'URL
            var matchUrl = Regex.Match(pdfUrl, @"TBE(\d+)_(\d{1,2})-(\d{4})", RegexOptions.IgnoreCase);
            if (matchUrl.Success)
            {
                result.NumeroTbe = int.Parse(matchUrl.Groups[1].Value);
                int moisRef = int.Parse(matchUrl.Groups[2].Value);
                int anneeRef = int.Parse(matchUrl.Groups[3].Value);
                string[] nomsMois = ["", "Janvier", "Février", "Mars", "Avril", "Mai", "Juin",
                    "Juillet", "Août", "Septembre", "Octobre", "Novembre", "Décembre"];
                result.PeriodeReference = $"{nomsMois[moisRef]} {anneeRef}";
            }

            // 2. Télécharger le PDF
            var pdfBytes = await _httpClient.GetByteArrayAsync(pdfUrl, cancellationToken);

            // 3. Parser le PDF
            ParseTbePdf(pdfBytes, result);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            result.Erreurs.Add($"Erreur HTTP : {ex.Message}");
        }
        catch (Exception ex)
        {
            result.Erreurs.Add($"Erreur parsing PDF : {ex.Message}");
        }

        return result;
    }

    /// <summary>
    /// Scrape la page d'accueil instat.mg pour trouver le lien vers la dernière page TBE,
    /// puis scrape cette page pour trouver le lien PDF direct.
    /// </summary>
    private async Task<string?> TrouverDernierTbePdfUrlAsync(CancellationToken ct)
    {
        // Étape 1 : trouver le lien de la page TBE sur la homepage
        var homepageHtml = await _httpClient.GetStringAsync("https://www.instat.mg", ct);

        var tbePageMatch = Regex.Match(homepageHtml,
            @"href=""(https?://www\.instat\.mg/p/tableau-de-bord-de-leconomie[^""]+)""",
            RegexOptions.IgnoreCase);

        if (!tbePageMatch.Success)
            return null;

        var tbePageUrl = tbePageMatch.Groups[1].Value;

        // Étape 2 : scraper la page TBE pour trouver le lien PDF
        var tbePageHtml = await _httpClient.GetStringAsync(tbePageUrl, ct);

        var pdfMatch = Regex.Match(tbePageHtml,
            @"href=""(https?://www\.instat\.mg/documents/upload/main/INSTAT_TBE[^""]+\.pdf)""",
            RegexOptions.IgnoreCase);

        if (pdfMatch.Success)
            return pdfMatch.Groups[1].Value;

        // Fallback : chercher tout lien PDF contenant TBE
        var pdfFallback = Regex.Match(tbePageHtml,
            @"(https?://[^""'\s]+TBE[^""'\s]+\.pdf)",
            RegexOptions.IgnoreCase);

        return pdfFallback.Success ? pdfFallback.Groups[1].Value : null;
    }

    /// <summary>
    /// Parse le contenu du TBE PDF et extrait les données mensuelles.
    /// </summary>
    private void ParseTbePdf(byte[] pdfBytes, InstatTbeData result)
    {
        using var doc = PdfDocument.Open(pdfBytes);

        // Index des tableaux — trouvé dynamiquement via la table des matières (page 3)
        // ou en scannant les titres de chaque page.
        var tableauPages = TrouverPagesTableaux(doc);

        // Dictionnaire temporaire mois → données
        var moisDict = new Dictionary<string, InstatTbeMensuel>();

        // Parser chaque tableau
        if (tableauPages.TryGetValue(26, out int pageCnaps))
            ParserTableau26Cnaps(doc, pageCnaps, moisDict, result);

        if (tableauPages.TryGetValue(29, out int pageRecettes))
            ParserTableau29Recettes(doc, pageRecettes, moisDict, result);

        if (tableauPages.TryGetValue(31, out int pageMonetaire))
            ParserTableau31Monetaire(doc, pageMonetaire, moisDict, result);

        if (tableauPages.TryGetValue(32, out int pageExports))
            ParserTableau32Exports(doc, pageExports, moisDict, result);

        if (tableauPages.TryGetValue(33, out int pageImports))
            ParserTableau33Imports(doc, pageImports, moisDict, result);

        // ── TABLEAU 34 : Tourisme ──
        // Le TOC pointe souvent vers un graphique, pas le tableau de données.
        // → Scanner TOUTES les pages de la 2ème moitié du PDF pour trouver les données.
        ParserTourismeScanComplet(doc, moisDict, result);

        // Trier par date décroissante
        result.Mois = [.. moisDict.Values.OrderByDescending(m => m.Annee * 100 + m.Mois)];
    }

    /// <summary>
    /// Scan la table des matières (typiquement page 3) pour trouver le numéro de page de chaque tableau.
    /// Format attendu : "Tableau 26 : ... 30" → tableau 26 à la page 30.
    /// </summary>
    private static Dictionary<int, int> TrouverPagesTableaux(PdfDocument doc)
    {
        var result = new Dictionary<int, int>();
        int[] tableauxRecherches = [26, 29, 31, 32, 33, 34];

        // Scanner les premières pages pour la table des matières
        for (int p = 1; p <= Math.Min(5, doc.NumberOfPages); p++)
        {
            var text = doc.GetPage(p).Text;
            foreach (int numTab in tableauxRecherches)
            {
                var match = Regex.Match(text, $@"Tableau\s+{numTab}\s*:.*?(\d+)", RegexOptions.IgnoreCase);
                if (match.Success && int.TryParse(match.Groups[1].Value, out int pageNum))
                {
                    result.TryAdd(numTab, pageNum);
                }
            }
        }

        // Fallback : scanner toutes les pages pour les titres de tableaux
        if (result.Count < tableauxRecherches.Length)
        {
            for (int p = 1; p <= doc.NumberOfPages; p++)
            {
                var text = doc.GetPage(p).Text;
                foreach (int numTab in tableauxRecherches)
                {
                    if (result.ContainsKey(numTab)) continue;
                    if (text.Contains($"Tableau {numTab}", StringComparison.OrdinalIgnoreCase) ||
                        text.Contains($"Tableau {numTab} :", StringComparison.OrdinalIgnoreCase))
                    {
                        result.TryAdd(numTab, p);
                    }
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Recherche une page PDF contenant TOUS les mots-clés spécifiés (insensible à la casse).
    /// Ignore les 5 premières pages (table des matières) pour éviter les faux positifs.
    /// </summary>
    private static int? TrouverPageParMotsCles(PdfDocument doc, string[] motsCles)
    {
        for (int p = 6; p <= doc.NumberOfPages; p++)
        {
            var text = doc.GetPage(p).Text;
            if (motsCles.All(mot => text.Contains(mot, StringComparison.OrdinalIgnoreCase)))
                return p;
        }
        return null;
    }

    /// <summary>
    /// Extrait les lignes de texte d'une page PDF en regroupant les mots par Y-coordinate.
    /// Utilise les coordonnées X des mots pour distinguer les séparateurs de milliers
    /// (gap faible → espace) des séparateurs de colonnes (gap large → tabulation).
    /// Ex: "4 634\t55\t868\t797\t582\tjanv 24" au lieu de "4 634 55 868 797 582 janv 24".
    /// </summary>
    private static List<string> ExtraireLines(PdfDocument doc, int pageNum)
    {
        if (pageNum < 1 || pageNum > doc.NumberOfPages) return [];

        var page = doc.GetPage(pageNum);
        var words = page.GetWords()
            .OrderBy(w => w.BoundingBox.Bottom)
            .ThenBy(w => w.BoundingBox.Left)
            .ToList();

        var lines = new List<string>();
        double lastY = -1;
        var currentLine = new List<(string text, double left, double right)>();

        foreach (var word in words)
        {
            double y = Math.Round(word.BoundingBox.Bottom, 0);
            if (Math.Abs(y - lastY) > 3 && currentLine.Count > 0)
            {
                lines.Add(BuildLineWithColumnTabs(currentLine));
                currentLine.Clear();
            }
            currentLine.Add((word.Text, word.BoundingBox.Left, word.BoundingBox.Right));
            lastY = y;
        }
        if (currentLine.Count > 0)
            lines.Add(BuildLineWithColumnTabs(currentLine));

        return lines;
    }

    /// <summary>
    /// Construit une ligne en insérant \t entre mots séparés par un gap horizontal
    /// &gt; seuil (colonnes distinctes), et un espace pour les gaps faibles (séparateur de milliers).
    /// Seuil typique : ~10 points PDF — un séparateur de milliers fait ~2-4pt, une colonne &gt;10pt.
    /// </summary>
    private static string BuildLineWithColumnTabs(List<(string text, double left, double right)> words)
    {
        const double GapThreshold = 10.0;
        var sb = new StringBuilder();
        for (int i = 0; i < words.Count; i++)
        {
            if (i > 0)
            {
                double gap = words[i].left - words[i - 1].right;
                sb.Append(gap > GapThreshold ? '\t' : ' ');
            }
            sb.Append(words[i].text);
        }
        return sb.ToString();
    }

    // ════════════════════════════════════════════════════════════
    //  TABLEAU 26 : Nouveaux affiliés CNaPS
    // ════════════════════════════════════════════════════════════

    private void ParserTableau26Cnaps(PdfDocument doc, int pageNum, Dictionary<string, InstatTbeMensuel> moisDict, InstatTbeData result)
    {
        try
        {
            var lines = ExtraireLines(doc, pageNum);

            // Les lignes de données CNAPS ont le format :
            // "TOTAL_VALUE ... janv 25" ou "4 634 55 868 797 582 55 843 1 185 249 janv 24"
            // Le TOTAL est le premier grand nombre, suivi des ventilations par secteur, puis la période
            foreach (var line in lines)
            {
                var periodeMatch = PeriodeMoisRegex().Match(line);
                if (!periodeMatch.Success) continue;

                var (annee, mois, label) = ParsePeriodeMois(periodeMatch.Value);
                if (annee == 0) continue;

                // Le premier nombre de la ligne est le TOTAL
                var numbers = ExtractNumbers(line[..periodeMatch.Index]);
                if (numbers.Count == 0) continue;

                // Le TOTAL est le plus grand nombre (typiquement le 1er)
                int total = (int)numbers[0];

                var key = $"{annee:D4}-{mois:D2}";
                var entry = GetOrCreateMois(moisDict, key, annee, mois, label);
                entry.NouveauxAffiliesCNaPS = total;
            }
        }
        catch (Exception ex)
        {
            result.Erreurs.Add($"Tableau 26 (CNaPS) : {ex.Message}");
        }
    }

    // ════════════════════════════════════════════════════════════
    //  TABLEAU 29 : Recettes fiscales (TOFE)
    // ════════════════════════════════════════════════════════════

    private void ParserTableau29Recettes(PdfDocument doc, int pageNum, Dictionary<string, InstatTbeMensuel> moisDict, InstatTbeData result)
    {
        try
        {
            var lines = ExtraireLines(doc, pageNum);

            // Trouver la ligne des en-têtes de colonnes avec les périodes
            // Format: "juil-24 août-24 sept-24 oct-24 ... juil-25"
            List<(int annee, int mois, string label)>? headerPeriodes = null;
            int headerLineIdx = -1;

            for (int i = 0; i < lines.Count; i++)
            {
                var periodes = ExtrairePeriodesEnTete(lines[i]);
                if (periodes.Count >= 5) // Au moins 5 colonnes de mois
                {
                    headerPeriodes = periodes;
                    headerLineIdx = i;
                    break;
                }
            }

            if (headerPeriodes == null)
            {
                result.Erreurs.Add("Tableau 29 : en-têtes de périodes non trouvés");
                return;
            }

            // Chercher la ligne "Recettes fiscales"
            foreach (var line in lines)
            {
                if (!line.Contains("Recettes fiscales", StringComparison.OrdinalIgnoreCase)) continue;
                if (line.Contains("non fiscales", StringComparison.OrdinalIgnoreCase)) continue;

                var numbers = ExtractDecimalNumbers(line);
                if (numbers.Count == 0) continue;

                // Mapper les valeurs aux périodes (les dernières valeurs correspondent aux colonnes mensuelles)
                AssignerValeursRecettes(numbers, headerPeriodes, moisDict);
                break;
            }
        }
        catch (Exception ex)
        {
            result.Erreurs.Add($"Tableau 29 (Recettes) : {ex.Message}");
        }
    }

    private static void AssignerValeursRecettes(List<double> numbers, List<(int annee, int mois, string label)> periodes,
        Dictionary<string, InstatTbeMensuel> moisDict)
    {
        // Le Tableau 29 a les cumuls annuels en premières colonnes, puis les opérations mensuelles
        // Les valeurs sont en milliards MGA (cumul depuis janvier)
        // On prend les N dernières valeurs pour les N périodes mensuelles
        int colCount = periodes.Count;
        if (numbers.Count < colCount) return;

        // Les dernières valeurs correspondent aux périodes mensuelles les plus récentes
        int offset = numbers.Count - colCount;
        for (int i = 0; i < colCount; i++)
        {
            var (annee, mois, label) = periodes[i];
            var key = $"{annee:D4}-{mois:D2}";
            var entry = GetOrCreateMois(moisDict, key, annee, mois, label);
            entry.RecettesFiscalesCumulMds = numbers[offset + i];
        }
    }

    // ════════════════════════════════════════════════════════════
    //  TABLEAU 31 : Indicateurs monétaires (M3, crédits, taux de change)
    // ════════════════════════════════════════════════════════════

    private void ParserTableau31Monetaire(PdfDocument doc, int pageNum, Dictionary<string, InstatTbeMensuel> moisDict, InstatTbeData result)
    {
        try
        {
            var lines = ExtraireLines(doc, pageNum);

            // Trouver la ligne d'en-têtes de périodes (format "févr-23 mars-23 ...")
            List<(int annee, int mois, string label)>? headerPeriodes = null;
            for (int i = 0; i < lines.Count; i++)
            {
                var periodes = ExtrairePeriodesEnTete(lines[i]);
                if (periodes.Count >= 10)
                {
                    headerPeriodes = periodes;
                    break;
                }
            }

            if (headerPeriodes == null)
            {
                result.Erreurs.Add("Tableau 31 : en-têtes de périodes non trouvés");
                return;
            }

            int colCount = headerPeriodes.Count;

            foreach (var line in lines)
            {
                if (line.Contains("Masse mon", StringComparison.OrdinalIgnoreCase) &&
                    line.Contains("M3", StringComparison.OrdinalIgnoreCase))
                {
                    var numbers = ExtractIntegerNumbers(line);
                    AssignerM3(numbers, headerPeriodes, moisDict);
                }
                else if (line.Contains("court terme", StringComparison.OrdinalIgnoreCase) &&
                         line.Contains("CREDIT", StringComparison.OrdinalIgnoreCase))
                {
                    var numbers = ExtractIntegerNumbers(line);
                    AssignerCreditCT(numbers, headerPeriodes, moisDict);
                }
                else if (line.Contains("moyen et long terme", StringComparison.OrdinalIgnoreCase))
                {
                    var numbers = ExtractIntegerNumbers(line);
                    AssignerCreditMLT(numbers, headerPeriodes, moisDict);
                }
                else if (line.Contains("Ariary/USD", StringComparison.OrdinalIgnoreCase))
                {
                    var numbers = ExtractDecimalNumbers(line);
                    AssignerTauxChange(numbers, headerPeriodes, moisDict);
                }
            }
        }
        catch (Exception ex)
        {
            result.Erreurs.Add($"Tableau 31 (Monétaire) : {ex.Message}");
        }
    }

    private static void AssignerM3(List<double> numbers, List<(int annee, int mois, string label)> periodes,
        Dictionary<string, InstatTbeMensuel> moisDict)
    {
        int colCount = periodes.Count;
        if (numbers.Count < colCount) return;
        int offset = numbers.Count - colCount;
        for (int i = 0; i < colCount; i++)
        {
            var (annee, mois, label) = periodes[i];
            var key = $"{annee:D4}-{mois:D2}";
            var entry = GetOrCreateMois(moisDict, key, annee, mois, label);
            entry.M3Mds = numbers[offset + i];
        }
    }

    private static void AssignerCreditCT(List<double> numbers, List<(int annee, int mois, string label)> periodes,
        Dictionary<string, InstatTbeMensuel> moisDict)
    {
        int colCount = periodes.Count;
        if (numbers.Count < colCount) return;
        int offset = numbers.Count - colCount;
        for (int i = 0; i < colCount; i++)
        {
            var (annee, mois, label) = periodes[i];
            var key = $"{annee:D4}-{mois:D2}";
            var entry = GetOrCreateMois(moisDict, key, annee, mois, label);
            entry.CreditEconomieCourtTermeMds = numbers[offset + i];
        }
    }

    private static void AssignerCreditMLT(List<double> numbers, List<(int annee, int mois, string label)> periodes,
        Dictionary<string, InstatTbeMensuel> moisDict)
    {
        int colCount = periodes.Count;
        if (numbers.Count < colCount) return;
        int offset = numbers.Count - colCount;
        for (int i = 0; i < colCount; i++)
        {
            var (annee, mois, label) = periodes[i];
            var key = $"{annee:D4}-{mois:D2}";
            var entry = GetOrCreateMois(moisDict, key, annee, mois, label);
            entry.CreditEconomieMoyenLongTermeMds = numbers[offset + i];
        }
    }

    private static void AssignerTauxChange(List<double> numbers, List<(int annee, int mois, string label)> periodes,
        Dictionary<string, InstatTbeMensuel> moisDict)
    {
        int colCount = periodes.Count;
        if (numbers.Count < colCount) return;
        int offset = numbers.Count - colCount;
        for (int i = 0; i < colCount; i++)
        {
            var (annee, mois, label) = periodes[i];
            var key = $"{annee:D4}-{mois:D2}";
            var entry = GetOrCreateMois(moisDict, key, annee, mois, label);
            entry.TauxChangeMgaUsd = numbers[offset + i];
        }
    }

    // ════════════════════════════════════════════════════════════
    //  TABLEAU 32 : Exportations FOB
    // ════════════════════════════════════════════════════════════

    private void ParserTableau32Exports(PdfDocument doc, int pageNum, Dictionary<string, InstatTbeMensuel> moisDict, InstatTbeData result)
    {
        try
        {
            var lines = ExtraireLines(doc, pageNum);

            // Les lignes de données exports ont un format similaire au CNAPS :
            // "TOTAL_VALUE ... janv 25" avec la période à la fin
            foreach (var line in lines)
            {
                var periodeMatch = PeriodeMoisRegex().Match(line);
                if (!periodeMatch.Success) continue;

                // Ignorer les lignes de cumul
                if (line.Contains("Cumul", StringComparison.OrdinalIgnoreCase)) continue;
                if (line.Contains("Variation", StringComparison.OrdinalIgnoreCase)) continue;

                var (annee, mois, label) = ParsePeriodeMois(periodeMatch.Value);
                if (annee == 0) continue;

                // Le premier nombre est "Total en valeur" des exportations
                var numbers = ExtractNumbers(line[..periodeMatch.Index]);
                if (numbers.Count == 0) continue;

                var key = $"{annee:D4}-{mois:D2}";
                var entry = GetOrCreateMois(moisDict, key, annee, mois, label);
                entry.ExportsFobMillions = numbers[0];
            }
        }
        catch (Exception ex)
        {
            result.Erreurs.Add($"Tableau 32 (Exports) : {ex.Message}");
        }
    }

    // ════════════════════════════════════════════════════════════
    //  TABLEAU 33 : Importations CIF
    // ════════════════════════════════════════════════════════════

    private void ParserTableau33Imports(PdfDocument doc, int pageNum, Dictionary<string, InstatTbeMensuel> moisDict, InstatTbeData result)
    {
        try
        {
            var lines = ExtraireLines(doc, pageNum);

            foreach (var line in lines)
            {
                var periodeMatch = PeriodeMoisRegex().Match(line);
                if (!periodeMatch.Success) continue;
                if (line.Contains("Cumul", StringComparison.OrdinalIgnoreCase)) continue;
                if (line.Contains("Variation", StringComparison.OrdinalIgnoreCase)) continue;

                var (annee, mois, label) = ParsePeriodeMois(periodeMatch.Value);
                if (annee == 0) continue;

                var numbers = ExtractNumbers(line[..periodeMatch.Index]);
                if (numbers.Count == 0) continue;

                var key = $"{annee:D4}-{mois:D2}";
                var entry = GetOrCreateMois(moisDict, key, annee, mois, label);
                entry.ImportsCifMillions = numbers[0];
            }
        }
        catch (Exception ex)
        {
            result.Erreurs.Add($"Tableau 33 (Imports) : {ex.Message}");
        }
    }

    // ════════════════════════════════════════════════════════════
    //  TABLEAU 34 : Tourisme (arrivées + devises DTS)
    //
    //  Le TOC du TBE pointe souvent vers une page de GRAPHIQUE tourisme,
    //  pas vers le tableau de données chiffrées. Solution : scanner
    //  TOUTES les pages du PDF et extraire les lignes qui matchent
    //  le pattern "période + entier + décimal".
    // ════════════════════════════════════════════════════════════

    /// <summary>
    /// Scanne toutes les pages du PDF pour extraire les données du Tableau 34 (tourisme).
    ///
    /// Caractéristique unique du Tableau 34 : la PÉRIODE est au DÉBUT de chaque ligne
    /// (contrairement aux tableaux 26/32/33 où elle est à la FIN).
    /// Format de chaque ligne de données :
    ///   "oct 23\t29 944\t51,19"  →  période[début] + arrivées(entier) + devises(décimal virgule)
    ///
    /// Validation : les deux valeurs doivent être présentes et dans les plages Madagascar :
    ///   arrivées  : 5 000 – 200 000 (visiteurs/mois)
    ///   devises   : 5 – 200 (millions DTS/mois)
    /// </summary>
    private void ParserTourismeScanComplet(PdfDocument doc, Dictionary<string, InstatTbeMensuel> moisDict, InstatTbeData result)
    {
        try
        {
            int totalExtracted = 0;
            int startPage = Math.Max(1, doc.NumberOfPages / 2);

            for (int p = startPage; p <= doc.NumberOfPages && totalExtracted < 5; p++)
            {
                var lines = ExtraireLines(doc, p);

                foreach (var line in lines)
                {
                    var normalizedLine = NormalizePdfText(line);

                    // ── 1. La période DOIT être au DÉBUT de la ligne ────────────
                    // (distingue Tableau 34 des autres tableaux où elle est à la fin)
                    var periodeMatch = PeriodeMoisDebutRegex().Match(normalizedLine);
                    if (!periodeMatch.Success) continue;

                    // Ignorer en-têtes et lignes de synthèse
                    if (normalizedLine.Contains("Cumul",      StringComparison.OrdinalIgnoreCase)) continue;
                    if (normalizedLine.Contains("Variation",  StringComparison.OrdinalIgnoreCase)) continue;
                    if (normalizedLine.Contains("Tableau",    StringComparison.OrdinalIgnoreCase)) continue;
                    if (normalizedLine.Contains("Période",    StringComparison.OrdinalIgnoreCase)) continue;
                    if (normalizedLine.Contains("Indicateur", StringComparison.OrdinalIgnoreCase)) continue;

                    var (annee, mois, label) = ParsePeriodeMois(periodeMatch.Value);
                    if (annee == 0) continue;

                    // ── 2. Partie données = tout après la période ───────────────
                    var dataPart = normalizedLine[periodeMatch.Length..].Trim();

                    // "nd" dans les données = mois non disponible.
                    // Ne PAS arrêter : le tableau est trié du plus récent au plus ancien,
                    // donc des mois plus anciens valides se trouvent plus bas.
                    if (dataPart.Contains("nd", StringComparison.OrdinalIgnoreCase))
                        continue;

                    // Normaliser : remplacer \t par espace pour éviter les captures cross-colonne
                    var data = dataPart.Replace('\t', ' ');

                    // ── 3. Extraire devises DTS : XX,XX ou XX.XX (décimal virgule) ──
                    // Ex: "51,19" → 51.19,  "62,84" → 62.84
                    // Plage réaliste Madagascar : 5–200 millions DTS/mois
                    double? devises = null;
                    var mDts = Regex.Match(data, @"(?<!\d)(\d{1,3})[,.](\d{2})(?!\d)");
                    if (mDts.Success)
                    {
                        string s = mDts.Groups[1].Value + "." + mDts.Groups[2].Value;
                        if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out double d)
                            && d >= 5 && d <= 200)
                            devises = d;
                    }

                    // ── 4. Extraire arrivées : entier avec séparateur espace ────────
                    // Ex: "29 944" → 29944,  "15 582" → 15582
                    // Plage réaliste Madagascar : 5 000–200 000 visiteurs/mois
                    int? arrivees = null;
                    var mArr = Regex.Match(data, @"(?<!\d)(\d{2,3})[ ](\d{3})(?!\d)");
                    if (mArr.Success)
                    {
                        string s = mArr.Groups[1].Value + mArr.Groups[2].Value;
                        if (int.TryParse(s, out int a) && a >= 5_000 && a <= 200_000)
                            arrivees = a;
                    }
                    // Fallback : entier sans séparateur (ex: "29944")
                    if (!arrivees.HasValue)
                    {
                        var mArrNoSep = Regex.Match(data, @"(?<!\d)(\d{5,6})(?!\d)");
                        if (mArrNoSep.Success
                            && int.TryParse(mArrNoSep.Groups[1].Value, out int a2)
                            && a2 >= 5_000 && a2 <= 200_000)
                            arrivees = a2;
                    }

                    // ── 5. Valider : les deux valeurs doivent être présentes ────────
                    // (évite les faux positifs sur d'autres tableaux)
                    if (!devises.HasValue || !arrivees.HasValue) continue;

                    // ── 6. Stocker ───────────────────────────────────────────────
                    var key = $"{annee:D4}-{mois:D2}";
                    var entry = GetOrCreateMois(moisDict, key, annee, mois, label);

                    if (!entry.ArriveesTouristes.HasValue)
                        entry.ArriveesTouristes = arrivees.Value;
                    if (!entry.DevisesTourismeMillionsDTS.HasValue)
                        entry.DevisesTourismeMillionsDTS = devises.Value;

                    totalExtracted++;
                }
            }

            if (totalExtracted == 0)
                result.Erreurs.Add("Tableau 34 (Tourisme) : aucune donnée trouvée. " +
                    "Vérifier que le tableau de données (pas le graphique) est présent dans le PDF.");
        }
        catch (Exception ex)
        {
            result.Erreurs.Add($"Tableau 34 (Tourisme) : {ex.Message}");
        }
    }

    // ════════════════════════════════════════════════════════════
    //  APPLIQUER AUX CIBLES
    // ════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public void AppliquerAuxCibles(InstatTbeData tbeData, List<MonthlyCalibrationTarget> cibles, int annee)
    {
        if (!tbeData.EstExploitable) return;

        var tbeMoisAnnee = tbeData.Mois.Where(m => m.Annee == annee).ToList();
        if (tbeMoisAnnee.Count == 0) return;

        // Calculer les cumuls exports/imports pour convertir les données mensuelles en cumulées
        double cumulExports = 0;
        double cumulImports = 0;

        string[] nomsMois = ["", "Janvier", "Février", "Mars", "Avril", "Mai", "Juin",
            "Juillet", "Août", "Septembre", "Octobre", "Novembre", "Décembre"];

        foreach (var cible in cibles.OrderBy(c => c.Mois))
        {
            int moisCalendaire = cible.Mois;
            if (moisCalendaire < 1 || moisCalendaire > 12) continue;

            var tbeMois = tbeMoisAnnee.FirstOrDefault(m => m.Mois == moisCalendaire);
            if (tbeMois == null) continue;

            // M3 en Mds → MGA (×1e9)
            if (tbeMois.M3Mds.HasValue)
                cible.M3Cible = tbeMois.M3Mds.Value * 1_000_000_000;

            // Recettes fiscales cumulées déjà en Mds → MGA (×1e9)
            if (tbeMois.RecettesFiscalesCumulMds.HasValue)
                cible.RecettesFiscalesCumuleesCible = tbeMois.RecettesFiscalesCumulMds.Value * 1_000_000_000;

            // Exports mensuels en millions → cumulés en MGA (×1e6)
            if (tbeMois.ExportsFobMillions.HasValue)
            {
                cumulExports += tbeMois.ExportsFobMillions.Value * 1_000_000;
                cible.ExportationsFOBCumuleesCible = cumulExports;
            }

            // Imports mensuels en millions → cumulés en MGA (×1e6)
            if (tbeMois.ImportsCifMillions.HasValue)
            {
                cumulImports += tbeMois.ImportsCifMillions.Value * 1_000_000;
                cible.ImportationsCIFCumuleesCible = cumulImports;
            }

            // Tourisme : convertir millions DTS → USD → MGA
            // Chaîne : millions_DTS × 1.33 (DTS→USD) × 1e6 × MGA/USD = MGA
            if (tbeMois.DevisesTourismeMillionsUSD.HasValue)
            {
                double tauxMgaParUsd = tbeMois.TauxChangeMgaUsd ?? 4_800;
                cible.DevisesTourismeCible = tbeMois.DevisesTourismeMillionsUSD.Value * 1_000_000 * tauxMgaParUsd;
            }

            // CNAPS
            if (tbeMois.NouveauxAffiliesCNaPS.HasValue)
                cible.NouveauxAffiliesCNaPSCible = tbeMois.NouveauxAffiliesCNaPS.Value;

            // Mettre à jour le label
            cible.Label = $"{nomsMois[moisCalendaire]} {annee} (TBE N°{tbeData.NumeroTbe})";
        }
    }

    // ════════════════════════════════════════════════════════════
    //  UTILITAIRES DE PARSING
    // ════════════════════════════════════════════════════════════

    /// <summary>
    /// Parse une période de type "janv 25", "oct 23", "févr-24" en (année, mois, label).
    /// </summary>
    private static (int annee, int mois, string label) ParsePeriodeMois(string text)
    {
        text = NormalizePdfText(text).Trim();
        var match = Regex.Match(text, @"([a-zéèêûô]+)[\s\-]+(\d{2,4})", RegexOptions.IgnoreCase);
        if (!match.Success) return (0, 0, text);

        string moisStr = match.Groups[1].Value.ToLowerInvariant();
        string anneeStr = match.Groups[2].Value;

        if (!MoisAbreviation.TryGetValue(moisStr, out int mois))
            return (0, 0, text);

        int annee = int.Parse(anneeStr);
        if (annee < 100) annee += 2000; // "25" → 2025

        return (annee, mois, text);
    }

    /// <summary>
    /// Extrait les en-têtes de périodes dans une ligne (format "févr-23 mars-23 avr-23 ...").
    /// </summary>
    private static List<(int annee, int mois, string label)> ExtrairePeriodesEnTete(string line)
    {
        line = NormalizePdfText(line);
        var result = new List<(int, int, string)>();
        var matches = Regex.Matches(line, @"([a-zéèêûô]+)[\s\-]+(\d{2,4})", RegexOptions.IgnoreCase);

        foreach (Match match in matches)
        {
            string moisStr = match.Groups[1].Value.ToLowerInvariant();
            string anneeStr = match.Groups[2].Value;

            if (!MoisAbreviation.TryGetValue(moisStr, out int mois)) continue;

            int annee = int.Parse(anneeStr);
            if (annee < 100) annee += 2000;

            result.Add((annee, mois, match.Value));
        }

        return result;
    }

    /// <summary>
    /// Extrait tous les nombres entiers d'une chaîne tab-délimitée.
    /// Les tabulations séparent les colonnes du tableau ; les espaces à l'intérieur
    /// d'une cellule sont des séparateurs de milliers (ex: "1 892 301" → 1892301).
    /// </summary>
    private static List<double> ExtractNumbers(string text)
    {
        var result = new List<double>();
        foreach (var cell in text.Split('\t'))
        {
            string cleaned = cell.Trim().Replace(" ", "");
            if (cleaned.Length > 0 && cleaned.All(char.IsDigit) &&
                double.TryParse(cleaned, NumberStyles.Integer, CultureInfo.InvariantCulture, out double val))
                result.Add(val);
        }
        return result;
    }

    /// <summary>
    /// Extrait les nombres décimaux (virgule ou point) d'une chaîne tab-délimitée.
    /// Ex: cellule "5 328,3" → 5328.3
    /// </summary>
    private static List<double> ExtractDecimalNumbers(string text)
    {
        var result = new List<double>();
        foreach (var cell in text.Split('\t'))
        {
            string cleaned = cell.Trim().Replace(" ", "").Replace(",", ".");
            if (cleaned.Length == 0) continue;
            // Vérifier que c'est bien un nombre (chiffres + éventuellement un point décimal)
            if (cleaned.All(c => char.IsDigit(c) || c == '.') &&
                double.TryParse(cleaned, NumberStyles.Float, CultureInfo.InvariantCulture, out double val))
                result.Add(val);
        }
        return result;
    }

    /// <summary>
    /// Extrait les nombres entiers uniquement (pas de décimaux) d'une chaîne tab-délimitée.
    /// </summary>
    private static List<double> ExtractIntegerNumbers(string text)
    {
        return ExtractNumbers(text);
    }

    private static InstatTbeMensuel GetOrCreateMois(Dictionary<string, InstatTbeMensuel> dict,
        string key, int annee, int mois, string label)
    {
        if (!dict.TryGetValue(key, out var entry))
        {
            entry = new InstatTbeMensuel { Annee = annee, Mois = mois, Label = label };
            dict[key] = entry;
        }
        return entry;
    }

    /// <summary>
    /// Normalise les chaînes extraites du PDF quand PdfPig produit du texte mojibake
    /// (ex: "f├®vr" au lieu de "févr").
    /// </summary>
    private static string NormalizePdfText(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;

        return text
            .Replace("f├®vr", "févr", StringComparison.OrdinalIgnoreCase)
            .Replace("fe├üvr", "févr", StringComparison.OrdinalIgnoreCase)
            .Replace("ao├╗t", "août", StringComparison.OrdinalIgnoreCase)
            .Replace("d├®c", "déc", StringComparison.OrdinalIgnoreCase)
            .Replace("P├®riode", "Période", StringComparison.OrdinalIgnoreCase)
            .Replace("non-r├®sidents", "non-résidents", StringComparison.OrdinalIgnoreCase)
            .Replace("Arriv├®es", "Arrivées", StringComparison.OrdinalIgnoreCase)
            .Replace("fronti├¿res", "frontières", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Regex pour détecter une période mois en fin de ligne (ex: "janv 25", "sept 24").</summary>
    [GeneratedRegex(@"(janv|jan|f[eé]vr|fev|mars|mar|avr|avril|mai|juin|juil|juillet|ao[uû]t|aout|sept|oct|octobre|nov|novembre|d[eé]c|dec)\s+\d{2,4}", RegexOptions.IgnoreCase)]
    private static partial Regex PeriodeMoisRegex();

    /// <summary>Regex pour détecter une période mois en début de texte.</summary>
    [GeneratedRegex(@"^(janv|jan|f[eé]vr|fev|mars|mar|avr|avril|mai|juin|juil|juillet|ao[uû]t|aout|sept|oct|octobre|nov|novembre|d[eé]c|dec)\s+\d{2,4}", RegexOptions.IgnoreCase)]
    private static partial Regex PeriodeMoisDebutRegex();
}
