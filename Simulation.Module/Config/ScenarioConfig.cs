using Company.Module.Models;
using Household.Module.Models;

namespace Simulation.Module.Config;

/// <summary>
/// Configuration d'un scénario de simulation économique.
/// </summary>
public class ScenarioConfig
{
    public string Name { get; set; } = "Scénario de base";
    public string Description { get; set; } = "Paramètres macroéconomiques de base de Madagascar";

    /// <summary>
    /// Résumé structuré des implications économiques attendues pour ce scénario.
    /// Affiché dans l'encadré bleu de l'interface utilisateur.
    /// </summary>
    public ScenarioImplications Implications { get; set; } = new();

    public static readonly double NombreMenagesReference = 6_000_000;

    /// <summary>
    /// Ratio ménages / entreprises dans l'économie réelle malgache.
    /// Madagascar : ~6M ménages pour ~500k entreprises (formelles + informelles significatives) ≈ 12:1.
    /// Utilisé pour calculer le facteur correctif de trésorerie quand NombreEntreprises
    /// n'est pas proportionnel à NombreMenages.
    /// </summary>
    public static readonly double RatioMenagesParEntrepriseReference = 12.0;

    public static readonly int CourtTerme = 90;
    public static readonly int MoyenTerme = 365;
    public static readonly int LongTerme = 1825;

    public int DureeJours { get; set; } = 365;
    public int NombreMenages { get; set; } = 100_000;

    /// <summary>
    /// Nombre d'entreprises simulées. Le défaut (8 000) maintient le ratio ~12:1
    /// observé dans l'économie malgache (6M ménages / ~500k entreprises).
    /// Un ratio trop bas (ex: 100k/50k = 2:1) gonfle artificiellement les dépôts
    /// bancaires et donc M3.
    /// </summary>
    public int NombreEntreprises { get; set; } = 8_000;

    public double TauxIS { get; set; } = 0.20;
    public double TauxTVA { get; set; } = 0.20;
    public double TauxDirecteur { get; set; } = 0.09;
    public double TauxInflation { get; set; } = 0.08;
    public double PrixCarburantLitre { get; set; } = 5_500;

    public double SalaireMedian { get; set; } = 170_000;
    public double SalaireSigma { get; set; } = 0.85;
    public double SalairePlancher { get; set; } = 50_000;
    public double PartSecteurInformel { get; set; } = 0.85;

    /// <summary>
    /// Bornes du facteur de productivité pour les entreprises informelles (0-1).
    /// Reflète l'absence de capital, formation et technologie dans l'informel.
    /// INSTAT ENEMPSI : productivité informelle ≈ 30-60% du formel à secteur égal.
    /// Les entreprises formelles ont un facteur de 1.0 (aucune réduction).
    /// </summary>
    public double FacteurProductiviteInformelMin { get; set; } = 0.30;
    public double FacteurProductiviteInformelMax { get; set; } = 0.60;

    public double TauxEpargneMenage { get; set; } = 0.10;
    public double PropensionConsommation { get; set; } = 0.75;
    // Propension marginale à consommer par classe socio-économique (0-1)
    public double PropensionConsommation_Subsistance { get; set; } = 0.92; // précédemment behavior
    public double PropensionConsommation_InformelBas { get; set; } = 0.85;
    public double PropensionConsommation_FormelBas { get; set; } = 0.75;
    public double PropensionConsommation_FormelQualifie { get; set; } = 0.65;
    public double PropensionConsommation_Cadre { get; set; } = 0.50;

    public double GetPropensionParClasse(Company.Module.Models.ESecteurActivite? dummy = null) => PropensionConsommation; // placeholder

    public double GetPropensionParClasse(Household.Module.Models.ClasseSocioEconomique classe)
    {
        return classe switch
        {
            ClasseSocioEconomique.Subsistance => PropensionConsommation_Subsistance,
            ClasseSocioEconomique.InformelBas => PropensionConsommation_InformelBas,
            ClasseSocioEconomique.FormelBas => PropensionConsommation_FormelBas,
            ClasseSocioEconomique.FormelQualifie => PropensionConsommation_FormelQualifie,
            ClasseSocioEconomique.Cadre => PropensionConsommation_Cadre,
            _ => PropensionConsommation
        };
    }

    public double ConsommationRizAnnuelleKgParPersonne { get; set; } = 130;
    public double PrixRizLocalKg { get; set; } = 2_400;
    public double PrixRizImporteKg { get; set; } = 2_800;
    public double PartRizImporte { get; set; } = 0.18;

    // Chargés depuis sim.ParamJirama via ScenarioConfigLoader — pas de default C# intentionnel.
    public double TarifEauJourMenage { get; set; }
    public double PrixElectriciteArKWh { get; set; }
    public double ConsommationElecMenageKWhJour { get; set; }
    public double PartProductionHydraulique { get; set; }
    public double PartConsommationElecMenages { get; set; }
    public double TauxPertesDistribution { get; set; }

    public double TauxAccesEau { get; set; }
    public double TauxAccesElectricite { get; set; }
    public double CoutTransportPaiementJirama { get; set; }

    public double ConsommationElecParEmployeKWhJour { get; set; }
    public double ConsommationElecEtatKWhJour { get; set; }

    // ─── Agent Jirama ───────────────────────────────────────────────────
    /// <summary>Trésorerie initiale de l'agent Jirama avant facteur d'échelle (MGA).</summary>
    public double JiramaTresorerieInitiale { get; set; }
    /// <summary>Nombre d'employés de base de la Jirama avant facteur d'échelle.</summary>
    public int JiramaNombreEmployesBase { get; set; }
    /// <summary>Salaire moyen mensuel des employés Jirama (MGA).</summary>
    public double JiramaSalaireMoyenMensuelEmploye { get; set; }

    public double MargeBeneficiaireEntreprise { get; set; } = 0.20;
    public double ProductiviteParEmploye { get; set; } = 15_000;
    public double PartEntreprisesAgricoles { get; set; } = 0.30;
    public double PartEntreprisesConstruction { get; set; } = 0.05;
    /// <summary>Part des entreprises dans le secteur hôtellerie/tourisme (~3-4% à Madagascar)</summary>
    public double PartEntreprisesHotellerieTourisme { get; set; } = 0.03;

    /// <summary>Marge de revente des importateurs agrégés sur la valeur CIF. Chargé depuis sim.ParamEntreprises.</summary>
    public double MargeReventeImport { get; set; }

    /// <summary>Part de la production des exportateurs orientée vers l'export. Chargé depuis sim.ParamEntreprises.</summary>
    public double PartExporteurProduction { get; set; }

    public double TauxCotisationsPatronalesCNaPS { get; set; } = 0.18;
    public double TauxCotisationsSalarialesCNaPS { get; set; } = 0.01;

    public double AideInternationaleJour { get; set; } = 3_704_000_000;
    public double SubventionJiramaJour { get; set; } = 1_370_000_000;
    public double RemittancesJour { get; set; } = 7_400_000_000;

    /// <summary>
    /// Élasticité des transferts diaspora au taux de change MGA/USD.
    /// Quand le MGA se déprécie, les remittances en MGA augmentent
    /// (effet mécanique + comportemental : la diaspora envoie plus).
    /// Littérature Afrique subsaharienne : élasticité ≈ 0.3-0.8.
    /// 0.50 = +5% de remittances MGA pour +10% de dépréciation MGA/USD.
    /// </summary>
    public double ElasticiteRemittancesChange { get; set; } = 0.50;

    public double LoyerImputeJourParMenage { get; set; } = 1_000;
    public double TauxMenagesProprietaires { get; set; } = 0.65;
    public double LoyerJourLocataire { get; set; } = 3_500;
    public double ProbabiliteConstructionMaisonLocataire { get; set; } = 0.08;
    public int DureeConstructionMaisonJours { get; set; } = 240;
    public double BudgetConstructionMaisonJour { get; set; } = 7_500;
    public double PartBudgetConstructionBTP { get; set; } = 0.55;
    public double PartBudgetConstructionQuincaillerie { get; set; } = 0.30;
    public double PartBudgetConstructionTransportInformel { get; set; } = 0.15;

    public double NombreEnfantsMoyenParMenage { get; set; } = 2.3;
    public double PartEnfantsScolarises { get; set; } = 0.72;
    public int DureeDepenseEducationJours { get; set; } = 180;
    public double CoutEducationJournalierParEnfant { get; set; } = 900;
    public double PartFormelleDepenseEducation { get; set; } = 0.75;

    public double TauxOccupationHopitaux { get; set; } = 0.68;
    public double CoutConsultationSanteBase { get; set; } = 8_000;
    public double CoutHospitalisationSanteBase { get; set; } = 45_000;
    public double PartFormelleDepenseSante { get; set; } = 0.70;

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
    // ░░░ TAUX DE CHANGE DYNAMIQUE MGA/USD ░░░
    // ════════════════════════════════════════════════════════════════

    /// <summary>
    /// Active le taux de change dynamique.
    /// Si true, le taux MGA/USD est recalculé chaque jour à partir des flux de devises.
    /// Si false, le taux reste fixe (TauxChangeMGAParUSD).
    /// </summary>
    public bool TauxChangeDynamiqueActive { get; set; } = true;

