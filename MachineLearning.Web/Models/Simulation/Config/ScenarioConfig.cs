using MachineLearning.Web.Models.Agents.Companies;

namespace MachineLearning.Web.Models.Simulation.Config;

/// <summary>
/// Configuration d'un scénario de simulation économique.
/// </summary>
public class ScenarioConfig
{
    public string Name { get; set; } = "Scénario de base";
    public string Description { get; set; } = "Paramètres macroéconomiques de base de Madagascar";

    /// <summary>
    /// Population de référence (nombre réel de ménages à Madagascar, ~6 millions).
    /// Tous les paramètres macro absolus (aide, subventions, fonctionnaires, dépenses publiques)
    /// sont calibrés à cette échelle et seront mis à l'échelle proportionnellement au
    /// NombreMenages configuré dans la simulation.
    /// </summary>
    public static readonly double NombreMenagesReference = 6_000_000;

    // --- Horizons temporels (en jours simulés, 1 seconde = 1 jour) ---
    public static readonly int CourtTerme = 90;       // 3 mois
    public static readonly int MoyenTerme = 365;      // 1 an
    public static readonly int LongTerme = 1825;      // 5 ans

    /// <summary>Durée de la simulation en jours</summary>
    public int DureeJours { get; set; } = 365;

    // --- Nombre d'agents ---
    public int NombreMenages { get; set; } = 100000;
    public int NombreEntreprises { get; set; } = 50000;

    // --- Paramètres fiscaux (État) ---
    /// <summary>IRSA : barème progressif par défaut (pas de taux flat configurable)</summary>
    public double TauxIS { get; set; } = 0.20;
    public double TauxTVA { get; set; } = 0.20;

    // --- Politique monétaire ---
    public double TauxDirecteur { get; set; } = 0.09;
    public double TauxInflation { get; set; } = 0.08;

    // --- Transport & carburant ---
    /// <summary>Prix du litre de carburant en MGA (~5 500 MGA à Madagascar en 2024)</summary>
    public double PrixCarburantLitre { get; set; } = 5_500;

    // --- Distribution salariale (loi log-normale) ---
    /// <summary>Salaire médian mensuel en MGA (~170 000 pour Madagascar)</summary>
    public double SalaireMedian { get; set; } = 170_000;

    /// <summary>
    /// Sigma de la log-normale (dispersion/inégalité).
    /// σ ≈ 0.85 → Gini ~0.43 (Madagascar actuel)
    /// </summary>
    public double SalaireSigma { get; set; } = 0.85;

    /// <summary>Salaire plancher (minimum vital, secteur informel)</summary>
    public double SalairePlancher { get; set; } = 50_000;

    /// <summary>Part du secteur informel (~85%)</summary>
    public double PartSecteurInformel { get; set; } = 0.85;

    // --- Paramètres des ménages (valeurs par défaut, surchargées par la classe socio-éco) ---
    public double TauxEpargneMenage { get; set; } = 0.10;
    public double PropensionConsommation { get; set; } = 0.75;

    // --- Consommation de riz ---
    // Source : FAO / INSTAT — Madagascar est le 1er consommateur de riz per capita d'Afrique
    // Consommation moyenne : ~130 kg/personne/an (urbain ~115 kg, rural ~145 kg)

    /// <summary>Consommation annuelle de riz par personne en kg (~130 kg pour Madagascar)</summary>
    public double ConsommationRizAnnuelleKgParPersonne { get; set; } = 130;

    /// <summary>Prix du kg de riz produit localement en MGA (~2 400 MGA en 2024)</summary>
    public double PrixRizLocalKg { get; set; } = 2_400;

    /// <summary>Prix du kg de riz importé en MGA (~2 800 MGA en 2024, prix CIF + droits)</summary>
    public double PrixRizImporteKg { get; set; } = 2_800;

    /// <summary>
    /// Part du riz importé dans la consommation totale (~15-20%).
    /// Madagascar importe ~300 000 à 500 000 tonnes/an sur ~4 millions consommées.
    /// Source : INSTAT, Ministère du Commerce
    /// </summary>
    public double PartRizImporte { get; set; } = 0.18;

