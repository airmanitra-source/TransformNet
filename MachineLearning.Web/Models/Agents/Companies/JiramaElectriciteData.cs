namespace MachineLearning.Web.Models.Agents.Companies;

/// <summary>
/// Données mensuelles de production et consommation d'électricité JIRAMA.
/// Source : Tableau 21 — Évolution mensuelle des production et consommation d'électricité.
/// Source : JIRAMA / ORE / INSTAT — Données juil. 2023 à juin 2025.
/// </summary>
public record JiramaElectriciteData
{
    /// <summary>Année (ex: 2024)</summary>
    public int Annee { get; init; }

    /// <summary>Mois (1-12)</summary>
    public int Mois { get; init; }

    // --- Production (MWh) ---
    /// <summary>Production totale en MWh</summary>
    public double ProductionTotaleMWh { get; init; }

    /// <summary>Production hydraulique en MWh</summary>
    public double ProductionHydrauliqueMWh { get; init; }

    /// <summary>Production thermique en MWh</summary>
    public double ProductionThermiqueMWh { get; init; }

    // --- Prix ---
    /// <summary>Prix moyen de l'électricité en Ariary par KWh</summary>
    public double PrixMoyenArKWh { get; init; }

    // --- Consommation (MWh) ---
    /// <summary>Consommation totale en MWh</summary>
    public double ConsommationTotaleMWh { get; init; }

    /// <summary>Consommation des ménages en MWh</summary>
    public double ConsommationMenagesMWh { get; init; }

    /// <summary>Consommation industries et services en MWh</summary>
    public double ConsommationIndustrieMWh { get; init; }

    /// <summary>Consommation éclairage public en MWh</summary>
    public double ConsommationEclairagePublicMWh { get; init; }

    // --- Indicateurs dérivés ---
    /// <summary>Part hydraulique dans la production (%)</summary>
    public double PartHydraulique => ProductionTotaleMWh > 0
        ? ProductionHydrauliqueMWh / ProductionTotaleMWh
        : 0;

    /// <summary>Part thermique dans la production (%)</summary>
    public double PartThermique => ProductionTotaleMWh > 0
        ? ProductionThermiqueMWh / ProductionTotaleMWh
        : 0;

    /// <summary>Part ménages dans la consommation (%)</summary>
    public double PartConsommationMenages => ConsommationTotaleMWh > 0
        ? ConsommationMenagesMWh / ConsommationTotaleMWh
        : 0;

    /// <summary>Part industries dans la consommation (%)</summary>
    public double PartConsommationIndustrie => ConsommationTotaleMWh > 0
        ? ConsommationIndustrieMWh / ConsommationTotaleMWh
        : 0;

    /// <summary>Pertes de distribution = Production - Consommation (MWh)</summary>
    public double PertesMWh => ProductionTotaleMWh - ConsommationTotaleMWh;

    /// <summary>Taux de pertes (%)</summary>
    public double TauxPertes => ProductionTotaleMWh > 0
        ? PertesMWh / ProductionTotaleMWh
        : 0;