    /// <summary>
    /// Taux de change initial MGA par USD. BCM sept. 2024 ≈ 4 500.
    /// Interprétation : 1 USD = 4 500 MGA.
    /// </summary>
    public double TauxChangeMGAParUSD { get; set; } = 4_500;

    /// <summary>
    /// Taux de change MGA par EUR. BCM sept. 2024 ≈ 5 000.
    /// Calculé comme TauxChangeMGAParUSD × 1.11 (parité EUR/USD).
    /// </summary>
    public double TauxChangeMGAParEUR => TauxChangeMGAParUSD * 1.11;

    /// <summary>
    /// Réserves de change BCM en USD. BCM 2024 ≈ 2.5 milliards USD.
    /// Couvrent environ 5 mois d'importations.
    /// </summary>
    public double ReservesBCMUSD { get; set; } = 2_500_000_000;

    /// <summary>
    /// Taux d'inflation étrangère (USD zone, annuel).
    /// Sert au calcul de la PPA relative. FED 2024 ≈ 2.5-3.5%.
    /// </summary>
    public double InflationEtrangere { get; set; } = 0.03;

    /// <summary>
    /// Élasticité du taux de change au solde commercial (0-1).
    /// 0 = fixité (peg), 1 = flottement libre.
    /// Madagascar ≈ 0.5 (flottement géré BCM).
    /// </summary>
    public double ElasticiteChangeBalanceCommerciale { get; set; } = 0.50;

    /// <summary>
    /// Poids de la PPA relative dans le taux de change (0-1).
    /// Convergence lente vers la parité de pouvoir d'achat.
    /// </summary>
    public double PoidsChangePPA { get; set; } = 0.30;

    /// <summary>
    /// Intensité d'intervention BCM (0 = aucune, 1 = fixité totale).
    /// 0.5 = intervention modérée pour lisser la volatilité.
    /// </summary>
    public double IntensiteInterventionBCM { get; set; } = 0.50;

    /// <summary>
    /// Réserves minimales BCM en mois d'imports.
    /// En dessous, la BCM cesse d'intervenir. FMI recommande 3 mois.
    /// </summary>
    public double ReservesMinimalesMoisImports { get; set; } = 3.0;

    /// <summary>
    /// Tendance de dépréciation structurelle annuelle (ex: 0.05 = 5%/an).
    /// Historique Madagascar 2015-2024 ≈ 5-7%/an.
    /// </summary>
    public double DepreciationStructurelleAnnuelle { get; set; } = 0.05;

    /// <summary>
    /// Élasticité du taux de change vers l'inflation (pass-through, ζ).
    /// Part de la dépréciation MGA transmise aux prix intérieurs.
    /// Madagascar ≈ 0.20-0.40 (pass-through modéré à élevé,
    /// car ~40% du PIB est importé).
    /// </summary>
    public double ElasticiteChangeInflation { get; set; } = 0.30;

    // ════════════════════════════════════════════════════════════════

    // ════════════════════════════════════════════════════════════════
    // ░░░ SECTEUR BANCAIRE ET MONÉTAIRE ░░░
    // ════════════════════════════════════════════════════════════════
    public double TauxReserveObligatoire { get; set; } = 0.13;
    public double CroissanceCreditJour { get; set; } = 0.00041; // Permet de viser +15% sur un an (1.15^(1/365) - 1)

    /// <summary>
    /// Part des dépôts considérés comme "à vue" (comptes courants, M1).
    /// BCM 2024 : ~55% des dépôts sont à vue.
    /// </summary>
    public double PartDepotsAVue { get; set; } = 0.55;

    /// <summary>
    /// Part de la monnaie fiduciaire dans M1.
    /// BCM 2024 : billets et pièces ≈ 45% de M1.
    /// </summary>
    public double PartMonnaieCirculationDansM1 { get; set; } = 0.45;

    /// <summary>
    /// Ratio M3/M2 pour capturer les autres actifs financiers (titres, placements).
    /// BCM 2024 : M3/M2 ≈ 1.08-1.12.
    /// </summary>
    public double RatioM3SurM2 { get; set; } = 1.10;

    /// <summary>
    /// Taux d'intérêt annuel sur les dépôts (rémunération de l'épargne).
    /// BCM rapport conjoncturel 2024 : taux créditeur moyen pondéré ~4-5%.
    /// </summary>
    public double TauxInteretDepots { get; set; } = 0.045;

    /// <summary>
    /// Taux d'intérêt annuel sur les crédits (coût du crédit bancaire).
    /// BCM rapport conjoncturel 2024 : taux débiteur moyen pondéré ~14-18%.
    /// </summary>
    public double TauxInteretCredits { get; set; } = 0.16;

    /// <summary>
    /// Probabilité journalière de défaut sur un crédit (passage en NPL).
    /// Calibré pour un ratio NPL/crédits cible ~7-9% (BCM 2024).
    /// 0.0003 ≈ ~10% des crédits deviennent NPL sur 1 an.
    /// </summary>
    public double ProbabiliteDefautCreditJour { get; set; } = 0.0003;

    /// <summary>
    /// Taux de recouvrement journalier des NPL (provision + récupération).
    /// 0.002 = ~0.2%/jour des NPL sont récupérés ou provisionnés.
    /// </summary>
    public double TauxRecouvrementNPLJour { get; set; } = 0.002;

    /// <summary>
    /// Avoirs extérieurs nets initiaux (Foreign Assets) en MGA.
    /// BCM 2024 : réserves de change ~2.5 Mds USD × 4500 MGA/USD ≈ 11 250 Mds MGA.
    /// Mis à l'échelle par facteurEchelle.
    /// </summary>
    public double AvoirsExterieursNetsInitiaux { get; set; } = 11_250_000_000_000;

    /// <summary>
    /// Créances nettes sur l'État initiales (avances BCM au Trésor, MGA).
    /// BCM 2024 : ~3 000 Mds MGA.
    /// Mis à l'échelle par facteurEchelle.
    /// </summary>
    public double CreancesNettesEtatInitiales { get; set; } = 3_000_000_000_000;

    /// <summary>
    /// Part du crédit distribué aux entreprises (vs ménages).
    /// BCM : ~75% du crédit bancaire privé va aux entreprises.
    /// </summary>
    public double PartCreditEntreprises { get; set; } = 0.75;

    /// <summary>
    /// SCB initial — Solde en Compte des Banques à la BFM (MGA).
    /// BFM Sept. 2025 : 2 430,2 Mds MGA. Mis à l'échelle par facteurEchelle.
    /// Proxy de la liquidité bancaire du système.
    /// </summary>
    public double SCBInitial { get; set; } = 2_430_000_000_000;

    /// <summary>
    /// Intensité d'intervention BFM pour maintenir l'excédent SCB-RO cible (0-1).
    /// 0 = aucune intervention, 0.5 = modérée, 1.0 = stérilisation totale.
    /// </summary>
    public double IntensiteInterventionBFM { get; set; } = 0.50;

    /// <summary>
    /// Ratio d'excédent SCB-RO cible (en fraction des RO).
    /// BFM T3 2025 : excédent moyen 126 Mds / RO ~2300 Mds ≈ 5.5%.
    /// </summary>
    public double RatioExcedentSCBCible { get; set; } = 0.055;

    // ════════════════════════════════════════════════════════════════

    // ════════════════════════════════════════════════════════════════
    // ░░░ INVESTISSEMENT PRODUCTIF (FBCF) ░░░
    // ════════════════════════════════════════════════════════════════

    /// <summary>
    /// Active le module d'investissement productif (FBCF privée par entreprise).
    /// Essentiel pour les simulations >1 an : sans lui, la capacité de production est fixe.
    /// </summary>
    public bool InvestissementProductifActive { get; set; } = true;

    /// <summary>
    /// Taux de dépréciation annuel du capital (0-1).
    /// Madagascar : ~5-8% selon le type d'actif.
    /// Agriculture (terre) : ~3-4% | Minier (machines) : ~10-12%.
    /// </summary>
    public double TauxDepreciationCapitalAnnuel { get; set; } = 0.07;

    /// <summary>
    /// Seuil de taux d'utilisation de la capacité pour investir (0-1).
    /// L'entreprise n'investit que si la demande utilise ≥70% de sa capacité.
    /// </summary>
    public double SeuilUtilisationInvestissement { get; set; } = 0.70;

    /// <summary>
    /// Élasticité du capital à la productivité (α dans Prod = Base × (1 + α × ln(1 + K/K₀))).
    /// 0.08 = effet modéré, rendements décroissants marqués.
    /// </summary>
    public double ElasticiteCapitalProductivite { get; set; } = 0.08;

    // ════════════════════════════════════════════════════════════════
    // ░░░ MATRICE INPUT-OUTPUT (TABLEAU DES ENTRÉES-SORTIES) ░░░
    // ════════════════════════════════════════════════════════════════

    /// <summary>
    /// Active la matrice input-output simplifiée pour ventiler les achats B2B
    /// entre secteurs fournisseurs. Fournit des multiplicateurs sectoriels réalistes.
    /// </summary>
    public bool InputOutputActivee { get; set; } = true;

