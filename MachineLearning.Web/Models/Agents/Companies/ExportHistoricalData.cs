namespace MachineLearning.Web.Models.Agents.Companies;

/// <summary>
/// Données historiques réelles des exportations FOB de Madagascar par catégorie.
/// Source : INSTAT — Tableau 32 : Évolution mensuelle des exportations (millions d'Ariary).
/// Période couverte : octobre 2023 – septembre 2025.
/// </summary>
public class ExportHistoricalData
{
    public int Annee { get; set; }
    public int Mois { get; set; }
    public string Periode => $"{NomMois(Mois)} {Annee:D2}";

    /// <summary>Total en valeur FOB (millions MGA)</summary>
    public double TotalFOB { get; set; }

    /// <summary>Biens alimentaires (millions MGA)</summary>
    public double BiensAlimentaires { get; set; }

    /// <summary>Vanille (millions MGA)</summary>
    public double Vanille { get; set; }

    /// <summary>Crevettes (millions MGA)</summary>
    public double Crevettes { get; set; }

    /// <summary>Café (millions MGA)</summary>
    public double Cafe { get; set; }

    /// <summary>Girofle (millions MGA)</summary>
    public double Girofle { get; set; }

    /// <summary>Produits miniers (millions MGA)</summary>
    public double ProduitsMiniers { get; set; }

    /// <summary>Zones Franches (millions MGA)</summary>
    public double ZonesFranches { get; set; }

    /// <summary>Balance commerciale FOB du mois</summary>
    public double BalanceCommerciale => TotalFOB;

    /// <summary>
    /// Retourne la valeur FOB par catégorie d'export.
    /// </summary>
    public double ParCategorie(ECategorieExport categorie) => categorie switch
    {
        ECategorieExport.BiensAlimentaires => BiensAlimentaires,
        ECategorieExport.Vanille => Vanille,
        ECategorieExport.Crevettes => Crevettes,
        ECategorieExport.Cafe => Cafe,
        ECategorieExport.Girofle => Girofle,
        ECategorieExport.ProduitsMiniers => ProduitsMiniers,
        ECategorieExport.ZonesFranches => ZonesFranches,
        _ => 0
    };

    /// <summary>
    /// Vecteur de features pour l'entraînement ML : [7 catégories normalisées par le total].
    /// </summary>
    public double[] VecteurParts()
    {
        double t = TotalFOB > 0 ? TotalFOB : 1;
        return
        [
            BiensAlimentaires / t,
            Vanille / t,
            Crevettes / t,
            Cafe / t,
            Girofle / t,
            ProduitsMiniers / t,
            ZonesFranches / t
        ];
    }

