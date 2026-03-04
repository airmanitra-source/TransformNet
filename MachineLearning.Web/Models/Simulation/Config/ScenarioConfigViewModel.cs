using MachineLearning.Web.Models.Agents.Companies;
using Company.Module.Models;

namespace MachineLearning.Web.Models.Simulation.Config;

/// <summary>
/// Configuration d'un scénario de simulation économique.
/// </summary>
public class ScenarioConfigViewModel
{
    public string Name { get; set; } = "Scénario de base";
    public string Description { get; set; } = "Paramètres macroéconomiques de base de Madagascar";

    public static readonly double NombreMenagesReference = 6_000_000;
    public static readonly int CourtTerme = 90;
    public static readonly int MoyenTerme = 365;
    public static readonly int LongTerme = 1825;

    public int DureeJours { get; set; } = 365;
    public int NombreMenages { get; set; } = 100000;
    public int NombreEntreprises { get; set; } = 50000;

    public double TauxIS { get; set; } = 0.20;
    public double TauxTVA { get; set; } = 0.20;
    public double TauxDirecteur { get; set; } = 0.09;
    public double TauxInflation { get; set; } = 0.08;
    public double PrixCarburantLitre { get; set; } = 5_500;

    public double SalaireMedian { get; set; } = 170_000;
    public double SalaireSigma { get; set; } = 0.85;
    public double SalairePlancher { get; set; } = 50_000;
    public double PartSecteurInformel { get; set; } = 0.85;

    public double TauxEpargneMenage { get; set; } = 0.10;
    public double PropensionConsommation { get; set; } = 0.75;

    public double ConsommationRizAnnuelleKgParPersonne { get; set; } = 130;
    public double PrixRizLocalKg { get; set; } = 2_400;
    public double PrixRizImporteKg { get; set; } = 2_800;
    public double PartRizImporte { get; set; } = 0.18;

    public double TarifEauJourMenage { get; set; } = 500;
    public double PrixElectriciteArKWh { get; set; } = 653;
    public double ConsommationElecMenageKWhJour { get; set; } = 1.03;
    public double PartProductionHydraulique { get; set; } = 0.516;
    public double PartConsommationElecMenages { get; set; } = 0.474;
    public double TauxPertesDistribution { get; set; } = 0.289;

    public double TauxAccesEau { get; set; } = 0.25;
    public double TauxAccesElectricite { get; set; } = 0.30;
    public double CoutTransportPaiementJirama { get; set; } = 1_200;

    public double ConsommationElecParEmployeKWhJour { get; set; } = 2.5;
    public double ConsommationElecEtatKWhJour { get; set; } = 44_400;

    public double MargeBeneficiaireEntreprise { get; set; } = 0.20;
    public double ProductiviteParEmploye { get; set; } = 15_000;
    public double PartEntreprisesAgricoles { get; set; } = 0.30;
    public double PartEntreprisesConstruction { get; set; } = 0.05;

    public double TauxCotisationsPatronalesCNaPS { get; set; } = 0.18;
    public double TauxCotisationsSalarialesCNaPS { get; set; } = 0.01;

    public double AideInternationaleJour { get; set; } = 3_704_000_000;
    public double SubventionJiramaJour { get; set; } = 1_370_000_000;
    public double RemittancesJour { get; set; } = 7_400_000_000;

    public double LoyerImputeJourParMenage { get; set; } = 1_000;
    public double TauxMenagesProprietaires { get; set; } = 0.65;

    public int NombreFonctionnaires { get; set; } = 350_000;
    public double SalaireMoyenFonctionnaireMensuel { get; set; } = 863_000;

    public double TauxReinvestissementPrive { get; set; } = 0.25;
    public double DepensesCapitalJour { get; set; } = 13_526_000_000;
    public double InteretsDetteJour { get; set; } = 1_678_000_000;
    public double DettePubliqueInitiale { get; set; } = 29_250_000_000_000;

    public double TauxDroitsDouane { get; set; } = 0.12;
    public double TauxAccise { get; set; } = 0.10;
    public double TauxTaxeExport { get; set; } = 0.03;
    public double DepensesPubliquesJour { get; set; } = 3_218_000_000;

    // ══════════════════════════════════════════════════════════════
    // ░░░ NOUVEAUX PARAMÈTRES : AJUSTEMENT DE PRIX PAR CARBURANT ░░░
    // ══════════════════════════════════════════════════════════════