    // ════════════════════════════════════════════════════════════════
    // ░░░ SAISONNALITÉ AGRICOLE ░░░
    // ════════════════════════════════════════════════════════════════

    // ════════════════════════════════════════════════════════════════
    // ░░░ DUALITÉ URBAIN / RURAL ░░░
    // ════════════════════════════════════════════════════════════════

    /// <summary>
    /// Part des ménages en zone urbaine (~30% à Madagascar, RGPH 2018).
    /// Les 70% restants sont ruraux avec des comportements très différents.
    /// C'est LA distinction la plus structurante pour Madagascar.
    /// </summary>
    public double PartMenagesUrbains { get; set; } = 0.30;

    /// <summary>
    /// Multiplicateur de prix pour les zones urbaines par rapport à la base.
    /// Les prix en ville sont plus élevés (~15-25% de plus).
    /// </summary>
    public double MultiplicateurPrixUrbain { get; set; } = 1.20;

    /// <summary>
    /// Multiplicateur de prix pour les zones rurales.
    /// Prix plus bas en rural, sauf en période de soudure.
    /// </summary>
    public double MultiplicateurPrixRural { get; set; } = 0.85;

    /// <summary>
    /// Multiplicateur de salaire urbain (salaires plus élevés en ville).
    /// INSTAT EPM : salaire médian urbain ~1.5x le rural.
    /// </summary>
    public double MultiplicateurSalaireUrbain { get; set; } = 1.40;

    /// <summary>
    /// Multiplicateur de salaire rural.
    /// </summary>
    public double MultiplicateurSalaireRural { get; set; } = 0.75;

    /// <summary>
    /// Taux d'accès à l'eau en zone urbaine (~50% vs 10% en rural).
    /// </summary>
    public double TauxAccesEauUrbain { get; set; } = 0.50;

    /// <summary>
    /// Taux d'accès à l'eau en zone rurale.
    /// </summary>
    public double TauxAccesEauRural { get; set; } = 0.10;

    /// <summary>
    /// Taux d'accès à l'électricité en zone urbaine (~65% vs 8% en rural).
    /// </summary>
    public double TauxAccesElectriciteUrbain { get; set; } = 0.65;

    /// <summary>
    /// Taux d'accès à l'électricité en zone rurale.
    /// </summary>
    public double TauxAccesElectriciteRural { get; set; } = 0.08;

    // ════════════════════════════════════════════════════════════════

    // ════════════════════════════════════════════════════════════════
    // ░░░ AUTOCONSOMMATION AGRICOLE ░░░
    // ════════════════════════════════════════════════════════════════

    /// <summary>
    /// Part des ménages ruraux pratiquant l'autoconsommation agricole (~80%).
    /// Ces ménages consomment une partie de leur production sans transaction monétaire.
    /// </summary>
    public double PartMenagesRurauxAutoconsommation { get; set; } = 0.80;

    /// <summary>
    /// Part de la production agricole autoconsommée par les ménages ruraux (~40%).
    /// Non capturée par le PIB marchand → le PIB est surestimé si on ne l'impute pas.
    /// Source : INSTAT EPM — comptes des ménages ruraux.
    /// </summary>
    public double PartProductionAutoconsommee { get; set; } = 0.40;

    /// <summary>
    /// Valeur monétaire imputée de l'autoconsommation journalière par ménage (MGA).
    /// Estimation : ~2 500 MGA/jour (équivalent riz + légumes du potager).
    /// </summary>
    public double ValeurAutoconsommationJourBase { get; set; } = 2_500;

    // ════════════════════════════════════════════════════════════════

    // ════════════════════════════════════════════════════════════════
    // ░░░ CROISSANCE DÉMOGRAPHIQUE ░░░
    // ════════════════════════════════════════════════════════════════

    /// <summary>
    /// Active la croissance démographique pendant la simulation.
    /// Si true, de nouveaux ménages sont ajoutés périodiquement (+2.7%/an).
    /// Sans cela, une simulation 5 ans est structurellement biaisée.
    /// </summary>
    public bool CroissanceDemographiqueActivee { get; set; } = true;

    /// <summary>
    /// Taux de croissance démographique annuel (~2.7% à Madagascar, Banque Mondiale 2024).
    /// Converti en taux journalier pour l'ajout progressif de ménages.
    /// </summary>
    public double TauxCroissanceDemographiqueAnnuel { get; set; } = 0.027;

    // ════════════════════════════════════════════════════════════════

    // ════════════════════════════════════════════════════════════════
    // ░░░ SÉCHERESSE GRAND SUD (KERE) ░░░
    // ════════════════════════════════════════════════════════════════

    /// <summary>
    /// Active les chocs de sécheresse dans le Grand Sud.
    /// Contrairement aux cyclones : pas de reconstruction BTP, mais aide alimentaire
    /// et migration interne vers les villes.
    /// </summary>
    public bool ChocsSecheresseActives { get; set; } = true;

    /// <summary>
    /// Probabilité journalière de début de sécheresse pendant la saison sèche (mai-nov).
    /// 0.001 ≈ ~16% de chance sur 6 mois. Le kere est moins fréquent que les cyclones.
    /// </summary>
    public double ProbabiliteSecheresseJourSaison { get; set; } = 0.001;

    /// <summary>
    /// Part des ménages du Grand Sud affectés par la sécheresse (15-25%).
    /// Le Grand Sud représente ~10% de la population malgache.
    /// </summary>
    public double PartMenagesAffectesSecheresse { get; set; } = 0.08;

    /// <summary>
    /// Durée moyenne d'un épisode de sécheresse (jours).
    /// Le kere peut durer 90-180 jours (3-6 mois).
    /// </summary>
    public int DureeSecheresseJours { get; set; } = 120;

    /// <summary>
    /// Réduction de la production agricole pendant la sécheresse (0-1).
    /// 0.60 = perte de 60% de la production dans les zones touchées.
    /// </summary>
    public double ReductionProductionAgricoleSecheresse { get; set; } = 0.60;

    /// <summary>
    /// Aide alimentaire journalière par ménage affecté (MGA).
    /// PAM/BNGRC : distributions de riz, légumineuses, huile.
    /// Valeur estimée : ~3 000 MGA/jour/ménage.
    /// </summary>
    public double AideAlimentaireSecheresseJourParMenage { get; set; } = 3_000;

    /// <summary>
    /// Probabilité de migration interne pour les ménages touchés par le kere (0-1).
    /// ~10-15% des ménages touchés migrent vers les villes (Toliara, Tana).
    /// </summary>
    public double ProbabiliteMigrationSecheresse { get; set; } = 0.12;

    // ════════════════════════════════════════════════════════════════

    // ════════════════════════════════════════════════════════════════
    // ░░░ MICROFINANCE / CRÉDIT SEGMENTÉ ░░░
    // ════════════════════════════════════════════════════════════════

    /// <summary>
    /// Part des ménages ayant accès au crédit bancaire formel (~10%).
    /// Principalement cadres et formel qualifié en zone urbaine.
    /// Source : BCM rapport inclusion financière 2024.
    /// </summary>
    public double PartMenagesCreditBancaire { get; set; } = 0.10;

    /// <summary>
    /// Part des ménages ayant accès à la microfinance / IMF (~25%).
    /// Taux d'intérêt : 24-36% annuel (CNFI Madagascar).
    /// </summary>
    public double PartMenagesCreditMicrofinance { get; set; } = 0.25;

    /// <summary>
    /// Part des ménages participant à des tontines informelles (~30%).
    /// Système d'épargne rotative sans intérêt formel.
    /// </summary>
    public double PartMenagesTontine { get; set; } = 0.30;

    /// <summary>
    /// Taux d'intérêt annuel de la microfinance (24-36%).
    /// Bien plus élevé que le crédit bancaire (14-18%).
    /// </summary>
    public double TauxInteretMicrofinanceAnnuel { get; set; } = 0.30;

    /// <summary>
    /// Montant maximal d'un crédit microfinance par ménage (MGA).
    /// IMF Madagascar : plafond typique ~500 000 - 2 000 000 MGA.
    /// </summary>
    public double PlafondCreditMicrofinance { get; set; } = 1_000_000;

    /// <summary>
    /// Montant maximal d'une tontine par tour (MGA).
    /// Tontines informelles : 50 000 - 300 000 MGA par tour.
    /// </summary>
    public double PlafondTontine { get; set; } = 200_000;

    /// <summary>
    /// Probabilité journalière d'octroi de crédit microfinance (si éligible).
    /// </summary>
    public double ProbabiliteOctroiMicrofinanceJour { get; set; } = 0.001;

    // ════════════════════════════════════════════════════════════════

    /// <summary>
    /// Active la saisonnalité agricole.
    /// Si true, la productivité agricole, les prix alimentaires, les exports
    /// et le tourisme varient selon le calendrier cultural malgache.
    /// Si false, tous les facteurs saisonniers restent à 1.0 (pas de variation).
    /// </summary>
    public bool SaisonnaliteActivee { get; set; } = true;