    /// <summary>
    /// Données INSTAT — Tableau 32 (oct 2023 – sept 2025).
    /// Valeurs en millions d'Ariary.
    /// </summary>
    public static List<ExportHistoricalData> DonneesINSTAT() =>
    [
        // ──── 2023 (oct–déc) ────
        new() { Annee = 2023, Mois = 10, TotalFOB = 963_637,   BiensAlimentaires = 121_431, Vanille = 1,       Crevettes = 5_861,  Cafe = 2,      Girofle = 5_145,   ProduitsMiniers = 478_196, ZonesFranches = 260_351 },
        new() { Annee = 2023, Mois = 11, TotalFOB = 861_897,   BiensAlimentaires = 281_767, Vanille = 1,       Crevettes = 24_925, Cafe = 81,     Girofle = 109_597, ProduitsMiniers = 152_625, ZonesFranches = 289_617 },
        new() { Annee = 2023, Mois = 12, TotalFOB = 1_429_086, BiensAlimentaires = 399_829, Vanille = 30_909,  Crevettes = 29_595, Cafe = 1,      Girofle = 232_898, ProduitsMiniers = 536_172, ZonesFranches = 325_850 },

        // ──── 2024 (janv–déc) ────
        new() { Annee = 2024, Mois = 1,  TotalFOB = 982_619,   BiensAlimentaires = 306_282, Vanille = 81_390,  Crevettes = 19_578, Cafe = 0,      Girofle = 165_593, ProduitsMiniers = 298_468, ZonesFranches = 257_208 },
        new() { Annee = 2024, Mois = 2,  TotalFOB = 991_723,   BiensAlimentaires = 343_457, Vanille = 99_483,  Crevettes = 25_864, Cafe = 0,      Girofle = 166_827, ProduitsMiniers = 209_672, ZonesFranches = 294_503 },
        new() { Annee = 2024, Mois = 3,  TotalFOB = 909_158,   BiensAlimentaires = 248_008, Vanille = 71_228,  Crevettes = 17_344, Cafe = 1,      Girofle = 109_073, ProduitsMiniers = 286_032, ZonesFranches = 267_846 },
        new() { Annee = 2024, Mois = 4,  TotalFOB = 919_445,   BiensAlimentaires = 243_174, Vanille = 91_958,  Crevettes = 28_566, Cafe = 0,      Girofle = 60_078,  ProduitsMiniers = 340_189, ZonesFranches = 267_381 },
        new() { Annee = 2024, Mois = 5,  TotalFOB = 1_003_376, BiensAlimentaires = 279_935, Vanille = 121_697, Crevettes = 50_256, Cafe = 5_445,  Girofle = 38_407,  ProduitsMiniers = 307_829, ZonesFranches = 293_831 },
        new() { Annee = 2024, Mois = 6,  TotalFOB = 984_704,   BiensAlimentaires = 253_391, Vanille = 127_380, Crevettes = 46_562, Cafe = 4_850,  Girofle = 19_077,  ProduitsMiniers = 295_037, ZonesFranches = 267_389 },
        new() { Annee = 2024, Mois = 7,  TotalFOB = 1_243_730, BiensAlimentaires = 498_586, Vanille = 343_364, Crevettes = 30_301, Cafe = 8_202,  Girofle = 16_322,  ProduitsMiniers = 242_042, ZonesFranches = 363_346 },
        new() { Annee = 2024, Mois = 8,  TotalFOB = 1_057_360, BiensAlimentaires = 153_647, Vanille = 16_612,  Crevettes = 12_548, Cafe = 23_831, Girofle = 4_408,   ProduitsMiniers = 511_277, ZonesFranches = 280_023 },
        new() { Annee = 2024, Mois = 9,  TotalFOB = 1_117_201, BiensAlimentaires = 159_063, Vanille = 3,       Crevettes = 21_135, Cafe = 33_112, Girofle = 6_828,   ProduitsMiniers = 476_557, ZonesFranches = 302_016 },
        new() { Annee = 2024, Mois = 10, TotalFOB = 902_596,   BiensAlimentaires = 159_784, Vanille = 9_150,   Crevettes = 24_188, Cafe = 23_273, Girofle = 10_662,  ProduitsMiniers = 273_354, ZonesFranches = 326_233 },
        new() { Annee = 2024, Mois = 11, TotalFOB = 871_368,   BiensAlimentaires = 281_643, Vanille = 35_825,  Crevettes = 21_603, Cafe = 17_576, Girofle = 55_483,  ProduitsMiniers = 158_748, ZonesFranches = 288_332 },
        new() { Annee = 2024, Mois = 12, TotalFOB = 1_113_874, BiensAlimentaires = 243_191, Vanille = 51_895,  Crevettes = 23_829, Cafe = 20_100, Girofle = 87_640,  ProduitsMiniers = 429_373, ZonesFranches = 302_924 },

        // ──── 2025 (janv–sept) ────
        new() { Annee = 2025, Mois = 1,  TotalFOB = 1_010_031, BiensAlimentaires = 226_806, Vanille = 39_673,  Crevettes = 16_655, Cafe = 20_598, Girofle = 100_088, ProduitsMiniers = 355_464, ZonesFranches = 297_095 },
        new() { Annee = 2025, Mois = 2,  TotalFOB = 984_976,   BiensAlimentaires = 213_122, Vanille = 42_184,  Crevettes = 17_050, Cafe = 20_022, Girofle = 74_940,  ProduitsMiniers = 335_455, ZonesFranches = 291_152 },
        new() { Annee = 2025, Mois = 3,  TotalFOB = 787_440,   BiensAlimentaires = 237_015, Vanille = 70_381,  Crevettes = 15_159, Cafe = 30_287, Girofle = 66_536,  ProduitsMiniers = 123_081, ZonesFranches = 305_460 },
        new() { Annee = 2025, Mois = 4,  TotalFOB = 809_558,   BiensAlimentaires = 256_119, Vanille = 91_623,  Crevettes = 36_166, Cafe = 14_360, Girofle = 40_080,  ProduitsMiniers = 134_583, ZonesFranches = 303_323 },
        new() { Annee = 2025, Mois = 5,  TotalFOB = 922_266,   BiensAlimentaires = 237_858, Vanille = 82_335,  Crevettes = 42_496, Cafe = 7_378,  Girofle = 23_991,  ProduitsMiniers = 233_922, ZonesFranches = 369_097 },
        new() { Annee = 2025, Mois = 6,  TotalFOB = 974_189,   BiensAlimentaires = 276_471, Vanille = 169_167, Crevettes = 24_760, Cafe = 5_535,  Girofle = 9_173,   ProduitsMiniers = 259_579, ZonesFranches = 312_744 },
        new() { Annee = 2025, Mois = 7,  TotalFOB = 1_135_958, BiensAlimentaires = 168_213, Vanille = 18_753,  Crevettes = 23_282, Cafe = 716,    Girofle = 4_123,   ProduitsMiniers = 439_934, ZonesFranches = 368_414 },
        new() { Annee = 2025, Mois = 8,  TotalFOB = 851_575,   BiensAlimentaires = 116_262, Vanille = 6,       Crevettes = 17_804, Cafe = 0,      Girofle = 3_118,   ProduitsMiniers = 294_892, ZonesFranches = 279_017 },
        new() { Annee = 2025, Mois = 9,  TotalFOB = 844_910,   BiensAlimentaires = 117_811, Vanille = 5,       Crevettes = 19_395, Cafe = 4_671,  Girofle = 5_063,   ProduitsMiniers = 321_528, ZonesFranches = 267_121 },
    ];