    // --- Eau et électricité (JIRAMA) ---
    // Source : Tableau 21 JIRAMA/ORE — Évolution mensuelle production & consommation d'électricité
    // Cumul jan-juin 2025 : Production 986 023 MWh, Conso. 701 336 MWh, Prix moy. 653 Ar/KWh
    // Consommation ménages : 332 284 MWh / 6 mois → ~55 380 MWh/mois
    // Nb ménages connectés ≈ 1 800 000 (30% de ~6M) → ~30.8 KWh/ménage connecté/mois → ~1.03 KWh/jour
    // Facture élec. moy. ≈ 30.8 KWh × 653 Ar/KWh ≈ 20 100 MGA/mois ≈ 670 MGA/jour
    // Eau courante : ~25% des ménages, facture moy. ~15 000 MGA/mois ≈ 500 MGA/jour

    /// <summary>Tarif moyen eau JIRAMA par ménage connecté par jour (MGA)</summary>
    public double TarifEauJourMenage { get; set; } = 500; // ~15 000 MGA/mois

    /// <summary>
    /// Prix moyen de l'électricité en Ariary par KWh (source : Tableau 21, cumul jan-juin 2025).
    /// Historique : 610 Ar/KWh (juil. 2023) → 653 Ar/KWh (moy. 2025) — tendance +4,7%/an.
    /// </summary>
    public double PrixElectriciteArKWh { get; set; } = 653;

    /// <summary>
    /// Consommation électrique moyenne par ménage connecté par jour en KWh.
    /// Calcul : 332 284 MWh / 6 mois / ~1 800 000 ménages / 30 jours ≈ 1.03 KWh/jour.
    /// Source : Tableau 21 — Consommation ménages jan-juin 2025.
    /// </summary>
    public double ConsommationElecMenageKWhJour { get; set; } = 1.03;

    /// <summary>
    /// Part de la production hydraulique dans la production totale (~51,6% en 2025).
    /// Le reste est thermique (fuel/diesel) — plus coûteux et polluant.
    /// Variation : -12,7% hydraulique en 2025 vs 2024 (sécheresse).
    /// </summary>
    public double PartProductionHydraulique { get; set; } = 0.516;

    /// <summary>
    /// Part de la consommation ménages dans la consommation totale (~47,4%).
    /// Industries et services : ~51,9%, Éclairage public : ~0,75%.
    /// Source : Tableau 21 cumul jan-juin 2025.
    /// </summary>
    public double PartConsommationElecMenages { get; set; } = 0.474;

    /// <summary>
    /// Taux de pertes de distribution JIRAMA (~28,9%).
    /// Pertes = Production - Consommation = 986 023 - 701 336 = 284 687 MWh / 986 023.
    /// Niveau très élevé (norme internationale ~8-12%), reflète vétusté du réseau.
    /// </summary>
    public double TauxPertesDistribution { get; set; } = 0.289;

    /// <summary>Taux d'accès des ménages à l'eau courante JIRAMA (~25%)</summary>
    public double TauxAccesEau { get; set; } = 0.25;

    /// <summary>Taux d'accès des ménages à l'électricité JIRAMA (~30%)</summary>
    public double TauxAccesElectricite { get; set; } = 0.30;

    /// <summary>
    /// Coût de transport aller-retour pour payer la facture JIRAMA à l'agence (MGA).
    /// À Madagascar, les ménages doivent se déplacer physiquement (~1 200 MGA en taxi-brousse).
    /// Ce coût est appliqué 1 fois par mois (tous les 30 jours) pour chaque ménage connecté.
    /// </summary>
    public double CoutTransportPaiementJirama { get; set; } = 1_200;

    /// <summary>
    /// Consommation électrique par employé d'entreprise par jour (KWh).
    /// Calcul : Industries et services consomment ~363 763 MWh / 6 mois / 30 jours ≈ 2 021 MWh/jour.
    /// Emploi formel estimé ~800 000 personnes → ~2.5 KWh/employé/jour.
    /// Source : Tableau 21 JIRAMA/ORE jan-juin 2025 — Consommation industries et services.
    /// </summary>
    public double ConsommationElecParEmployeKWhJour { get; set; } = 2.5;

    /// <summary>
    /// Consommation électrique quotidienne de l'État en KWh (éclairage public + bâtiments).
    /// Éclairage public : 5 290 MWh / 6 mois / 30 jours ≈ 29.4 MWh/jour = 29 400 KWh/jour.
    /// Bâtiments publics (estimation) : ~15 000 KWh/jour.
    /// Total État : ~44 400 KWh/jour.
    /// Source : Tableau 21 JIRAMA/ORE + estimation bâtiments publics.
    /// </summary>
    public double ConsommationElecEtatKWhJour { get; set; } = 44_400;

