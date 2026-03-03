namespace MachineLearning.Web.Models.Agents.Companies;

/// <summary>
/// Données historiques réelles des importations CAF/CIF de Madagascar par catégorie.
/// Source : INSTAT — Tableau 33 : Évolution mensuelle des importations (millions d'Ariary).
/// Période couverte : juillet 2023 – juin 2025 (24 mois).
/// </summary>
public class ImportHistoricalData
{
    public int Annee { get; set; }
    public int Mois { get; set; }
    public string Periode => $"{NomMois(Mois)} {Annee:D2}";

    /// <summary>Total en valeur CAF/CIF (millions MGA)</summary>
    public double TotalCIF { get; set; }

    /// <summary>Biens alimentaires (millions MGA)</summary>
    public double BiensAlimentaires { get; set; }

    /// <summary>Riz (millions MGA)</summary>
    public double Riz { get; set; }

    /// <summary>Sucre (millions MGA)</summary>
    public double Sucre { get; set; }

    /// <summary>Énergie (carburants, électricité) (millions MGA)</summary>
    public double Energie { get; set; }

    /// <summary>Équipement (machines, véhicules) (millions MGA)</summary>
    public double Equipement { get; set; }

    /// <summary>Ciment et matériaux de construction (millions MGA)</summary>
    public double Ciment { get; set; }

    /// <summary>
    /// Retourne la valeur CIF par catégorie d'import.
    /// </summary>
    public double ParCategorie(ECategorieImport categorie) => categorie switch
    {
        ECategorieImport.Alimentaire => BiensAlimentaires,
        ECategorieImport.Carburant => Energie,
        ECategorieImport.Vehicule => Equipement,
        ECategorieImport.MatierePremiere => Ciment,
        ECategorieImport.BienConsommation => 0, // À compléter avec d'autres données si nécessaire
        ECategorieImport.Electronique => 0,
        _ => 0
    };

    private static string NomMois(int mois) => mois switch
    {
        1 => "Janvier",
        2 => "Février",
        3 => "Mars",
        4 => "Avril",
        5 => "Mai",
        6 => "Juin",
        7 => "Juillet",
        8 => "Août",
        9 => "Septembre",
        10 => "Octobre",
        11 => "Novembre",
        12 => "Décembre",
        _ => "?"
    };

    /// <summary>
    /// Données INSTAT Tableau 33 : Importations mensuelles (millions MGA).
    /// 24 mois : juillet 2023 → juin 2025.
    /// </summary>
    public static List<ImportHistoricalData> DonneesINSTAT() =>
    [
        // 2023
        new() { Annee = 2023, Mois = 7, TotalCIF = 1_728_418, BiensAlimentaires = 223_270, Riz = 41_010, Sucre = 18_480, Energie = 346_438, Equipement = 355_524, Ciment = 9_919 },
        new() { Annee = 2023, Mois = 8, TotalCIF = 1_787_858, BiensAlimentaires = 202_511, Riz = 96_933, Sucre = 23_427, Energie = 360_920, Equipement = 475_785, Ciment = 30_018 },
        new() { Annee = 2023, Mois = 9, TotalCIF = 1_919_554, BiensAlimentaires = 223_146, Riz = 47_702, Sucre = 36_854, Energie = 469_469, Equipement = 362_834, Ciment = 40_925 },
        new() { Annee = 2023, Mois = 10, TotalCIF = 1_892_301, BiensAlimentaires = 255_186, Riz = 51_302, Sucre = 28_075, Energie = 424_217, Equipement = 357_695, Ciment = 32_038 },
        new() { Annee = 2023, Mois = 11, TotalCIF = 1_539_625, BiensAlimentaires = 177_658, Riz = 29_562, Sucre = 28_248, Energie = 413_940, Equipement = 268_346, Ciment = 8_927 },
        new() { Annee = 2023, Mois = 12, TotalCIF = 1_866_972, BiensAlimentaires = 218_859, Riz = 98_047, Sucre = 18_397, Energie = 420_907, Equipement = 302_452, Ciment = 42_647 },
        
        // 2024
        new() { Annee = 2024, Mois = 1, TotalCIF = 1_482_732, BiensAlimentaires = 190_645, Riz = 50_082, Sucre = 22_250, Energie = 283_222, Equipement = 290_649, Ciment = 41_690 },
        new() { Annee = 2024, Mois = 2, TotalCIF = 1_704_766, BiensAlimentaires = 296_603, Riz = 117_085, Sucre = 30_032, Energie = 323_626, Equipement = 288_543, Ciment = 9_927 },
        new() { Annee = 2024, Mois = 3, TotalCIF = 1_560_021, BiensAlimentaires = 188_649, Riz = 44_995, Sucre = 36_912, Energie = 365_763, Equipement = 300_324, Ciment = 27_151 },
        new() { Annee = 2024, Mois = 4, TotalCIF = 2_002_110, BiensAlimentaires = 230_919, Riz = 54_773, Sucre = 70_893, Energie = 325_153, Equipement = 347_791, Ciment = 25_684 },
        new() { Annee = 2024, Mois = 5, TotalCIF = 1_609_505, BiensAlimentaires = 209_036, Riz = 39_966, Sucre = 44_574, Energie = 281_969, Equipement = 293_873, Ciment = 25_119 },
        new() { Annee = 2024, Mois = 6, TotalCIF = 1_869_088, BiensAlimentaires = 199_545, Riz = 9_269, Sucre = 46_951, Energie = 329_108, Equipement = 391_546, Ciment = 53_519 },
        new() { Annee = 2024, Mois = 7, TotalCIF = 1_967_356, BiensAlimentaires = 187_101, Riz = 11_064, Sucre = 55_164, Energie = 408_860, Equipement = 410_067, Ciment = 7_465 },
        new() { Annee = 2024, Mois = 8, TotalCIF = 1_937_515, BiensAlimentaires = 185_819, Riz = 4_820, Sucre = 46_664, Energie = 347_283, Equipement = 386_690, Ciment = 36_529 },
        new() { Annee = 2024, Mois = 9, TotalCIF = 2_073_656, BiensAlimentaires = 209_021, Riz = 32_914, Sucre = 42_101, Energie = 348_869, Equipement = 477_871, Ciment = 38_201 },
        new() { Annee = 2024, Mois = 10, TotalCIF = 2_199_679, BiensAlimentaires = 416_133, Riz = 200_333, Sucre = 70_710, Energie = 334_102, Equipement = 366_217, Ciment = 26_556 },
        new() { Annee = 2024, Mois = 11, TotalCIF = 1_942_353, BiensAlimentaires = 313_582, Riz = 66_045, Sucre = 53_439, Energie = 311_746, Equipement = 327_279, Ciment = 52_946 },
        new() { Annee = 2024, Mois = 12, TotalCIF = 1_863_330, BiensAlimentaires = 221_146, Riz = 49_242, Sucre = 42_393, Energie = 387_145, Equipement = 391_852, Ciment = 39_133 },

        // 2025
        new() { Annee = 2025, Mois = 1, TotalCIF = 2_466_009, BiensAlimentaires = 321_337, Riz = 181_490, Sucre = 35_824, Energie = 414_409, Equipement = 466_010, Ciment = 24_329 },
        new() { Annee = 2025, Mois = 2, TotalCIF = 2_132_077, BiensAlimentaires = 431_409, Riz = 270_909, Sucre = 38_719, Energie = 284_410, Equipement = 483_949, Ciment = 25_661 },
        new() { Annee = 2025, Mois = 3, TotalCIF = 1_932_636, BiensAlimentaires = 249_179, Riz = 183_046, Sucre = 29_530, Energie = 339_139, Equipement = 324_959, Ciment = 33_696 },
        new() { Annee = 2025, Mois = 4, TotalCIF = 1_765_410, BiensAlimentaires = 239_875, Riz = 86_911, Sucre = 38_075, Energie = 283_564, Equipement = 370_998, Ciment = 44_197 },
        new() { Annee = 2025, Mois = 5, TotalCIF = 1_866_021, BiensAlimentaires = 210_096, Riz = 55_047, Sucre = 20_015, Energie = 266_736, Equipement = 381_626, Ciment = 35_310 },
        new() { Annee = 2025, Mois = 6, TotalCIF = 1_607_158, BiensAlimentaires = 129_663, Riz = 51_970, Sucre = 14_557, Energie = 310_216, Equipement = 277_552, Ciment = 56_057 },
    ];