    /// <summary>Cumul janv–sept 2025 pré-calculé (millions MGA)</summary>
    public static ExportHistoricalData CumulJanvSept2025 => new()
    {
        Annee = 2025, Mois = 0,
        TotalFOB = 8_320_903, BiensAlimentaires = 1_849_678, Vanille = 514_127,
        Crevettes = 212_766, Cafe = 103_566, Girofle = 327_112,
        ProduitsMiniers = 2_498_438, ZonesFranches = 2_793_423
    };

    /// <summary>Cumul janv–sept 2024 pré-calculé (millions MGA)</summary>
    public static ExportHistoricalData CumulJanvSept2024 => new()
    {
        Annee = 2024, Mois = 0,
        TotalFOB = 9_209_315, BiensAlimentaires = 2_485_543, Vanille = 953_116,
        Crevettes = 252_153, Cafe = 75_441, Girofle = 586_614,
        ProduitsMiniers = 2_967_275, ZonesFranches = 2_593_543
    };

    /// <summary>
    /// Variations en % janv–sept 2025 vs 2024.
    /// </summary>
    public static Dictionary<string, double> VariationsAnnuelles => new()
    {
        ["Total"] = -9.6,
        ["BiensAlimentaires"] = -25.6,
        ["Vanille"] = -46.1,
        ["Crevettes"] = -15.6,
        ["Cafe"] = 37.3,
        ["Girofle"] = -44.2,
        ["ProduitsMiniers"] = -15.8,
        ["ZonesFranches"] = 7.7,
    };

    private static string NomMois(int mois) => mois switch
    {
        1 => "janv", 2 => "fév", 3 => "mars", 4 => "avr",
        5 => "mai", 6 => "juin", 7 => "juil", 8 => "août",
        9 => "sept", 10 => "oct", 11 => "nov", 12 => "déc",
        _ => "cumul"
    };

    /// <summary>
    /// Calcule la moyenne journalière FOB par catégorie d'export à partir des données INSTAT.
    /// Retourne un dictionnaire ECategorieExport → millions MGA / jour.
    /// Formule : moyenne mensuelle INSTAT ÷ 30 jours.
    /// </summary>
    public static Dictionary<ECategorieExport, double> MoyenneJournaliereParCategorie()
    {
        var donnees = DonneesINSTAT();

        return new Dictionary<ECategorieExport, double>
        {
            [ECategorieExport.BiensAlimentaires] = donnees.Average(d => d.BiensAlimentaires) / 30.0,
            [ECategorieExport.Vanille] = donnees.Average(d => d.Vanille) / 30.0,
            [ECategorieExport.Crevettes] = donnees.Average(d => d.Crevettes) / 30.0,
            [ECategorieExport.Cafe] = donnees.Average(d => d.Cafe) / 30.0,
            [ECategorieExport.Girofle] = donnees.Average(d => d.Girofle) / 30.0,
            [ECategorieExport.ProduitsMiniers] = donnees.Average(d => d.ProduitsMiniers) / 30.0,
            [ECategorieExport.ZonesFranches] = donnees.Average(d => d.ZonesFranches) / 30.0,
        };
    }
}