    // --- Paramètres des entreprises ---
    public double MargeBeneficiaireEntreprise { get; set; } = 0.20;
    public double ProductiviteParEmploye { get; set; } = 15_000;

    /// <summary>
    /// Trésorerie initiale par secteur d'activité (en MGA).
    /// Représente le fonds de roulement typique d'une entreprise du secteur.
    /// L'utilisateur peut ajuster ces valeurs dans l'interface.
    /// </summary>
    public Dictionary<ESecteurActivite, double> TresorerieInitialeParSecteur { get; set; } = new()
    {
        [ESecteurActivite.Agriculture] = 500_000,
        [ESecteurActivite.Textiles] = 4_000_000,
        [ESecteurActivite.Commerces] = 8_000_000,
        [ESecteurActivite.Services] = 4_000_000,
        [ESecteurActivite.SecteurMinier] = 50_000_000,
        [ESecteurActivite.Construction] = 7_000_000,
    };

    /// <summary>
    /// Part des entreprises dans le secteur agricole (~25% du PIB, ~75% de l'emploi).
    /// Source : INSTAT 2024.
    /// </summary>
    public double PartEntreprisesAgricoles { get; set; } = 0.30;

    /// <summary>
    /// Part des entreprises dans le BTP/Construction (~5% du PIB).
    /// </summary>
    public double PartEntreprisesConstruction { get; set; } = 0.05;

    // --- Cotisations sociales (CNaPS) ---
    /// <summary>
    /// Taux de cotisation patronale CNaPS (~18% du salaire brut).
    /// 13% retraite + 5% accidents du travail.
    /// Source : Code de la Protection Sociale de Madagascar.
    /// </summary>
    public double TauxCotisationsPatronalesCNaPS { get; set; } = 0.18;

    /// <summary>
    /// Taux de cotisation salariale CNaPS (~1% du salaire brut).
    /// </summary>
    public double TauxCotisationsSalarialesCNaPS { get; set; } = 0.01;

    // --- Aide internationale ---
    /// <summary>
    /// Dons reçus par jour (MGA).
    /// TOFE sept. 2025 : 1 000,1 Mds MGA cumul 9 mois → ~3 704 M MGA/jour.
    /// Inclut dons courants (27,3 Mds) + dons en capital (972,8 Mds).
    /// Source : TOFE Madagascar sept. 2025.
    /// </summary>
    public double AideInternationaleJour { get; set; } = 3_704_000_000;

    // --- Subventions État → JIRAMA ---
    /// <summary>
    /// Subventions de l'État à la JIRAMA par jour (MGA).
    /// ~500 Mds MGA/an ≈ ~1 370 M MGA/jour.
    /// La JIRAMA est chroniquement déficitaire et subventionnée par l'État.
    /// Source : FMI Article IV 2024.
    /// </summary>
    public double SubventionJiramaJour { get; set; } = 1_370_000_000;

    // --- Transferts de la diaspora (remittances) ---
    /// <summary>
    /// Transferts de la diaspora par jour répartis entre les ménages (MGA total).
    /// ~$600M/an ≈ 2 700 Mds MGA/an ≈ ~7 400 M MGA/jour (répartis entre tous les ménages).
    /// Source : BCM, Banque Mondiale 2024.
    /// </summary>
    public double RemittancesJour { get; set; } = 7_400_000_000;

    // --- Loyers imputés ---
    /// <summary>
    /// Loyer imputé moyen par ménage propriétaire par jour (MGA).
    /// ~65% des ménages sont propriétaires. Loyer fictif ~30 000 MGA/mois ≈ 1 000 MGA/jour.
    /// Représente ~5-8% du PIB en comptabilité nationale (SCN 2008).
    /// </summary>
    public double LoyerImputeJourParMenage { get; set; } = 1_000;

    /// <summary>Part des ménages propriétaires occupants (~65%)</summary>
    public double TauxMenagesProprietaires { get; set; } = 0.65;

    // --- Fonctionnaires ---
    /// <summary>
    /// Nombre d'agents publics (fonctionnaires + contractuels + militaires + enseignants + santé).
    ///
    /// TOFE Tableau 6 sept. 2025 :
    ///   Base engagement  : Personnel = 2 815 384 M → 10 427 M/jour → 350k × 894k/mois
    ///   Base ordonnancement : Personnel = 2 716 852 M → 10 063 M/jour → 350k × 863k/mois
    ///     dont salaires et traitements : 2 599 095 M (96%) + indemnités : 117 757 M (4%)
    ///
    /// On retient la base ordonnancement (montants réellement décaissés).
    /// Source : TOFE Madagascar Tableau 6, sept. 2025.
    /// </summary>
    public int NombreFonctionnaires { get; set; } = 350_000;