    /// <summary>
    /// Jour calendaire du début de la simulation (1 = 1er janvier, 182 = 1er juillet).
    /// Détermine la position dans le cycle saisonnier au jour 1 de la simulation.
    /// Par défaut : 1er janvier (début d'année / milieu de saison des pluies).
    /// </summary>
    public int JourCalendaireDebutSimulation { get; set; } = 1;

    /// <summary>
    /// Amplitude de la variation saisonnière de productivité agricole (0-1).
    /// 0 = pas de variation, 0.5 = ±50%, 1.0 = variation extrême.
    /// Calibrage Madagascar : rendements rizicoles varient de 40-60% entre saisons.
    /// </summary>
    public double AmplitudeProductiviteAgricole { get; set; } = 0.50;

    /// <summary>
    /// Amplitude de la variation saisonnière du prix du riz (0-1).
    /// INSTAT : prix du riz local varie de 25-40% entre soudure et post-récolte.
    /// </summary>
    public double AmplitudePrixRiz { get; set; } = 0.35;

    /// <summary>
    /// Amplitude de la variation des prix alimentaires généraux (0-1).
    /// Plus faible que le riz seul (panier diversifié).
    /// </summary>
    public double AmplitudePrixAlimentaire { get; set; } = 0.18;

    /// <summary>
    /// Amplitude de la variation touristique (0-1).
    /// Haute saison (jul-oct) vs basse saison cyclones (jan-mars).
    /// </summary>
    public double AmplitudeTourisme { get; set; } = 0.45;

    /// <summary>
    /// Amplitude de la variation de l'emploi agricole saisonnier (0-1).
    /// Semis/récolte = embauche, inter-saison = sous-emploi rural.
    /// </summary>
    public double AmplitudeEmploiAgricole { get; set; } = 0.25;

    // ════════════════════════════════════════════════════════════════

    // ════════════════════════════════════════════════════════════════
    // ░░░ INFLATION ENDOGÈNE ░░░
    // ════════════════════════════════════════════════════════════════

    /// <summary>
    /// Active le calcul de l'inflation endogène.
    /// Si true, TauxInflation est recalculé chaque jour à partir des fondamentaux
    /// (Phillips, cost-push, monétaire, anticipations).
    /// Si false, TauxInflation reste fixe (comportement d'origine).
    /// </summary>
    public bool InflationEndogeneActivee { get; set; } = true;

    /// <summary>
    /// NAIRU (Non-Accelerating Inflation Rate of Unemployment).
    /// Taux de chômage structurel en dessous duquel l'inflation accélère.
    /// Madagascar ≈ 0.15-0.20 (sous-emploi structurel élevé).
    /// </summary>
    public double NAIRU { get; set; } = 0.18;

    /// <summary>
    /// Coefficient de Phillips (β) : sensibilité de l'inflation à l'écart de chômage.
    /// Plus β est élevé, plus un chômage bas génère de l'inflation.
    /// Valeurs typiques : 0.01 (faible) à 0.10 (forte réactivité).
    /// </summary>
    public double CoefficientPhillips { get; set; } = 0.03;

    /// <summary>
    /// Poids des anticipations d'inflation dans la formule finale.
    /// Reflète l'inertie inflationniste (spirale prix-salaires).
    /// </summary>
    public double PoidsAnticipationsInflation { get; set; } = 0.40;

    /// <summary>
    /// Poids de la composante demand-pull (Phillips) dans la formule finale.
    /// </summary>
    public double PoidsDemandPullInflation { get; set; } = 0.20;

    /// <summary>
    /// Poids de la composante cost-push (carburant + imports) dans la formule finale.
    /// Élevé pour Madagascar (économie très dépendante des importations).
    /// </summary>
    public double PoidsCostPushInflation { get; set; } = 0.25;

    /// <summary>
    /// Poids de la composante monétaire (excès M3 vs PIB) dans la formule finale.
    /// </summary>
    public double PoidsMonetaireInflation { get; set; } = 0.15;

    /// <summary>
    /// Élasticité carburant → inflation (δ).
    /// Part du choc carburant transmise aux prix intérieurs.
    /// Madagascar ≈ 0.15-0.25 (forte transmission : routes, Jirama thermique).
    /// </summary>
    public double ElasticiteCarburantInflation { get; set; } = 0.20;

    /// <summary>
    /// Élasticité importations → inflation (ε).
    /// Part de la hausse du coût CIF transmise aux prix intérieurs.
    /// </summary>
    public double ElasticiteImportInflation { get; set; } = 0.12;

    /// <summary>
    /// Élasticité salaires → inflation (pass-through salarial, η).
    /// Part de la hausse des salaires transmise aux prix via les coûts de production.
    /// Essentiel pour le scénario "Hausse SMIG" : sans ce canal, une hausse du SMIG
    /// n'a aucun impact inflationniste, ce qui est irréaliste.
    /// Madagascar ≈ 0.10-0.20 (faible pouvoir de négociation salariale,
    /// secteur informel absorbe les chocs).
    /// </summary>
    public double ElasticiteSalairesInflation { get; set; } = 0.15;

    /// <summary>
    /// Coefficient monétaire (λ).
    /// Part de l'excès de croissance monétaire (ΔM3/M3 - ΔPIB/PIB) transmise aux prix.
    /// </summary>
    public double CoefficientMonetaire { get; set; } = 0.30;

    /// <summary>
    /// Vitesse d'adaptation des anticipations (α).
    /// 0 = ancrage parfait (BCM très crédible), 1 = adaptatif pur (spirale).
    /// Madagascar ≈ 0.50-0.70 (ancrage modéré, crédibilité BCM limitée).
    /// </summary>
    public double VitesseAdaptationAnticipations { get; set; } = 0.60;

    /// <summary>
    /// Inflation d'ancrage (cible implicite BCM).
    /// La composante anticipative converge vers cette valeur à long terme.
    /// BCM cible implicite ≈ 5-7%.
    /// </summary>
    public double InflationAncrage { get; set; } = 0.06;

    /// <summary>
    /// Taux de croissance annuel du PIB potentiel (tendanciel).
    /// Sert au calcul de l'output gap et à la composante monétaire.
    /// Madagascar ≈ 4-5% (tendance historique 2015-2024).
    /// </summary>
    public double CroissancePIBPotentielAnnuel { get; set; } = 0.045;

    // ════════════════════════════════════════════════════════════════

    // ════════════════════════════════════════════════════════════════
    // ░░░ RÈGLE DE TAYLOR (TAUX DIRECTEUR ENDOGÈNE BCM) ░░░
    // ════════════════════════════════════════════════════════════════

    /// <summary>
    /// Active le calcul endogène du taux directeur via la règle de Taylor.
    /// Si true, le taux directeur est recalculé chaque jour en fonction de
    /// l'inflation et de l'output gap. Si false, il reste fixe (TauxDirecteur).
    /// </summary>
    public bool TauxDirecteurEndogeneActive { get; set; } = true;

    /// <summary>
    /// Taux réel neutre r* (rendement d'équilibre, annuel).
    /// FMI estime ~2-3% pour Madagascar (pays à revenu faible).
    /// </summary>
    public double TauxReelNeutreTaylor { get; set; } = 0.02;

    /// <summary>
    /// Cible implicite d'inflation de la BCM (annuel).
    /// La BCM ne pratique pas de ciblage formel mais vise ~5-7%.
    /// </summary>
    public double InflationCibleTaylor { get; set; } = 0.06;

    /// <summary>
    /// Coefficient de réaction de Taylor à l'écart d'inflation α.
    /// Taylor standard = 0.5. Valeurs plus élevées = BCM plus agressive.
    /// </summary>
    public double CoefficientInflationTaylor { get; set; } = 0.50;

    /// <summary>
    /// Coefficient de réaction de Taylor à l'output gap β.
    /// Taylor standard = 0.5. BCM Madagascar probablement plus faible (~0.25)
    /// car la BCM est plus focalisée sur l'inflation que sur la croissance.
    /// </summary>
    public double CoefficientOutputGapTaylor { get; set; } = 0.25;

    /// <summary>
    /// Vitesse de lissage du taux directeur (0-1).
    /// 0.03 = très inertiel (convergence en ~3 mois), réaliste pour la BCM.
    /// 0.10 = réactif (convergence en ~1 mois).
    /// </summary>
    public double VitesseLissageTaylor { get; set; } = 0.03;

    // ════════════════════════════════════════════════════════════════

    // ════════════════════════════════════════════════════════════════
    // ░░░ DYNAMIQUE DU MARCHÉ DU TRAVAIL ░░░
    // ════════════════════════════════════════════════════════════════

    /// <summary>
    /// Active la dynamique d'embauche/licenciement des entreprises.
    /// Si true, les entreprises ajustent leurs effectifs en fonction de la demande
    /// et de leur trésorerie. Si false, l'emploi reste fixe (comportement d'origine).
    /// </summary>
    public bool DynamiqueEmploiActivee { get; set; } = true;