    /// <summary>
    /// Élasticité-prix : impact de la variation du carburant sur les prix des marchandises locales.
    /// 0.3 = faible transmission (commerce local absorbé les chocs)
    /// 0.7 = transmission modérée (réaliste pour Madagascar)
    /// 1.0 = transmission complète (100% du choc carburant → prix)
    /// </summary>
    public double ElasticitePrixParCarburant { get; set; } = 0.70;

    /// <summary>
    /// Volatilité de l'aléa de marché ~ N(0, σ).
    /// 0.05 = ±5% d'aléa journalier (marchés stables)
    /// 0.15 = ±15% d'aléa journalier (marchés informels volatiles)
    /// </summary>
    public double VolatiliteAleatoireMarche { get; set; } = 0.10;

    /// <summary>
    /// Part du revenu mensuel consacrée aux dépenses alimentaires.
    /// Utilisée pour calibrer l'impact comportemental des hausses de prix.
    /// Exemple: 0.40 = 40% du revenu en nourriture (réaliste pour pauvres).
    /// </summary>
    public double PartRevenuAlimentaire { get; set; } = 0.40;

    /// <summary>
    /// Élasticité du comportement du consommateur face aux chocs de prix répétitifs.
    /// 0.3 = peu sensible (accepte les augmentations)
    /// 0.7 = très sensible (réduit quantités rapidement)
    /// Affecte la courbe de privation (réduction quantités).
    /// </summary>
    public double ElasticiteComportementMenage { get; set; } = 0.65;

    /// <summary>
    /// Prix carburant de référence pour calibrage des élasticités.
    /// Permet de ramener tous les chocs de carburant à une base comparable.
    /// </summary>
    public double PrixCarburantReference { get; set; } = 5_500;

    // ════════════════════════════════════════════════════════════════

    public Dictionary<ESecteurActivite, double> TresorerieInitialeParSecteur { get; set; } = new()
    {
        { ESecteurActivite.Agriculture, 2_000_000 },
        { ESecteurActivite.Textiles, 10_000_000 },
        { ESecteurActivite.Commerces, 5_000_000 },
        { ESecteurActivite.Services, 8_000_000 },
        { ESecteurActivite.SecteurMinier, 50_000_000 },
        { ESecteurActivite.Construction, 15_000_000 }
    };

    public Dictionary<ECategorieExport, double> FOBJourParCategorie { get; set; } = new();
    public Dictionary<ECategorieImport, double> CIFJourParCategorie { get; set; } = new();

    public bool UseExportCalibresDirectement { get; set; } = true;
    public string SourceCalibration { get; set; } = "Moyenne INSTAT Tableau 32 (oct 2023 – sept 2025)";
    public Dictionary<ECategorieExport, double> FOBCalibresJour { get; set; } = new();

    public bool UseImportCalibresDirectement { get; set; } = true;
    public string SourceCalibrationImports { get; set; } = "Moyenne INSTAT Tableau 33 (juil 2023 – juin 2025)";
    public Dictionary<ECategorieImport, double> CIFCalibresJour { get; set; } = new();

    public static ScenarioConfigViewModel BaseMadagascar() => new();