    /// <summary>
    /// Salaire moyen mensuel des agents publics (MGA), toutes indemnités incluses.
    ///
    /// TOFE Tableau 6 sept. 2025 (base ordonnancement) :
    ///   Personnel = 2 716 852 M / 9 mois / 350 000 agents = 863 000 MGA/mois
    ///   (base engagement donnerait 894 000 MGA/mois)
    ///
    /// Décomposition TOFE :
    ///   - Salaires et traitements : 2 599 095 M (96%)
    ///   - Indemnités : 117 757 M (4%)
    ///
    /// Source : TOFE Madagascar Tableau 6, sept. 2025 (base ordonnancement).
    /// </summary>
    public double SalaireMoyenFonctionnaireMensuel { get; set; } = 863_000;

    // --- FBCF (Formation Brute de Capital Fixe) ---
    /// <summary>
    /// Part des bénéfices positifs des entreprises réinvestie (investissement privé).
    /// ~20-30% à Madagascar. Source : Banque Mondiale 2024.
    /// </summary>
    public double TauxReinvestissementPrive { get; set; } = 0.25;

    /// <summary>
    /// Dépenses en capital de l'État par jour (MGA) = FBCF publique.
    /// TOFE sept. 2025 : 3 651,9 Mds cumul 9 mois → ~13 526 M MGA/jour.
    /// Inclut financement intérieur (1 050 Mds) + extérieur (2 601,9 Mds).
    /// Source : TOFE Madagascar sept. 2025.
    /// </summary>
    public double DepensesCapitalJour { get; set; } = 13_526_000_000;

    /// <summary>
    /// Intérêts de la dette publique par jour (MGA).
    /// TOFE sept. 2025 : dette extérieure (164,6 Mds) + intérieure (288,4 Mds) = 453 Mds / 9 mois.
    /// → ~1 678 M MGA/jour.
    /// Source : TOFE Madagascar sept. 2025.
    /// </summary>
    public double InteretsDetteJour { get; set; } = 1_678_000_000;

    /// <summary>
    /// Dette publique initiale (MGA).
    /// Madagascar : dette totale ~$6,5 Mds ≈ 29 250 Mds MGA (au taux 4 500 MGA/USD).
    /// Source : FMI Article IV 2024.
    /// </summary>
    public double DettePubliqueInitiale { get; set; } = 29_250_000_000_000;

    // --- Commerce extérieur (Import / Export) ---
    // 1 agent agrégé par catégorie INSTAT (pas de nombre configurable d'importateurs/exportateurs)

    // --- Cibles de recettes fiscales (TOFE Tableau 5 sept. 2025, référence pour validation) ---
    // Ces valeurs ne sont PAS des paramètres d'entrée : elles servent de cibles de validation.
    // Si le simulateur produit des recettes très différentes, c'est un signal d'incohérence.
    //
    // TOFE Tableau 5 — Recettes fiscales intérieures, cumul sept. 2025 (M MGA) :
    //   IRSA (salaires)     :   586 436 M →  2 172 M/jour
    //   IS (sociétés)       :   106 705 M →    395 M/jour  (faible → 85% informel)
    //   TVA intérieure      : 1 245 741 M →  4 614 M/jour
    //   IMP (marchés publ.) :   201 566 M →    746 M/jour
    //   DA (accise intér.)  :   536 530 M →  1 987 M/jour  (⚠️ non modélisé côté domestique)
    //   IR (revenus)        : 1 143 597 M →  4 235 M/jour
    //   Total fiscal intér. : 3 954 415 M → 14 646 M/jour
    //
    // Observations clés :
    //   - IS très faible (395 M/j) confirme que ~85% des entreprises sont informelles
    //   - TVA intérieure (4 614 M/j) implique ~23 Mds/j de ventes formelles assujetties
    //   - DA domestique (1 987 M/j) : accise sur biens locaux, non captée dans le modèle actuel

    /// <summary>Taux de base des droits de douane (~12% moyenne pondérée Madagascar)</summary>
    public double TauxDroitsDouane { get; set; } = 0.12;