    /// <summary>
    /// Seuil d'utilisation de la capacité au-delà duquel l'entreprise envisage d'embaucher.
    /// 0.85 = si la demande dépasse 85% de la capacité de production.
    /// INSTAT : taux d'utilisation moyen ~70-80% dans le formel malgache.
    /// </summary>
    public double SeuilUtilisationEmbauche { get; set; } = 0.85;

    /// <summary>
    /// Nombre de jours consécutifs de demande excédentaire avant embauche (formel).
    /// Reflète le délai de recrutement (annonces, entretiens, formalités CNaPS).
    /// Pour l'informel, ce seuil est divisé par 2 (ajustement rapide).
    /// </summary>
    public int JoursAvantEmbauche { get; set; } = 7;

    /// <summary>
    /// Nombre de jours consécutifs de stress de trésorerie avant licenciement (formel).
    /// Code du travail malgache : préavis de 1-3 mois selon ancienneté.
    /// Pour l'informel, ce seuil est divisé par 3 (pas de code du travail).
    /// </summary>
    public int JoursAvantLicenciement { get; set; } = 15;

    /// <summary>
    /// Taux maximal d'embauche par jour (% des effectifs actuels).
    /// 0.02 = max 2% des effectifs embauchés par jour.
    /// Évite les embauches massives irréalistes.
    /// </summary>
    public double TauxEmbaucheMaxJour { get; set; } = 0.02;

    /// <summary>
    /// Taux maximal de licenciement par jour (% des effectifs actuels).
    /// 0.03 = max 3% des effectifs licenciés par jour.
    /// Légèrement plus élevé que l'embauche (plus facile de licencier que d'embaucher).
    /// </summary>
    public double TauxLicenciementMaxJour { get; set; } = 0.03;

    // ════════════════════════════════════════════════════════════════

    // ════════════════════════════════════════════════════════════════
    // ░░░ CHOCS CLIMATIQUES STOCHASTIQUES (CYCLONES) ░░░
    // ════════════════════════════════════════════════════════════════

    /// <summary>
    /// Active les chocs cycloniques stochastiques.
    /// Si true, des cyclones peuvent survenir aléatoirement pendant la saison
    /// cyclonique (nov-avr) avec des impacts sur la production, les prix et
    /// une phase de reconstruction BTP post-cyclone.
    /// </summary>
    public bool ChocsCycloniquesActives { get; set; } = true;

    /// <summary>
    /// Probabilité journalière de cyclone pendant la saison cyclonique (nov-avr).
    /// 0.003 = ~0.3%/jour → ~54% de chance d'au moins 1 cyclone sur 6 mois.
    /// Calibré BNGRC : 1.5 cyclones/an en moyenne touchant Madagascar.
    /// </summary>
    public double ProbabiliteCycloneJourSaison { get; set; } = 0.003;

    /// <summary>
    /// Probabilité journalière de cyclone hors saison (mai-oct).
    /// Résiduel (~0.02%/jour) pour capturer les événements exceptionnels.
    /// </summary>
    public double ProbabiliteCycloneJourHorsSaison { get; set; } = 0.0002;

    /// <summary>
    /// Part du budget de reconstruction routée vers le BTP (maçons, charpentiers).
    /// Calibrage terrain : ~55% des dépenses de reconstruction vont aux artisans BTP.
    /// </summary>
    public double PartReconstructionBTP { get; set; } = 0.55;

    /// <summary>
    /// Part du budget de reconstruction routée vers la quincaillerie (tôle, briques, ciment).
    /// Calibrage terrain : ~30% des dépenses de reconstruction sont des matériaux.
    /// </summary>
    public double PartReconstructionQuincaillerie { get; set; } = 0.30;

    /// <summary>
    /// Part du budget de reconstruction routée vers le transport informel de matériaux.
    /// Charrettes, camionnettes, pousse-pousse pour acheminer tôle et briques.
    /// </summary>
    public double PartReconstructionTransportInformel { get; set; } = 0.15;

    // ════════════════════════════════════════════════════════════════

    // ════════════════════════════════════════════════════════════════
    // ░░░ VALIDATION MACRO AUTOMATIQUE ░░░
    // ════════════════════════════════════════════════════════════════

    /// <summary>
    /// Active la validation macro automatique à la fin de la simulation.
    /// Compare les résultats aux données de référence (INSTAT/BCM/FMI)
    /// et génère un rapport de diagnostic avec scoring.
    /// </summary>
    public bool ValidationMacroActivee { get; set; } = true;

    /// <summary>
    /// Données macroéconomiques de référence pour la validation.
    /// Par défaut : Madagascar 2024 (INSTAT/BCM/FMI).
    /// Modifiables pour tester d'autres années ou scénarios contrefactuels.
    /// </summary>
    public Simulation.Module.Models.MacroReferenceData DonneesReference { get; set; } = new();

    // ════════════════════════════════════════════════════════════════

    // ════════════════════════════════════════════════════════════════
    // ░░░ RECALIBRATION MENSUELLE — GARDE-FOUS MACRO ░░░
    // ════════════════════════════════════════════════════════════════

    /// <summary>
    /// Activer la recalibration mensuelle automatique.
    /// Si activé, à chaque fin de mois (jour 30, 60, 90...), le simulateur
    /// vérifie que les grandeurs simulées restent dans une bande de tolérance
    /// autour des données observées et ne corrige QUE si la dérive est excessive.
    /// Les données observées sont des garde-fous, pas des cibles à atteindre.
    /// </summary>
    public bool RecalibrationMensuelleActivee { get; set; } = false;

    /// <summary>
    /// Données de référence mensuelles (INSTAT/BCM/DGI).
    /// Servent de point d'ancrage pour détecter les dérives excessives.
    /// Ce ne sont PAS des cibles : la simulation peut diverger si le scénario le justifie.
    /// </summary>
    public List<MonthlyCalibrationTarget> CiblesMensuelles { get; set; } = [];

    /// <summary>
    /// Intensité de correction quand la dérive dépasse le seuil (0.0–1.0).
    /// Ne s'applique QUE sur l'excès au-delà de la bande de tolérance.
    /// 0.3 = correction douce, 1.0 = ramène au bord de la bande.
    /// Valeur recommandée : 0.5.
    /// </summary>
    public double VitesseConvergenceRecalibration { get; set; } = 0.5;

    /// <summary>
    /// Seuil de tolérance de la bande garde-fou (0.0–1.0).
    /// La recalibration n'intervient QUE si l'écart relatif dépasse ce seuil.
    /// 0.30 = ±30% autour de la valeur observée → large latitude pour le scénario.
    /// 0.10 = ±10% → très strict, le modèle sera très contraint par les données.
    /// Valeur recommandée : 0.30 (laisse le scénario s'exprimer).
    /// </summary>
    public double SeuilToleranceRecalibration { get; set; } = 0.30;

    // ════════════════════════════════════════════════════════════════

    // ════════════════════════════════════════════════════════════════
    // ░░░ COMPORTEMENTS PAR CLASSE (chargés depuis BD) ░░░
    // ════════════════════════════════════════════════════════════════

    /// <summary>
    /// Comportements par classe socio-économique chargés depuis la BD.
    /// Si vide, les modules utilisent leurs valeurs hardcodées par défaut.
    /// </summary>
    public List<ComportementClasseConfig> ComportementsParClasse { get; set; } = [];

    // ════════════════════════════════════════════════════════════════
    // ░░░ TRANSPORT (parts formel/informel, configurables) ░░░
    // ════════════════════════════════════════════════════════════════

    // Chargés depuis sim.ParamTransport via ScenarioConfigLoader — pas de default C# intentionnel.
    public double PartInformelTransportPublic { get; set; }
    public double PartFormelCarburant { get; set; }
    public double PartInformelEntretien { get; set; }
    public double EntretienVoitureJour { get; set; }
    public double EntretienFractionRevenuVoiture { get; set; }

    // ════════════════════════════════════════════════════════════════
    // ░░░ PROBABILITÉS INFORMEL PAR SECTEUR ░░░
    // ════════════════════════════════════════════════════════════════

    /// <summary>
    /// Probabilité qu'une entreprise d'un secteur donné soit dans le secteur informel.
    /// Chargé depuis sim.ParamSecteurActivite via ScenarioConfigLoader.
    /// Vide par défaut — toujours alimenté depuis la BD avant simulation.
    /// </summary>
    public Dictionary<ESecteurActivite, double> ProbabiliteInformelParSecteur { get; set; } = new();

    /// <summary>
    /// Marge bénéficiaire par secteur (override du paramètre global MargeBeneficiaireEntreprise).
    /// Chargé depuis sim.ParamSecteurActivite via ScenarioConfigLoader.
    /// </summary>
    public Dictionary<ESecteurActivite, double> MargeBeneficiaireParSecteur { get; set; } = new();

    // ════════════════════════════════════════════════════════════════

    // Chargé depuis sim.ParamSecteurActivite via ScenarioConfigLoader.
    public Dictionary<ESecteurActivite, double> TresorerieInitialeParSecteur { get; set; } = new();

    public Dictionary<ECategorieExport, double> FOBJourParCategorie { get; set; } = new();
    public Dictionary<ECategorieImport, double> CIFJourParCategorie { get; set; } = new();