    /// <summary>
    /// Données historiques mensuelles complètes (Tableau 21 JIRAMA/ORE).
    /// Période : juillet 2023 → juin 2025.
    /// Unités : MWh (production & consommation), Ar/KWh (prix).
    /// </summary>
    public static readonly JiramaElectriciteData[] HistoriqueMensuel =
    [
        // --- Année fiscale juil. 2023 → juin 2024 ---
        new() { Annee = 2023, Mois =  7, ProductionTotaleMWh = 160_067, ProductionHydrauliqueMWh =  75_211, ProductionThermiqueMWh =  84_856, PrixMoyenArKWh = 610, ConsommationTotaleMWh = 112_193, ConsommationMenagesMWh = 52_923, ConsommationIndustrieMWh = 58_630, ConsommationEclairagePublicMWh =  640 },
        new() { Annee = 2023, Mois =  8, ProductionTotaleMWh = 166_586, ProductionHydrauliqueMWh =  77_186, ProductionThermiqueMWh =  89_399, PrixMoyenArKWh = 615, ConsommationTotaleMWh = 113_889, ConsommationMenagesMWh = 53_406, ConsommationIndustrieMWh = 59_917, ConsommationEclairagePublicMWh =  566 },
        new() { Annee = 2023, Mois =  9, ProductionTotaleMWh = 161_636, ProductionHydrauliqueMWh =  63_862, ProductionThermiqueMWh =  97_774, PrixMoyenArKWh = 621, ConsommationTotaleMWh = 113_949, ConsommationMenagesMWh = 55_185, ConsommationIndustrieMWh = 58_150, ConsommationEclairagePublicMWh =  615 },
        new() { Annee = 2023, Mois = 10, ProductionTotaleMWh = 163_661, ProductionHydrauliqueMWh =  58_071, ProductionThermiqueMWh = 105_591, PrixMoyenArKWh = 618, ConsommationTotaleMWh = 116_820, ConsommationMenagesMWh = 55_520, ConsommationIndustrieMWh = 60_581, ConsommationEclairagePublicMWh =  719 },
        new() { Annee = 2023, Mois = 11, ProductionTotaleMWh = 164_903, ProductionHydrauliqueMWh =  63_121, ProductionThermiqueMWh = 101_782, PrixMoyenArKWh = 616, ConsommationTotaleMWh = 116_519, ConsommationMenagesMWh = 53_946, ConsommationIndustrieMWh = 61_885, ConsommationEclairagePublicMWh =  688 },
        new() { Annee = 2023, Mois = 12, ProductionTotaleMWh = 169_213, ProductionHydrauliqueMWh =  73_257, ProductionThermiqueMWh =  95_956, PrixMoyenArKWh = 621, ConsommationTotaleMWh = 114_312, ConsommationMenagesMWh = 53_822, ConsommationIndustrieMWh = 59_874, ConsommationEclairagePublicMWh =  616 },
        new() { Annee = 2024, Mois =  1, ProductionTotaleMWh = 169_180, ProductionHydrauliqueMWh =  98_357, ProductionThermiqueMWh =  70_823, PrixMoyenArKWh = 617, ConsommationTotaleMWh = 120_612, ConsommationMenagesMWh = 57_469, ConsommationIndustrieMWh = 62_521, ConsommationEclairagePublicMWh =  622 },
        new() { Annee = 2024, Mois =  2, ProductionTotaleMWh = 159_292, ProductionHydrauliqueMWh =  98_641, ProductionThermiqueMWh =  60_650, PrixMoyenArKWh = 615, ConsommationTotaleMWh = 115_927, ConsommationMenagesMWh = 56_135, ConsommationIndustrieMWh = 59_104, ConsommationEclairagePublicMWh =  688 },
        new() { Annee = 2024, Mois =  3, ProductionTotaleMWh = 167_060, ProductionHydrauliqueMWh =  99_955, ProductionThermiqueMWh =  67_104, PrixMoyenArKWh = 623, ConsommationTotaleMWh = 116_859, ConsommationMenagesMWh = 54_675, ConsommationIndustrieMWh = 61_567, ConsommationEclairagePublicMWh =  618 },
        new() { Annee = 2024, Mois =  4, ProductionTotaleMWh = 165_989, ProductionHydrauliqueMWh =  96_925, ProductionThermiqueMWh =  69_064, PrixMoyenArKWh = 623, ConsommationTotaleMWh = 126_256, ConsommationMenagesMWh = 53_980, ConsommationIndustrieMWh = 71_679, ConsommationEclairagePublicMWh =  597 },
        new() { Annee = 2024, Mois =  5, ProductionTotaleMWh = 171_604, ProductionHydrauliqueMWh =  98_918, ProductionThermiqueMWh =  72_686, PrixMoyenArKWh = 630, ConsommationTotaleMWh = 128_886, ConsommationMenagesMWh = 53_784, ConsommationIndustrieMWh = 74_350, ConsommationEclairagePublicMWh =  752 },
        new() { Annee = 2024, Mois =  6, ProductionTotaleMWh = 172_971, ProductionHydrauliqueMWh =  90_479, ProductionThermiqueMWh =  82_492, PrixMoyenArKWh = 633, ConsommationTotaleMWh = 121_028, ConsommationMenagesMWh = 54_855, ConsommationIndustrieMWh = 65_540, ConsommationEclairagePublicMWh =  633 },

        // --- Année fiscale juil. 2024 → juin 2025 ---
        new() { Annee = 2024, Mois =  7, ProductionTotaleMWh = 173_156, ProductionHydrauliqueMWh =  86_651, ProductionThermiqueMWh =  86_505, PrixMoyenArKWh = 625, ConsommationTotaleMWh = 140_116, ConsommationMenagesMWh = 54_898, ConsommationIndustrieMWh = 84_619, ConsommationEclairagePublicMWh =  657 /*corrigé de 599+58*/ },
        new() { Annee = 2024, Mois =  8, ProductionTotaleMWh = 168_784, ProductionHydrauliqueMWh =  80_033, ProductionThermiqueMWh =  88_751, PrixMoyenArKWh = 587, ConsommationTotaleMWh = 103_926, ConsommationMenagesMWh = 54_912, ConsommationIndustrieMWh = 48_358, ConsommationEclairagePublicMWh =  657 },
        new() { Annee = 2024, Mois =  9, ProductionTotaleMWh = 161_458, ProductionHydrauliqueMWh =  64_386, ProductionThermiqueMWh =  97_072, PrixMoyenArKWh = 636, ConsommationTotaleMWh = 113_027, ConsommationMenagesMWh = 53_247, ConsommationIndustrieMWh = 58_985, ConsommationEclairagePublicMWh =  795 },
        new() { Annee = 2024, Mois = 10, ProductionTotaleMWh = 154_787, ProductionHydrauliqueMWh =  53_274, ProductionThermiqueMWh = 101_513, PrixMoyenArKWh = 638, ConsommationTotaleMWh = 112_827, ConsommationMenagesMWh = 53_446, ConsommationIndustrieMWh = 58_768, ConsommationEclairagePublicMWh =  613 },
        new() { Annee = 2024, Mois = 11, ProductionTotaleMWh = 154_958, ProductionHydrauliqueMWh =  40_339, ProductionThermiqueMWh = 114_619, PrixMoyenArKWh = 671, ConsommationTotaleMWh = 113_303, ConsommationMenagesMWh = 52_525, ConsommationIndustrieMWh = 60_164, ConsommationEclairagePublicMWh =  614 },
        new() { Annee = 2024, Mois = 12, ProductionTotaleMWh = 164_772, ProductionHydrauliqueMWh =  42_647, ProductionThermiqueMWh = 122_125, PrixMoyenArKWh = 666, ConsommationTotaleMWh = 116_380, ConsommationMenagesMWh = 52_978, ConsommationIndustrieMWh = 62_790, ConsommationEclairagePublicMWh =  613 },
        new() { Annee = 2025, Mois =  1, ProductionTotaleMWh = 157_234, ProductionHydrauliqueMWh =  60_421, ProductionThermiqueMWh =  96_813, PrixMoyenArKWh = 672, ConsommationTotaleMWh = 106_617, ConsommationMenagesMWh = 50_390, ConsommationIndustrieMWh = 55_784, ConsommationEclairagePublicMWh =  442 },
        new() { Annee = 2025, Mois =  2, ProductionTotaleMWh = 152_394, ProductionHydrauliqueMWh =  91_166, ProductionThermiqueMWh =  61_228, PrixMoyenArKWh = 651, ConsommationTotaleMWh = 113_918, ConsommationMenagesMWh = 56_013, ConsommationIndustrieMWh = 57_243, ConsommationEclairagePublicMWh =  663 },
        new() { Annee = 2025, Mois =  3, ProductionTotaleMWh = 173_939, ProductionHydrauliqueMWh = 107_540, ProductionThermiqueMWh =  66_399, PrixMoyenArKWh = 631, ConsommationTotaleMWh = 119_095, ConsommationMenagesMWh = 54_845, ConsommationIndustrieMWh = 63_036, ConsommationEclairagePublicMWh = 1_214 },
        new() { Annee = 2025, Mois =  4, ProductionTotaleMWh = 173_169, ProductionHydrauliqueMWh =  88_508, ProductionThermiqueMWh =  84_661, PrixMoyenArKWh = 647, ConsommationTotaleMWh = 122_224, ConsommationMenagesMWh = 55_163, ConsommationIndustrieMWh = 65_877, ConsommationEclairagePublicMWh = 1_184 },
        new() { Annee = 2025, Mois =  5, ProductionTotaleMWh = 170_979, ProductionHydrauliqueMWh =  87_547, ProductionThermiqueMWh =  83_432, PrixMoyenArKWh = 646, ConsommationTotaleMWh = 123_342, ConsommationMenagesMWh = 58_447, ConsommationIndustrieMWh = 64_149, ConsommationEclairagePublicMWh =  746 },
        new() { Annee = 2025, Mois =  6, ProductionTotaleMWh = 158_308, ProductionHydrauliqueMWh =  74_092, ProductionThermiqueMWh =  84_216, PrixMoyenArKWh = 652, ConsommationTotaleMWh = 116_141, ConsommationMenagesMWh = 57_427, ConsommationIndustrieMWh = 57_675, ConsommationEclairagePublicMWh = 1_040 },
    ];