    public static List<ScenarioConfigViewModel> TousLesScenarios()
    {
        return new List<ScenarioConfigViewModel>
        {
            // ══════════════════════════════════════════════════════════════════════
            // 1. SCÉNARIO DE BASE — Madagascar actuel (sept. 2025)
            // ══════════════════════════════════════════════════════════════════════
            new()
            {
                Name = "🇲🇬 Madagascar actuel (TOFE sept. 2025)",
                Description = "Calibrage réel sur données INSTAT/FMI sept. 2025. État économique stable, inflation modérée."
            },

            // ══════════════════════════════════════════════════════════════════════
            // 2. CHOC CARBURANT MODÉRÉ (court terme) — +10%
            // ══════════════════════════════════════════════════════════════════════
            new()
            {
                Name = "⛽ Choc carburant modéré (+10%)",
                Description = "Carburant: 5500 → 6050 MGA/L. Élasticité normale. Impact court terme : inflation 5-7%, résilience ménages.",
                PrixCarburantLitre = 6_050,
                PrixCarburantReference = 5_500,
                ElasticitePrixParCarburant = 0.70,
                VolatiliteAleatoireMarche = 0.10,
                ElasticiteComportementMenage = 0.65
            },

            // ══════════════════════════════════════════════════════════════════════
            // 3. CHOC CARBURANT SÉVÈRE (moyen terme) — +50%
            // ══════════════════════════════════════════════════════════════════════
            new()
            {
                Name = "⛽⛽ Crise carburant (moyen terme +50%)",
                Description = "Carburant: 5500 → 8250 MGA/L. Transmission forte. Inflation alimentaire, réduction quantités, impact PIB négatif.",
                PrixCarburantLitre = 8_250,
                PrixCarburantReference = 5_500,
                ElasticitePrixParCarburant = 0.75,
                VolatiliteAleatoireMarche = 0.12,
                ElasticiteComportementMenage = 0.70,
                PartRevenuAlimentaire = 0.45
            },

            // ══════════════════════════════════════════════════════════════════════
            // 4. STAGFLATION — +80% carburant, transmission très forte
            // ══════════════════════════════════════════════════════════════════════
            new()
            {
                Name = "🔥 Stagflation (crise sévère +80% carburant)",
                Description = "Carburant: 5500 → 9900 MGA/L. Transmission complète. Inflation 50-60%, privation ménages, chômage, cercle vicieux.",
                PrixCarburantLitre = 9_900,
                PrixCarburantReference = 5_500,
                ElasticitePrixParCarburant = 0.85,
                VolatiliteAleatoireMarche = 0.15,
                ElasticiteComportementMenage = 0.75,
                PartRevenuAlimentaire = 0.50,
                TauxDirecteur = 0.15,
                TauxInflation = 0.20
            },

            // ══════════════════════════════════════════════════════════════════════
            // 5. RÉSILIENCE — Commerce absorbe les chocs
            // ══════════════════════════════════════════════════════════════════════
            new()
            {
                Name = "💪 Résilience (absorption des chocs)",
                Description = "Carburant monte mais commerce absorbe 70%. Volatilité basse. Ménages peu affectés, économie stable.",
                PrixCarburantLitre = 8_250,
                PrixCarburantReference = 5_500,
                ElasticitePrixParCarburant = 0.30,
                VolatiliteAleatoireMarche = 0.05,
                ElasticiteComportementMenage = 0.30,
                PartRevenuAlimentaire = 0.35
            },

            // ══════════════════════════════════════════════════════════════════════
            // 6. STIMULUS FISCAL — Augmentation dépenses publiques
            // ══════════════════════════════════════════════════════════════════════
            new()
            {
                Name = "📊 Stimulus fiscal (+20% dépenses publiques)",
                Description = "État augmente dépenses capital et fonctionnement. Demande aggregate ↑, emploi ↑, inflation modérée.",
                DepensesPubliquesJour = 3_861_600_000,  // +20%
                DepensesCapitalJour = 16_231_200_000,   // +20%
                TauxDirecteur = 0.06,
                TauxInflation = 0.10
            },

            // ══════════════════════════════════════════════════════════════════════
            // 7. AUSTÉRITÉ BUDGÉTAIRE — Réduction dépenses État
            // ══════════════════════════════════════════════════════════════════════
            new()
            {
                Name = "⚠️ Austérité budgétaire (-20% dépenses publiques)",
                Description = "État réduit drastiquement. Chômage du secteur public, baisse demande, récession.",
                DepensesPubliquesJour = 2_574_400_000,  // -20%
                DepensesCapitalJour = 10_820_800_000,   // -20%
                NombreFonctionnaires = 280_000,         // -20%
                TauxDirecteur = 0.12,
                TauxInflation = 0.12
            },

            // ══════════════════════════════════════════════════════════════════════
            // 8. BOOM EXPORT — Volumes export INSTAT +30%
            // ══════════════════════════════════════════════════════════════════════
            new()
            {
                Name = "📦 Boom export (vanilla, nickel, textile)",
                Description = "Exportations +30% (prix mondiaux ↑ ou volumes). Devise forte, investissements, emploi privé ↑.",
                UseExportCalibresDirectement = true,
                TauxIS = 0.15,
                TauxTVA = 0.18,
                PartEntreprisesConstruction = 0.08
            },

            // ══════════════════════════════════════════════════════════════════════
            // 9. CHOC IMPORTATIONS — Augmentation prix mondiaux
            // ══════════════════════════════════════════════════════════════════════
            new()
            {
                Name = "🌍 Choc importations (prix mondiaux +25%)",
                Description = "Coût CIF monte de 25%. Inflation importée, déficit commercial, pression reserve devises.",
                PrixRizImporteKg = 3_500,  // +25%
                PrixCarburantLitre = 6_875,  // +25%
                TauxDroitsDouane = 0.15,
                TauxAccise = 0.12,
                TauxInflation = 0.12
            },

            // ══════════════════════════════════════════════════════════════════════
            // 10. CHOC EMPLOI NÉGATIF — Entreprises réduisent effectifs
            // ══════════════════════════════════════════════════════════════════════
            new()
            {
                Name = "😔 Crise emploi (chômage +30%)",
                Description = "Entreprises réduisent effectifs de 30%. Baisse salaires, consommation, relayage par secteur informel.",
                NombreMenages = 70_000,
                NombreEntreprises = 35_000,
                MargeBeneficiaireEntreprise = 0.15,
                TauxTVA = 0.16,
                RemittancesJour = 9_620_000_000  // +30% remittances compensent
            },

            // ══════════════════════════════════════════════════════════════════════
            // 11. INNOVATION / PRODUCTIVITÉ — +15% efficacité
            // ══════════════════════════════════════════════════════════════════════
            new()
            {
                Name = "🚀 Innovation & productivité (+15%)",
                Description = "Secteurs modernes (FDI textile, Ambatovy expansion). VA par employé ↑, salaires ↑, gini ↓.",
                ProductiviteParEmploye = 17_250,  // +15%
                SalaireMedian = 195_500,  // +15%
                MargeBeneficiaireEntreprise = 0.25,
                TauxReinvestissementPrive = 0.35
            },

            // ══════════════════════════════════════════════════════════════════════
            // 12. CLIMAT DÉFAVORABLE — Cyclone, sécheresse
            // ══════════════════════════════════════════════════════════════════════
            new()
            {
                Name = "🌪️ Choc climatique (récoltes -40%)",
                Description = "Cyclone/sécheresse détruit récoltes. Secteur agricole -40%, inflation alimentaire, pauvreté temporaire.",
                PartEntreprisesAgricoles = 0.18,  // -40% production
                ConsommationRizAnnuelleKgParPersonne = 130,
                PrixRizLocalKg = 3_500,  // +45% pénurie
                PrixRizImporteKg = 4_200,
                TauxInflation = 0.15
            },

            // ══════════════════════════════════════════════════════════════════════
            // 13. POLITIQUE MONÉTAIRE EXPANSIVE — Taux directeur bas
            // ══════════════════════════════════════════════════════════════════════
            new()
            {
                Name = "💰 Monnaie facile (taux directeur 3%)",
                Description = "Banque centrale baisse taux. Crédit ↑, consommation ↑, investissement ↑, mais inflation risque ↑.",
                TauxDirecteur = 0.03,
                SubventionJiramaJour = 1_644_000_000,  // +20% subvention
                RemittancesJour = 8_880_000_000  // -20% remittances (dévaluation)
            },

            // ══════════════════════════════════════════════════════════════════════
            // 14. CONSOLIDATION FISCALE — Augmentation TVA
            // ══════════════════════════════════════════════════════════════════════
            new()
            {
                Name = "📈 Hausse TVA (18% → 22%)",
                Description = "Gouvernement augmente TVA pour réduire déficit. Inflation court terme, baisse consommation, mais finances saines LT.",
                TauxTVA = 0.22,
                TauxDirecteur = 0.10,
                AideInternationaleJour = 4_445_000_000  // +20% FMI
            },

            // ══════════════════════════════════════════════════════════════════════
            // 15. SCÉNARIO OPTIMISTE 2030 — Croissance inclusive
            // ══════════════════════════════════════════════════════════════════════
            new()
            {
                Name = "🌟 Vision 2030 (croissance inclusive)",
                Description = "Investissement FDI, réduction inégalités, inflation 4%, emploi +25%, Gini -10%, PIB +7%/an.",
                NombreMenages = 120_000,
                NombreEntreprises = 62_500,
                SalaireMedian = 220_000,
                SalaireSigma = 0.70,  // Moins d'inégalités
                MargeBeneficiaireEntreprise = 0.22,
                ProductiviteParEmploye = 19_500,
                TauxDirecteur = 0.05,
                TauxInflation = 0.04,
                TauxIS = 0.18,
                TauxTVA = 0.20,
                PartEntreprisesConstruction = 0.10
            }
        };
    }
}