    public bool UseExportCalibresDirectement { get; set; } = true;
    public string SourceCalibration { get; set; } = "Moyenne INSTAT Tableau 32 (oct 2023 – sept 2025)";
    public Dictionary<ECategorieExport, double> FOBCalibresJour { get; set; } = new();

    public bool UseImportCalibresDirectement { get; set; } = true;
    public string SourceCalibrationImports { get; set; } = "Moyenne INSTAT Tableau 33 (juil 2023 – juin 2025)";
    public Dictionary<ECategorieImport, double> CIFCalibresJour { get; set; } = new();

    public static ScenarioConfig BaseMadagascar() => new()
    {
        Implications = new()
        {
            ChangementsClés = ["Paramètres calibrés INSTAT/BCM/BFM sept. 2025", "85% secteur informel", "Saisonnalité agricole active"],
            CanauxTransmission = ["Consommation ménages (C)", "Dépenses publiques (G)", "Commerce extérieur (X−M)"],
            RisquesAttendus = ["Référence : aucun choc appliqué", "Dépréciation MGA structurelle 5%/an"],
            HorizonRecommande = "1 an",
            Categorie = "baseline"
        }
    };

    public static List<ScenarioConfig> TousLesScenarios()
    {
        return new List<ScenarioConfig>
        {
            // ══════════════════════════════════════════════════════════════
            // 0. BASELINE — Madagascar actuel (sept. 2025)
            // ══════════════════════════════════════════════════════════════
            new()
            {
                Name = "🇲🇬 Baseline — Madagascar actuel (2025)",
                Description = "Calibrage INSTAT/BCM/BFM sept. 2025. Référence pour mesurer l'impact de toute politique.",
                Implications = new()
                {
                    ChangementsClés = ["Paramètres calibrés sur données réelles sept. 2025", "M3 ~35 000 Mds MGA, PIB ~55 000 Mds MGA/an"],
                    CanauxTransmission = ["Tous les mécanismes actifs (saisonnalité, inflation endogène, change)"],
                    RisquesAttendus = ["Dépréciation MGA 5%/an (structurelle)", "Cyclone potentiel nov–avr (~54% sur 6 mois)"],
                    HorizonRecommande = "1 an",
                    Categorie = "baseline"
                }
            },

            // ══════════════════════════════════════════════════════════════
            // POLITIQUE SOCIALE
            // ══════════════════════════════════════════════════════════════

            // Q: Si j'augmente l'appui aux ménages pauvres (+50% transferts sociaux), quels impacts macro ?
            new()
            {
                Name = "🤝 Appui ménages pauvres (+50% transferts sociaux)",
                Description = "Transferts sociaux +50%. Impact : consommation ↑, PIB demand-pull ↑, inflation alimentaire ?, recettes TVA ?, secteur informel ?",
                DepensesPubliquesJour = 4_827_000_000, // +50% du montant dédié transferts
                AideInternationaleJour = 5_556_000_000, // +50% aide (financement externe)
                Implications = new()
                {
                    ChangementsClés = ["Dépenses publiques +50%", "Aide internationale +50%", "Revenus ménages vulnérables ↑"],
                    CanauxTransmission = ["Consommation C ↑ (propension 92% classe subsistance)", "Demande alimentaire informelle ↑", "Recettes TVA ↑ par rebond"],
                    RisquesAttendus = ["Inflation alimentaire si offre inélastique", "Déficit budgétaire si non financé par aide", "Attractivité importations alimentaires"],
                    HorizonRecommande = "3 mois",
                    Categorie = "social"
                }
            },

            // Q: Si je double les transferts avec financement par dette ?
            new()
            {
                Name = "🤝🤝 Appui ménages pauvres massif (×2 transferts, dette)",
                Description = "Transferts sociaux ×2, financés par dette publique. Impact : demande ↑↑, inflation ?, dette ?, effet multiplicateur ?",
                DepensesPubliquesJour = 6_436_000_000, // ×2
                Implications = new()
                {
                    ChangementsClés = ["Dépenses publiques ×2", "Financement par dette (effet d'éviction)"],
                    CanauxTransmission = ["Consommation C ↑↑", "M3 ↑ (financement monétaire)", "Taux d'intérêt ↑ (pression sur dette)"],
                    RisquesAttendus = ["Inflation forte si M3 non stérilisé", "Dette/PIB ↑ rapidement", "Dépréciation MGA accélérée"],
                    HorizonRecommande = "1 an",
                    Categorie = "social"
                }
            },

            // Q: Si j'augmente le salaire minimum (SMIG) de 200k à 300k MGA/mois ?
            new()
            {
                Name = "💰 Hausse SMIG (200k → 300k MGA/mois)",
                Description = "Salaire plancher +50%. Impact : pouvoir d'achat ↑, charges entreprises ↑, emploi formel ?, inflation salaires ?",
                SalairePlancher = 75_000, // 300k/mois ÷ ~4 semaines ≈ plafond journalier ajusté
                SalaireMedian = 200_000, // tiré vers le haut
                Implications = new()
                {
                    ChangementsClés = ["SMIG 200k → 300k MGA/mois (+50%)", "Salaire médian relevé"],
                    CanauxTransmission = ["Pouvoir d'achat classes InformelBas/FormelBas ↑", "Charges CNaPS entreprises ↑ (18% sur salaires)", "Pass-through salarial → inflation (η=0.15)"],
                    RisquesAttendus = ["Hausse coûts de production → inflation ~7.5%", "Licenciements informels (ajustement rapide)", "Substitution capital/travail dans le formel"],
                    HorizonRecommande = "1 an",
                    Categorie = "social"
                }
            },

            // ══════════════════════════════════════════════════════════════
            // POLITIQUE DE CRÉDIT & SECTEUR PRIVÉ
            // ══════════════════════════════════════════════════════════════

            // Q: Si j'accorde des facilités de crédit aux TPE/PME (CA > 10M MGA), quels impacts macro ?
            new()
            {
                Name = "🏦 Facilités crédit TPE/PME (crédit +30%, taux -3pts)",
                Description = "Crédit +30% aux entreprises, taux crédit -3pts. Impact : investissement ↑, emploi ↑, M3 ↑, NPL ?, inflation ?",
                CroissanceCreditJour = 0.00053,
                TauxInteretCredits = 0.13,
                PartCreditEntreprises = 0.85,
                Implications = new()
                {
                    ChangementsClés = ["Crédit bancaire +30%", "Taux crédit 16% → 13%", "75%→85% du crédit vers entreprises"],
                    CanauxTransmission = ["Investissement FBCF privé ↑", "Emploi formel ↑ (embauche facilitée)", "M3 ↑ via multiplicateur bancaire"],
                    RisquesAttendus = ["NPL potentiel si sélection adverse", "Pression inflationniste via M3", "Effet limité si demande insuffisante"],
                    HorizonRecommande = "1 an",
                    Categorie = "monetaire"
                }
            },

            // Q: Politique monétaire expansive — taux directeur bas
            new()
            {
                Name = "🏦 Monnaie facile (taux directeur 3%, crédit ↑)",
                Description = "BCM baisse taux à 3%. Impact : crédit ↑, investissement ↑, consommation ↑, inflation ?, taux change ?",
                TauxDirecteur = 0.03,
                CroissanceCreditJour = 0.00060,
                TauxInteretCredits = 0.12,
                TauxInteretDepots = 0.03,
                Implications = new()
                {
                    ChangementsClés = ["Taux directeur BCM 9% → 3%", "Crédit +46%/an", "Taux débiteur 16% → 12%"],
                    CanauxTransmission = ["Crédit entreprises ↑↑", "Investissement et consommation ↑", "MGA sous pression (carry trade sortant)"],
                    RisquesAttendus = ["Inflation si output gap positif", "Dépréciation MGA accélérée", "Bulles actifs immobiliers"],
                    HorizonRecommande = "1 an",
                    Categorie = "monetaire"
                }
            },

            // Q: Resserrement monétaire — taux directeur haut
            new()
            {
                Name = "🏦 Resserrement monétaire (taux directeur 15%)",
                Description = "BCM monte taux à 15% pour combattre inflation. Impact : crédit ↓, investissement ↓, inflation ↓, emploi ?",
                TauxDirecteur = 0.15,
                CroissanceCreditJour = 0.00020,
                TauxInteretCredits = 0.22,
                TauxInteretDepots = 0.07,
                Implications = new()
                {
                    ChangementsClés = ["Taux directeur 9% → 15%", "Crédit divisé par 2", "Taux débiteur 22%"],
                    CanauxTransmission = ["Crédit se contracte", "Investissement ↓", "MGA s'apprécie (afflux carry)"],
                    RisquesAttendus = ["Récession si surchauffe contenue", "NPL ↑ (entreprises surendettées)", "Chômage ↑ à court terme"],
                    HorizonRecommande = "1 an",
                    Categorie = "monetaire"
                }
            },

            // ══════════════════════════════════════════════════════════════
            // FORMALISATION DU SECTEUR INFORMEL
            // ══════════════════════════════════════════════════════════════

            // Q: Si je formalise 10% du secteur informel, quels impacts sur les recettes fiscales ?
            new()
            {
                Name = "📋 Formalisation secteur informel (-10pts informel)",
                Description = "Part informel 85%→75%. Impact : recettes fiscales ↑ (IR, IS, TVA), emploi formel ↑, productivité ↑, coût entreprises ?",
                PartSecteurInformel = 0.75,
                TauxCotisationsPatronalesCNaPS = 0.18,
                Implications = new()
                {
                    ChangementsClés = ["Secteur informel 85% → 75%", "Base fiscale élargie (IS + TVA)"],
                    CanauxTransmission = ["Recettes IS/TVA ↑ (~+15%)", "Cotisations CNaPS ↑", "Productivité travail ↑ (formel plus productif)"],
                    RisquesAttendus = ["Résistance des entreprises informelles", "Coûts de transition (comptabilité, CNaPS)", "Risque de fuite vers l'économie grise"],
                    HorizonRecommande = "1 an",
                    Categorie = "fiscal"
                }
            },

            // Q: Si je formalise 20% du secteur informel ?
            new()
            {
                Name = "📋📋 Formalisation massive (-20pts informel)",
                Description = "Part informel 85%→65%. Impact : recettes fiscales ↑↑, base TVA élargie, productivité ↑, mais coûts sociaux transition ?",
                PartSecteurInformel = 0.65,
                Implications = new()
                {
                    ChangementsClés = ["Secteur informel 85% → 65%", "Base fiscale fortement élargie"],
                    CanauxTransmission = ["Recettes IS/TVA ↑↑ (~+30%)", "M3 ↑ (plus de transactions formelles)", "PIB comptabilisé ↑ (moins d'économie souterraine)"],
                    RisquesAttendus = ["Choc de transition élevé (horizon 3-5 ans)", "Résistance politique forte", "Risque de destruction emploi informel"],
                    HorizonRecommande = "5 ans",
                    Categorie = "fiscal"
                }
            },

            // ══════════════════════════════════════════════════════════════
            // POLITIQUE FISCALE
            // ══════════════════════════════════════════════════════════════

            // Q: Si j'augmente la TVA de 20% à 22%, quels impacts ?
            new()
            {
                Name = "📈 Hausse TVA (20% → 22%)",
                Description = "TVA +2pts. Impact : recettes fiscales ↑, prix ↑, consommation ↓, secteur informel renforcé ?",
                TauxTVA = 0.22,
                Implications = new()
                {
                    ChangementsClés = ["TVA 20% → 22% (+2 pts)"],
                    CanauxTransmission = ["Prix biens formels ↑ 2%", "Recettes TVA ↑ (~+10%)", "Substitution vers informel (pas de TVA)"],
                    RisquesAttendus = ["Consommation formelle ↓", "Renforcement secteur informel", "Effet régressif (ménages pauvres plus impactés)"],
                    HorizonRecommande = "3 mois",
                    Categorie = "fiscal"
                }
            },

            // Q: Si je baisse la TVA à 18% pour stimuler la consommation ?
            new()
            {
                Name = "📉 Baisse TVA (20% → 18%)",
                Description = "TVA -2pts. Impact : prix ↓, consommation ↑, recettes fiscales ↓, déficit ?, effet Laffer ?",
                TauxTVA = 0.18,
                Implications = new()
                {
                    ChangementsClés = ["TVA 20% → 18% (−2 pts)"],
                    CanauxTransmission = ["Prix biens formels ↓ 2%", "Consommation formelle ↑", "Effet Laffer potentiel (volume ↑ compense taux ↓)"],
                    RisquesAttendus = ["Recettes TVA ↓ à court terme", "Déficit budgétaire si pas compensé", "Réduction incitation à l'informalité"],
                    HorizonRecommande = "3 mois",
                    Categorie = "fiscal"
                }
            },

            // Q: Si j'augmente l'IS sur les grandes entreprises ?
            new()
            {
                Name = "🏢 Hausse IS (20% → 25%)",
                Description = "IS +5pts. Impact : recettes IS ↑, investissement privé ↓, compétitivité ?, délocalisations ?",
                TauxIS = 0.25,
                Implications = new()
                {
                    ChangementsClés = ["IS 20% → 25% (+5 pts)"],
                    CanauxTransmission = ["Recettes IS ↑ sur entreprises formelles", "Profit net entreprises ↓ → FBCF ↓", "Trésorerie entreprises ↓"],
                    RisquesAttendus = ["Investissement privé ↓", "Fuite vers secteur informel", "Compétitivité exportateurs ↓"],
                    HorizonRecommande = "1 an",
                    Categorie = "fiscal"
                }
            },

            // Q: Si je réduis les droits de douane pour baisser les prix ?
            new()
            {
                Name = "🚢 Réduction droits douane (12% → 8%)",
                Description = "Droits douane -4pts. Impact : prix imports ↓, inflation ↓, recettes douanières ↓, déficit commercial ?",
                TauxDroitsDouane = 0.08,
                TauxAccise = 0.08,
                Implications = new()
                {
                    ChangementsClés = ["Droits douane 12% → 8%", "Accises 10% → 8%"],
                    CanauxTransmission = ["Prix imports ↓ → inflation importée ↓", "Recettes douanières ↓", "Volume imports ↑ (biens moins chers)"],
                    RisquesAttendus = ["Déficit commercial ↑", "Industrie locale sous pression concurrentielle", "MGA sous pression (imports ↑)"],
                    HorizonRecommande = "3 mois",
                    Categorie = "fiscal"
                }
            },

            // ══════════════════════════════════════════════════════════════
            // POLITIQUE BUDGÉTAIRE & INVESTISSEMENT PUBLIC
            // ══════════════════════════════════════════════════════════════

            // Q: Si j'augmente l'investissement public de 20% ?
            new()
            {
                Name = "🏗️ Stimulus investissement public (+20% FBCF)",
                Description = "Dépenses capital +20%. Impact : FBCF ↑, emploi BTP ↑, PIB ↑, dette ?, effet d'éviction ?",
                DepensesCapitalJour = 16_231_200_000,
                DepensesPubliquesJour = 3_861_600_000,
                Implications = new()
                {
                    ChangementsClés = ["FBCF publique +20% (13.5 → 16.2 Mds/jour)", "Dépenses publiques +20%"],
                    CanauxTransmission = ["Emploi BTP ↑ (multiplicateur ~1.5)", "PIB demande ↑", "Commandes secteur construction ↑"],
                    RisquesAttendus = ["Dette publique ↑ si non financé", "Effet d'éviction crédit privé", "Inflation BTP si capacité saturée"],
                    HorizonRecommande = "1 an",
                    Categorie = "budgetaire"
                }
            },

            // Q: Austérité budgétaire ?
            new()
            {
                Name = "⚠️ Austérité budgétaire (-20% dépenses État)",
                Description = "Dépenses publiques -20%. Impact : déficit ↓, dette ↓, mais emploi public ↓, demande ↓, PIB ?",
                DepensesPubliquesJour = 2_574_400_000,
                DepensesCapitalJour = 10_820_800_000,
                NombreFonctionnaires = 280_000,
                Implications = new()
                {
                    ChangementsClés = ["Dépenses publiques −20%", "FBCF publique −20%", "Fonctionnaires 350k → 280k"],
                    CanauxTransmission = ["Demande agrégée ↓ (G ↓)", "Salaires fonctionnaires ↓", "Déficit budgétaire ↓"],
                    RisquesAttendus = ["Récession à court terme", "Chômage fonctionnaires", "PIB ↓ si multiplicateur > 1"],
                    HorizonRecommande = "1 an",
                    Categorie = "budgetaire"
                }
            },

            // ══════════════════════════════════════════════════════════════
            // SUBVENTIONS SECTORIELLES
            // ══════════════════════════════════════════════════════════════

            // Q: Si je subventionne la Jirama (énergie) davantage ?
            new()
            {
                Name = "⚡ Subvention énergie (Jirama +50%)",
                Description = "Subvention Jirama +50%. Impact : facture ménages ↓, compétitivité entreprises ↑, déficit ↑, dette ?",
                SubventionJiramaJour = 2_055_000_000,
                Implications = new()
                {
                    ChangementsClés = ["Subvention Jirama 1.37 → 2.05 Mds MGA/jour (+50%)"],
                    CanauxTransmission = ["Facture électricité ménages ↓", "Coûts prod. entreprises ↓ (compétitivité ↑)", "Déficit État ↑"],
                    RisquesAttendus = ["Déficit budgétaire ↑", "Aucune incitation à l'efficacité Jirama", "Dépendance subvention long terme"],
                    HorizonRecommande = "1 an",
                    Categorie = "sectoriel"
                }
            },

            // Q: Si je supprime la subvention Jirama ?
            new()
            {
                Name = "⚡ Fin subvention Jirama (0 MGA)",
                Description = "Suppression subvention énergie. Impact : facture ménages ↑↑, compétitivité ↓, déficit ↓, pauvreté énergétique ?",
                SubventionJiramaJour = 0,
                Implications = new()
                {
                    ChangementsClés = ["Subvention Jirama supprimée (0 MGA)"],
                    CanauxTransmission = ["Tarif électricité ménages ↑↑", "Coûts prod. entreprises ↑", "Déficit État ↓ (500 Mds MGA/an économisés)"],
                    RisquesAttendus = ["Pauvreté énergétique (30% accès élec.)", "Inflation coûts de production", "Choc régressif sur ménages ruraux"],
                    HorizonRecommande = "3 mois",
                    Categorie = "sectoriel"
                }
            },

            // Q: Si j'investis dans l'agriculture (+30% productivité) ?
            new()
            {
                Name = "🌾 Investissement agricole (+30% productivité)",
                Description = "Productivité agricole +30%. Impact : prix alimentaires ↓, sécurité alimentaire ↑, exports ↑, revenu rural ↑",
                ProductiviteParEmploye = 19_500,
                PartEntreprisesAgricoles = 0.35,
                PrixRizLocalKg = 2_000,
                Implications = new()
                {
                    ChangementsClés = ["Productivité/employé 15k → 19.5k MGA/jour (+30%)", "Secteur agricole 30% → 35% entreprises", "Prix riz local 2 400 → 2 000 MGA/kg"],
                    CanauxTransmission = ["Prix alimentaires ↓ (inflation ↓)", "Revenus ruraux ↑", "Exports agricoles ↑ (vanille, girofle, riz)"],
                    RisquesAttendus = ["Horizon long terme (3-5 ans pour effets)", "Nécessite infrastructures eau/routes", "Saisonnalité amplifie les gains"],
                    HorizonRecommande = "5 ans",
                    Categorie = "sectoriel"
                }
            },

            // Q: Si je développe le tourisme (+50% capacité) ?
            new()
            {
                Name = "🏖️ Boost tourisme (+50% capacité hôtelière)",
                Description = "Secteur tourisme +50%. Impact : devises ↑, emploi services ↑, taux change ↓, revenus régions ?",
                PartEntreprisesHotellerieTourisme = 0.045,
                Implications = new()
                {
                    ChangementsClés = ["Hôtellerie/tourisme 3% → 4.5% des entreprises"],
                    CanauxTransmission = ["Devises USD/EUR ↑ (→ MGA s'apprécie)", "Emploi services ↑", "Recettes fiscales (IS, TVA hôtellerie) ↑"],
                    RisquesAttendus = ["Saisonnalité forte (juil–oct pic)", "Dutch disease léger (MGA fort = exports moins compétitifs)", "Dépendance aux arrivées internationales"],
                    HorizonRecommande = "1 an",
                    Categorie = "sectoriel"
                }
            },

            // ══════════════════════════════════════════════════════════════
            // CHOCS EXOGÈNES
            // ══════════════════════════════════════════════════════════════

            // Q: Impact d'un choc carburant ?
            new()
            {
                Name = "⛽ Choc carburant (+30%)",
                Description = "Carburant 5500→7150 MGA/L. Impact : inflation cost-push ↑, transport ↑, prix alimentaires ↑, pouvoir d'achat ↓",
                PrixCarburantLitre = 7_150,
                ElasticitePrixParCarburant = 0.70,
                Implications = new()
                {
                    ChangementsClés = ["Prix carburant 5 500 → 7 150 MGA/L (+30%)"],
                    CanauxTransmission = ["Coûts transport ↑ 30%", "Prix marchandises locales ↑ ~21% (ε=0.70)", "Inflation alimentaire ↑ (panier pauvres)"],
                    RisquesAttendus = ["Pouvoir d'achat ménages pauvres −15% à −25%", "Récession si choc durable", "MGA sous pression (imports carburant +)"],
                    HorizonRecommande = "3 mois",
                    Categorie = "choc"
                }
            },

            // Q: Impact d'un choc importations ?
            new()
            {
                Name = "🌍 Choc prix mondiaux imports (+25%)",
                Description = "Prix CIF +25%. Impact : inflation importée ↑, balance commerciale ↓, réserves ↓, taux change ?",
                PrixRizImporteKg = 3_500,
                PrixCarburantLitre = 6_875,
                TauxDroitsDouane = 0.15,
                Implications = new()
                {
                    ChangementsClés = ["Prix riz importé 2 800 → 3 500 MGA/kg (+25%)", "Carburant +25%", "DD 12% → 15%"],
                    CanauxTransmission = ["Inflation importée (ζ=0.30 pass-through)", "Balance commerciale ↓", "Réserves BCM ↓ (défense MGA)"],
                    RisquesAttendus = ["Dépréciation MGA si réserves faibles", "Inflation alimentaire importée", "Sécurité alimentaire compromise (riz)"],
                    HorizonRecommande = "3 mois",
                    Categorie = "choc"
                }
            },

            // Q: Catastrophe climatique ?
            new()
            {
                Name = "🌪️ Cyclone majeur (récoltes -40%)",
                Description = "Cyclone sévère détruit 40% récoltes. Impact : prix alimentaires ↑↑, pauvreté ↑, reconstruction BTP ↑",
                PartEntreprisesAgricoles = 0.18,
                PrixRizLocalKg = 3_500,
                PrixRizImporteKg = 4_200,
                Implications = new()
                {
                    ChangementsClés = ["Agriculture 30% → 18% (−40% récoltes)", "Prix riz local +46%", "Prix riz importé +50%"],
                    CanauxTransmission = ["Sécurité alimentaire ↓↓", "Reconstruction BTP ↑ (phase rebond)", "Imports alimentaires ↑ (pression MGA)"],
                    RisquesAttendus = ["Pauvreté rurale ↑↑", "Inflation alimentaire +30 à +50%", "PIB agricole −3 à −5% du PIB total"],
                    HorizonRecommande = "1 an",
                    Categorie = "choc"
                }
            },

            // Q: Boom export (prix matières premières mondiaux) ?
            new()
            {
                Name = "📦 Boom export (vanille, nickel +30%)",
                Description = "Exports +30%. Impact : devises ↑, taux change ↓, investissement ↑, Dutch disease ?",
                UseExportCalibresDirectement = true,
                PartEntreprisesConstruction = 0.08,
                Implications = new()
                {
                    ChangementsClés = ["Exports +30% (vanille, nickel, girofle)", "Secteur construction ↑"],
                    CanauxTransmission = ["Devises ↑ → M3 ↑", "MGA s'apprécie", "Investissement minier ↑"],
                    RisquesAttendus = ["Dutch disease (MGA fort → autres exports non-compétitifs)", "Dépendance aux cours mondiaux", "Enclave économique si peu de linkages locaux"],
                    HorizonRecommande = "1 an",
                    Categorie = "choc"
                }
            },

            // ══════════════════════════════════════════════════════════════
            // SCÉNARIOS COMPOSITES (STRATÉGIE COMPLÈTE)
            // ══════════════════════════════════════════════════════════════

            // Q: Scénario de croissance inclusive (package complet) ?
            new()
            {
                Name = "🌟 Stratégie croissance inclusive 2030",
                Description = "Package : formalisation + crédit PME + investissement public + productivité. Impact global sur PIB, emploi, inégalités.",
                PartSecteurInformel = 0.70,
                CroissanceCreditJour = 0.00053,
                TauxInteretCredits = 0.13,
                DepensesCapitalJour = 16_231_200_000,
                ProductiviteParEmploye = 19_500,
                SalaireMedian = 220_000,
                SalaireSigma = 0.70,
                TauxIS = 0.18,
                Implications = new()
                {
                    ChangementsClés = ["Informel 85% → 70%", "Crédit +30%, taux 13%", "FBCF +20%", "Productivité +30%", "IS réduit 20% → 18%"],
                    CanauxTransmission = ["Investissement privé + public ↑↑", "Emploi formel ↑", "Productivité totale des facteurs ↑", "Recettes fiscales ↑ (base élargie)"],
                    RisquesAttendus = ["Complexité de mise en œuvre simultanée", "Inflation transitoire possible", "Horizon 5 ans pour effets complets"],
                    HorizonRecommande = "5 ans",
                    Categorie = "composite"
                }
            },

            // Q: Scénario de crise (combinaison de chocs) ?
            new()
            {
                Name = "🔥 Crise combinée (carburant + climat + change)",
                Description = "Triple choc : carburant +50%, cyclone, dépréciation MGA. Stress-test de la résilience macro.",
                PrixCarburantLitre = 8_250,
                PartEntreprisesAgricoles = 0.18,
                PrixRizLocalKg = 3_500,
                TauxInflation = 0.15,
                TauxDirecteur = 0.12,
                Implications = new()
                {
                    ChangementsClés = ["Carburant +50% (8 250 MGA/L)", "Cyclone → agriculture −40%", "Inflation 15%", "Taux directeur 12%"],
                    CanauxTransmission = ["Inflation cost-push ↑↑", "PIB agricole effondré", "Taux d'intérêt restrictif", "MGA dépréciation accélérée"],
                    RisquesAttendus = ["Récession sévère (PIB −3% à −8%)", "Pauvreté extrême ↑↑", "Fuite de capitaux", "Crise de la balance des paiements"],
                    HorizonRecommande = "1 an",
                    Categorie = "choc"
                }
            },
        };
    }
}