    /// <summary>
    /// Cumul jan-juin 2025 (6 premiers mois 2025).
    /// Production : 986 023 MWh — Consommation : 701 336 MWh — Prix moyen : 653 Ar/KWh.
    /// </summary>
    public static JiramaElectriciteData CumulJanJuin2025 => new()
    {
        Annee = 2025, Mois = 0,
        ProductionTotaleMWh = 986_023,
        ProductionHydrauliqueMWh = 509_273,
        ProductionThermiqueMWh = 476_749,
        PrixMoyenArKWh = 653,
        ConsommationTotaleMWh = 701_336,
        ConsommationMenagesMWh = 332_284,
        ConsommationIndustrieMWh = 363_763,
        ConsommationEclairagePublicMWh = 5_290
    };

    /// <summary>
    /// Cumul jan-juin 2024 pour comparaison interannuelle.
    /// Production : 1 006 095 MWh — Consommation : 729 569 MWh — Prix moyen : 624 Ar/KWh.
    /// </summary>
    public static JiramaElectriciteData CumulJanJuin2024 => new()
    {
        Annee = 2024, Mois = 0,
        ProductionTotaleMWh = 1_006_095,
        ProductionHydrauliqueMWh = 583_275,
        ProductionThermiqueMWh = 422_820,
        PrixMoyenArKWh = 624,
        ConsommationTotaleMWh = 729_569,
        ConsommationMenagesMWh = 330_899,
        ConsommationIndustrieMWh = 394_762,
        ConsommationEclairagePublicMWh = 3_909
    };

    /// <summary>
    /// Variations interannuelles jan-juin 2025 vs jan-juin 2024.
    /// Source : dernière ligne du Tableau 21.
    /// </summary>
    public static class Variations2025vs2024
    {
        public const double ProductionTotale = -0.020;       // -2,0%
        public const double ProductionHydraulique = -0.127;  // -12,7%
        public const double ProductionThermique = 0.128;     //  12,8%
        public const double PrixMoyen = 0.047;               //   4,7%
        public const double ConsommationTotale = -0.030;     // -3,0%
        public const double ConsommationMenages = 0.004;     //   0,4%
        public const double ConsommationIndustrie = -0.070;  // -7,0%
        public const double ConsommationEclairagePublic = 0.353; // 35,3%
    }
}
