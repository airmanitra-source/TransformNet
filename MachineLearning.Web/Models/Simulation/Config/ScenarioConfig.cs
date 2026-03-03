using MachineLearning.Web.Models.Simulation.Companies;

namespace MachineLearning.Web.Models.Simulation.Config;

/// <summary>
/// Configuration d'un scénario de simulation économique.
/// </summary>
public class ScenarioConfig
{
    public string Name { get; set; } = "Scénario de base";
    public string Description { get; set; } = "Paramètres macroéconomiques de base de Madagascar";

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

    // --- Paramètres des entreprises ---
    public double MargeBeneficiaireEntreprise { get; set; } = 0.20;
    public double ProductiviteParEmploye { get; set; } = 15_000;

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
    public Dictionary<CategorieExport, double> RepartitionExportCategories { get; set; } = new()
    {
        [CategorieExport.BiensAlimentaires] = 0.25,
        [CategorieExport.Vanille] = 0.08,
        [CategorieExport.Crevettes] = 0.025,
        [CategorieExport.Cafe] = 0.015,
        [CategorieExport.Girofle] = 0.06,
        [CategorieExport.ProduitsMiniers] = 0.30,
        [CategorieExport.ZonesFranches] = 0.27,
    };

    /// <summary>
    /// Valeur FOB journalière par catégorie d'export (en millions MGA).
    /// INSTAT sept 2025 : total ~844 910 M MGA/mois → ~28 164 M MGA/jour.
    /// Ces valeurs × nb exportateurs de la catégorie ≈ FOB journalier INSTAT.
    /// </summary>
    public Dictionary<CategorieExport, double> FOBJourParCategorie { get; set; } = new()
    {
        [CategorieExport.BiensAlimentaires] = 280,   // ~8 400 M/mois pour ~30 exportateurs
        [CategorieExport.Vanille] = 150,              // très variable selon saison
        [CategorieExport.Crevettes] = 320,            // peu d'exportateurs, haute valeur unitaire
        [CategorieExport.Cafe] = 80,
        [CategorieExport.Girofle] = 120,
        [CategorieExport.ProduitsMiniers] = 500,      // Ambatovy, QMM = gros volumes
        [CategorieExport.ZonesFranches] = 250,        // textile, stable
    };

    // --- Dépenses publiques ---
    public double DepensesPubliquesJour { get; set; } = 500_000;

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