    /// <summary>Taux de base des droits d'accise (~10% moyenne, très variable par produit)</summary>
    public double TauxAccise { get; set; } = 0.10;

    /// <summary>Taux de base de la taxe à l'exportation (~3% moyenne Madagascar)</summary>
    public double TauxTaxeExport { get; set; } = 0.03;

    /// <summary>
    /// Valeur FOB journalière par catégorie d'export (en millions MGA).
    /// Chaque catégorie dispose d'un agent agrégé unique portant la moyenne INSTAT.
    /// INSTAT sept 2025 : total ~844 910 M MGA/mois → ~28 164 M MGA/jour.
    /// </summary>
    public Dictionary<ECategorieExport, double> FOBJourParCategorie { get; set; } = new()
    {
        [ECategorieExport.BiensAlimentaires] = 280,
        [ECategorieExport.Vanille] = 150,
        [ECategorieExport.Crevettes] = 320,
        [ECategorieExport.Cafe] = 80,
        [ECategorieExport.Girofle] = 120,
        [ECategorieExport.ProduitsMiniers] = 500,
        [ECategorieExport.ZonesFranches] = 250,
    };

    /// <summary>
    /// Valeur CIF journalière par catégorie d'import (en millions MGA).
    /// Chaque catégorie dispose d'un agent agrégé unique portant la moyenne INSTAT.
    /// </summary>
    public Dictionary<ECategorieImport, double> CIFJourParCategorie { get; set; } = new()
    {
        [ECategorieImport.Alimentaire] = 600,        // Biens alimentaires (inclut riz, sucre)
        [ECategorieImport.Carburant] = 800,           // Énergie / carburants
        [ECategorieImport.Vehicule] = 700,            // Équipement / véhicules
        [ECategorieImport.MatierePremiere] = 80,      // Ciment / matières premières
        [ECategorieImport.BienConsommation] = 300,    // Biens de consommation
        [ECategorieImport.Electronique] = 200,        // Électronique
    };

    // --- Dépenses publiques ---
    /// <summary>
    /// Budget journalier de fonctionnement de l'État (hors personnel, hors subventions JIRAMA, hors capital, hors intérêts).
    /// 
    /// TOFE Tableau 6 sept. 2025 (base engagement) :
    ///   Fonctionnement total = 1 238 649 M / 9 mois = 4 588 M/jour
    ///   - Biens et services : 273 736 M → 1 014 M/jour
    ///   - Transferts et subventions : 964 912 M → 3 574 M/jour
    ///     dont SubventionJirama (~1 370 M/jour) comptée séparément
    ///
    /// ⚠️ DepensesPubliquesJour = Fonctionnement − SubventionJirama = 4 588 − 1 370 = 3 218 M/jour
    /// Évite le double-comptage car SubventionJiramaJour est ajouté séparément dans Government.SimulerJournee.
    ///
    /// Source : TOFE Madagascar Tableau 6, sept. 2025 (base engagement).
    /// </summary>
    public double DepensesPubliquesJour { get; set; } = 3_218_000_000;

    // --- Mode injection directe des exports (données INSTAT) ---
    /// <summary>
    /// Si true, les exportations ne sont PAS simulées : on injecte directement
    /// la moyenne journalière FOB calculée à partir des données INSTAT.
    /// Les exportateurs gardent leur rôle d'employeurs et de vendeurs locaux,
    /// mais leur production export est remplacée par les moyennes INSTAT.
    /// Activé par défaut pour un alignement fiable avec les statistiques officielles.
    /// </summary>
    public bool UseExportCalibresDirectement { get; set; } = true;

    /// <summary>
    /// FOB journalier moyen par catégorie (en millions MGA/jour).
    /// Calculé = moyenne mensuelle INSTAT / 30 jours.
    /// Utilisé uniquement si UseExportCalibresDirectement = true.
    /// Initialisé par défaut à partir des moyennes INSTAT (Tableau 32).
    /// </summary>
    public Dictionary<ECategorieExport, double> FOBCalibresJour { get; set; }
        = ExportHistoricalData.MoyenneJournaliereParCategorie();

    /// <summary>
    /// Source des données exports.
    /// </summary>
    public string SourceCalibration { get; set; } = "Moyenne INSTAT Tableau 32 (oct 2023 – sept 2025)";

