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
    /// Aide internationale reçue par jour (MGA).
    /// Madagascar reçoit ~$1,5 Mds/an ≈ 6 750 Mds MGA/an ≈ ~18 500 M MGA/jour.
    /// Représente ~35% du budget de l'État.
    /// Source : OCDE/CAD, Banque Mondiale 2024.
    /// </summary>
    public double AideInternationaleJour { get; set; } = 18_500_000_000;

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
    /// Nombre de fonctionnaires de l'État (~200 000 agents).
    /// Source : Ministère des Finances 2024.
    /// </summary>
    public int NombreFonctionnaires { get; set; } = 200_000;

    /// <summary>
    /// Salaire moyen mensuel des fonctionnaires (MGA).
    /// ~300 000 MGA/mois (estimation moyenne toutes catégories).
    /// </summary>
    public double SalaireMoyenFonctionnaireMensuel { get; set; } = 300_000;

    // --- FBCF (Formation Brute de Capital Fixe) ---
    /// <summary>
    /// Part des bénéfices positifs des entreprises réinvestie (investissement privé).
    /// ~20-30% à Madagascar. Source : Banque Mondiale 2024.
    /// </summary>
    public double TauxReinvestissementPrive { get; set; } = 0.25;

    /// <summary>
    /// Part des dépenses publiques consacrée à l'investissement public (infrastructures, équipements).
    /// ~25-35% du budget à Madagascar. Source : Loi de Finances 2024.
    /// </summary>
    public double PartInvestissementPublic { get; set; } = 0.30;

    // --- Commerce extérieur (Import / Export) ---
    /// <summary>Part des entreprises qui sont importatrices (~20% à Madagascar)</summary>
    public double TauxImportateurs { get; set; } = 0.20;

    /// <summary>Part des entreprises qui sont exportatrices (~10% à Madagascar)</summary>
    public double TauxExportateurs { get; set; } = 0.10;

    /// <summary>Taux de base des droits de douane (~12% moyenne pondérée Madagascar)</summary>
    public double TauxDroitsDouane { get; set; } = 0.12;

    /// <summary>Taux de base des droits d'accise (~10% moyenne, très variable par produit)</summary>
    public double TauxAccise { get; set; } = 0.10;

    /// <summary>Taux de base de la taxe à l'exportation (~3% moyenne Madagascar)</summary>
    public double TauxTaxeExport { get; set; } = 0.03;

    /// <summary>
    /// Répartition des exportateurs par catégorie (poids normalisés).
    /// Calibré sur les données INSTAT 2024 (proportions des FOB totaux).
    /// </summary>
    public Dictionary<ECategorieExport, double> RepartitionExportCategories { get; set; } = new()
    {
        [ECategorieExport.BiensAlimentaires] = 0.25,
        [ECategorieExport.Vanille] = 0.08,
        [ECategorieExport.Crevettes] = 0.025,
        [ECategorieExport.Cafe] = 0.015,
        [ECategorieExport.Girofle] = 0.06,
        [ECategorieExport.ProduitsMiniers] = 0.30,
        [ECategorieExport.ZonesFranches] = 0.27,
    };

    /// <summary>
    /// Valeur FOB journalière par catégorie d'export (en millions MGA).
    /// INSTAT sept 2025 : total ~844 910 M MGA/mois → ~28 164 M MGA/jour.
    /// Ces valeurs × nb exportateurs de la catégorie ≈ FOB journalier INSTAT.
    /// </summary>
    public Dictionary<ECategorieExport, double> FOBJourParCategorie { get; set; } = new()
    {
        [ECategorieExport.BiensAlimentaires] = 280,   // ~8 400 M/mois pour ~30 exportateurs
        [ECategorieExport.Vanille] = 150,              // très variable selon saison
        [ECategorieExport.Crevettes] = 320,            // peu d'exportateurs, haute valeur unitaire
        [ECategorieExport.Cafe] = 80,
        [ECategorieExport.Girofle] = 120,
        [ECategorieExport.ProduitsMiniers] = 500,      // Ambatovy, QMM = gros volumes
        [ECategorieExport.ZonesFranches] = 250,        // textile, stable
    };

    /// <summary>
    /// Valeur CIF journalière par catégorie d'import (en millions MGA).
    /// Ces valeurs × nb importateurs de la catégorie ≈ CIF journalier INSTAT.
    /// Utilisé pour paramétrer les importateurs lors de la génération du dataset (calibration).
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
    /// Budget journalier de dépenses publiques de fonctionnement (hors salaires fonctionnaires, hors subventions).
    /// À l'échelle de référence Madagascar (~6M ménages).
    /// Budget total ~8 000 Mds MGA/an. Fonctionnement hors salaires et transferts ≈ ~40% ≈ 3 200 Mds/an ≈ 8,8 Mds/jour.
    /// Sera mis à l'échelle par facteurEchelle dans le simulateur.
    /// </summary>
    public double DepensesPubliquesJour { get; set; } = 8_800_000_000;

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
        Description = "Hausse des importateurs/exportateurs, baisse des droits de douane",
        TauxImportateurs = 0.35,
        TauxExportateurs = 0.20,
        TauxDroitsDouane = 0.08,
        TauxAccise = 0.07,
        TauxTaxeExport = 0.02
    };

    public static ScenarioConfig ProtectionnismeDouanier() => new()
    {
        Name = "Protectionnisme douanier",
        Description = "L'État augmente les droits de douane et accises pour protéger l'industrie locale",
        TauxDroitsDouane = 0.25,
        TauxAccise = 0.20,
        TauxImportateurs = 0.10,
        TauxExportateurs = 0.15
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