    /// <summary>
    /// Variations annuelles janv–juin 2025 vs 2024 (en %).
    /// </summary>
    public static Dictionary<string, double> VariationsAnnuelles => new()
    {
        ["Total"] = +13.3,
        ["Biens alim."] = -27.6,
        ["Riz"] = +192.8,
        ["Sucre"] = -28.6,
        ["Énergie"] = -1.0,
        ["Équipement"] = +30.3,
        ["Ciment"] = +19.9
    };

    /// <summary>
    /// Calcule la moyenne journalière CIF par catégorie d'import à partir des données INSTAT.
    /// Retourne un dictionnaire ECategorieImport → millions MGA / jour.
    /// Formule : moyenne mensuelle INSTAT ÷ 30 jours.
    /// Les catégories BienConsommation et Electronique ne sont pas couvertes par INSTAT
    /// et reçoivent une estimation par différence (TotalCIF - somme des catégories connues).
    /// </summary>
    public static Dictionary<ECategorieImport, double> MoyenneJournaliereParCategorie()
    {
        var donnees = DonneesINSTAT();
        int n = donnees.Count;

        double moyAlimentaire = donnees.Average(d => d.BiensAlimentaires);
        double moyEnergie = donnees.Average(d => d.Energie);
        double moyEquipement = donnees.Average(d => d.Equipement);
        double moyCiment = donnees.Average(d => d.Ciment);
        double moyTotal = donnees.Average(d => d.TotalCIF);

        // Catégories non détaillées par INSTAT : répartir le résidu
        double connues = moyAlimentaire + moyEnergie + moyEquipement + moyCiment;
        double residu = Math.Max(0, moyTotal - connues);
        double moyBienConso = residu * 0.65;  // ~65% du résidu
        double moyElectronique = residu * 0.35; // ~35% du résidu

        return new Dictionary<ECategorieImport, double>
        {
            [ECategorieImport.Alimentaire] = moyAlimentaire / 30.0,
            [ECategorieImport.Carburant] = moyEnergie / 30.0,
            [ECategorieImport.Vehicule] = moyEquipement / 30.0,
            [ECategorieImport.MatierePremiere] = moyCiment / 30.0,
            [ECategorieImport.BienConsommation] = moyBienConso / 30.0,
            [ECategorieImport.Electronique] = moyElectronique / 30.0,
        };
    }
}