    // --- Mode injection directe des imports (données INSTAT) ---
    /// <summary>
    /// Si true, les importations ne sont PAS simulées : on injecte directement
    /// la moyenne journalière CIF calculée à partir des données INSTAT.
    /// Activé par défaut pour un alignement fiable avec les statistiques officielles.
    /// </summary>
    public bool UseImportCalibresDirectement { get; set; } = true;

    /// <summary>
    /// CIF journalier moyen par catégorie (en millions MGA/jour).
    /// Calculé = moyenne mensuelle INSTAT / 30 jours.
    /// Utilisé uniquement si UseImportCalibresDirectement = true.
    /// Initialisé par défaut à partir des moyennes INSTAT (Tableau 33).
    /// </summary>
    public Dictionary<ECategorieImport, double> CIFCalibresJour { get; set; }
        = ImportHistoricalData.MoyenneJournaliereParCategorie();

    /// <summary>
    /// Source des données imports.
    /// </summary>
    public string SourceCalibrationImports { get; set; } = "Moyenne INSTAT Tableau 33 (juil 2023 – juin 2025)";

    /// <summary>
    /// Scénarios prédéfinis pour Madagascar
    /// </summary>
    public static ScenarioConfig BaseMadagascar() => new()
    {
        Name = "Base - Madagascar",
        Description = "Économie malgache avec distribution salariale log-normale (Gini ~0.43, médiane 170k MGA)"
    };

    public static ScenarioConfig HausseFiscale() => new()
    {
        Name = "Hausse fiscale",
        Description = "L'État augmente la TVA à 25% et l'IS à 25% pour augmenter les recettes",
        TauxTVA = 0.25,
        TauxIS = 0.25
    };

    public static ScenarioConfig BaisseTauxDirecteur() => new()
    {
        Name = "Baisse du taux directeur",
        Description = "La Banque Centrale baisse le taux directeur de 9% à 5% pour stimuler l'économie",
        TauxDirecteur = 0.05
    };

    public static ScenarioConfig RelanceBudgetaire() => new()
    {
        Name = "Relance budgétaire",
        Description = "L'État double ses dépenses publiques pour stimuler la croissance",
        DepensesPubliquesJour = 1_000_000,
        TauxIS = 0.15
    };

    public static ScenarioConfig CriseInflationniste() => new()
    {
        Name = "Crise inflationniste",
        Description = "L'inflation monte à 20%, le taux directeur augmente à 15%",
        TauxInflation = 0.20,
        TauxDirecteur = 0.15
    };

    public static ScenarioConfig AusteriteFiscale() => new()
    {
        Name = "Austérité fiscale",
        Description = "L'État réduit les dépenses publiques et augmente IS/TVA",
        DepensesPubliquesJour = 250_000,
        TauxIS = 0.25,
        TauxTVA = 0.25
    };

    public static ScenarioConfig ReductionInegalites() => new()
    {
        Name = "Réduction des inégalités",
        Description = "Politiques de redistribution : hausse SMIG, sigma réduit (moins d'inégalités)",
        SalaireMedian = 220_000,
        SalaireSigma = 0.65,
        SalairePlancher = 100_000,
        DepensesPubliquesJour = 700_000
    };

    public static ScenarioConfig ChocPetrolier() => new()
    {
        Name = "Choc pétrolier",
        Description = "Le prix du carburant double (11 000 MGA/L), impactant transport et inflation",
        PrixCarburantLitre = 11_000,
        TauxInflation = 0.12
    };

    public static ScenarioConfig OuvertureCommerciale() => new()
    {
        Name = "Ouverture commerciale",
        Description = "Baisse des droits de douane et accises pour favoriser les échanges",
        TauxDroitsDouane = 0.08,
        TauxAccise = 0.07,
        TauxTaxeExport = 0.02
    };

    public static ScenarioConfig ProtectionnismeDouanier() => new()
    {
        Name = "Protectionnisme douanier",
        Description = "L'État augmente les droits de douane et accises pour protéger l'industrie locale",
        TauxDroitsDouane = 0.25,
        TauxAccise = 0.20
    };

    public static List<ScenarioConfig> TousLesScenarios() =>
    [
        BaseMadagascar(),
        HausseFiscale(),
        BaisseTauxDirecteur(),
        RelanceBudgetaire(),
        CriseInflationniste(),
        AusteriteFiscale(),
        ReductionInegalites(),
        ChocPetrolier(),
        OuvertureCommerciale(),
        ProtectionnismeDouanier()
    ];
}
