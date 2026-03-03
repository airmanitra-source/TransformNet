namespace MachineLearning.Web.Models.Simulation.Companies;

/// <summary>
/// Entreprise exportatrice à Madagascar.
/// Exporte des produits malgaches, génère des devises, paie des taxes à l'exportation.
///
/// Réalités du commerce extérieur malgache :
/// - Madagascar exporte ~3 milliards USD/an (2023)
/// - Principaux produits : vanille (30%), nickel/cobalt (20%), textile (15%),
///   crevettes/pêche (8%), girofle/épices (7%), litchis (3%)
/// - Taxes à l'exportation : redevance + prélèvement de 2-5% selon produit
/// - Zone franche (textile) : régime fiscal favorable
/// - Les exportations rapportent des devises → renforcement de l'Ariary
///
/// Source : BCM, INSTAT, Ministère du Commerce
/// </summary>
public class Exporter : Company
{
    /// <summary>Catégorie principale d'exportation</summary>
    public CategorieExport Categorie { get; set; } = CategorieExport.Vanille;

    /// <summary>Valeur FOB journalière des exportations (en MGA)</summary>
    public double ValeurFOBJour { get; set; } = 300_000;

    /// <summary>Part du chiffre d'affaires provenant de l'exportation</summary>
    public double PartExport { get; set; } = 0.70;

    /// <summary>Taux de change implicite USD/MGA pour le calcul des recettes en devises</summary>
    public double TauxChangeMGA { get; set; } = 4_500;

    // --- Cumuls ---
    public double TotalExportationsFOB { get; set; }
    public double TotalTaxeExport { get; set; }
    public double TotalRedevanceExport { get; set; }
    public double TotalDevisesRapatriees { get; set; }

    /// <summary>
    /// Simule une journée pour un exportateur :
    /// 1. Production pour l'export (valeur FOB)
    /// 2. Paiement taxe à l'exportation + redevance
    /// 3. Ventes locales complémentaires (B2C/B2B)
    /// 4. Rapatriement de devises
    /// </summary>
    public DailyExporterResult SimulerJourneeExport(
        double demandeConsommationMenages,
        double tauxIS,
        double tauxTVA,
        double tauxInflation,
        double tauxDirecteur,
        double tauxTaxeExport)
    {
        var result = new DailyExporterResult();

        // 1. Production pour l'export (FOB)
        double facteurInflation = 1.0 + (tauxInflation / 365.0);
        double fobJour = ValeurFOBJour * facteurInflation;

        // Les produits de rente ont des prix internationaux volatils
        fobJour *= CoefficientPrixInternational();
        result.ValeurFOB = fobJour;
        TotalExportationsFOB += fobJour;

        // 2. Taxe à l'exportation (prélèvement fiscal)
        double taxeTaux = tauxTaxeExport * CoefficientTaxeParCategorie();
        double taxeExport = fobJour * taxeTaux;
        result.TaxeExport = taxeExport;
        TotalTaxeExport += taxeExport;

        // 3. Redevance à l'exportation (~1-2% selon produit)
        double redevance = fobJour * CoefficientRedevanceParCategorie();
        result.RedevanceExport = redevance;
        TotalRedevanceExport += redevance;

        // 4. Devises rapatriées (FOB - taxes)
        double devisesNettes = fobJour - taxeExport - redevance;
        result.DevisesRapatriees = devisesNettes;
        TotalDevisesRapatriees += devisesNettes;

        // 5. Simulation entreprise classique (ventes locales)
        // L'exportateur vend aussi localement (1 - PartExport)
        double demandeLocaleAjustee = demandeConsommationMenages * (1.0 - PartExport);
        var baseResult = SimulerJournee(
            demandeLocaleAjustee,
            tauxIS, tauxTVA, tauxInflation, tauxDirecteur);

        // Copier les résultats de base
        result.VentesB2C = baseResult.VentesB2C;
        result.VentesB2B = baseResult.VentesB2B;
        result.ChargesSalariales = baseResult.ChargesSalariales;
        result.AchatsB2B = baseResult.AchatsB2B;
        result.TVACollectee = baseResult.TVACollectee;
        result.ImpotIS = baseResult.ImpotIS;
        result.BeneficeAvantImpot = baseResult.BeneficeAvantImpot;
        result.FluxNetJour = baseResult.FluxNetJour;
        result.CoutFinancement = baseResult.CoutFinancement;

        // 6. Ajouter le revenu export à la trésorerie
        Tresorerie += devisesNettes;
        result.Tresorerie = Tresorerie;

        return result;
    }

    /// <summary>
    /// Coefficient du prix international selon la catégorie (volatilité).
    /// La vanille est très volatile, le textile est stable.
    /// </summary>
    private double CoefficientPrixInternational() => Categorie switch
    {
        CategorieExport.BiensAlimentaires => 0.95,
        CategorieExport.Vanille => 1.00,
        CategorieExport.Crevettes => 1.05,
        CategorieExport.Cafe => 0.92,
        CategorieExport.Girofle => 0.98,
        CategorieExport.ProduitsMiniers => 0.95,
        CategorieExport.ZonesFranches => 0.90,
        _ => 1.00
    };

    /// <summary>Coefficient de taxe à l'exportation par catégorie.</summary>
    private double CoefficientTaxeParCategorie() => Categorie switch
    {
        CategorieExport.BiensAlimentaires => 0.80,
        CategorieExport.Vanille => 1.50,
        CategorieExport.Crevettes => 0.80,
        CategorieExport.Cafe => 0.70,
        CategorieExport.Girofle => 1.00,
        CategorieExport.ProduitsMiniers => 1.20,
        CategorieExport.ZonesFranches => 0.20,
        _ => 1.00
    };

    /// <summary>Redevance à l'exportation par catégorie (~1-2%).</summary>
    private double CoefficientRedevanceParCategorie() => Categorie switch
    {
        CategorieExport.BiensAlimentaires => 0.015,
        CategorieExport.Vanille => 0.02,
        CategorieExport.Crevettes => 0.015,
        CategorieExport.Cafe => 0.015,
        CategorieExport.Girofle => 0.02,
        CategorieExport.ProduitsMiniers => 0.025,
        CategorieExport.ZonesFranches => 0.005,
        _ => 0.015
    };
}

/// <summary>
/// Catégories d'exportation de Madagascar alignées sur les statistiques INSTAT.
/// Source : Tableau 32 — Évolution mensuelle des exportations FOB.
/// </summary>
public enum CategorieExport
{
    /// <summary>Biens alimentaires (hors vanille/crevettes/café/girofle)</summary>
    BiensAlimentaires,
    /// <summary>Vanille — 1er producteur mondial</summary>
    Vanille,
    /// <summary>Crevettes, pêche</summary>
    Crevettes,
    /// <summary>Café</summary>
    Cafe,
    /// <summary>Girofle, épices</summary>
    Girofle,
    /// <summary>Produits miniers (nickel, cobalt, ilménite, saphir…)</summary>
    ProduitsMiniers,
    /// <summary>Zones franches (textile, confection, électronique)</summary>
    ZonesFranches
}

/// <summary>
/// Résultat journalier d'un exportateur.
/// </summary>
public class DailyExporterResult : DailyCompanyResult
{
    public double ValeurFOB { get; set; }
    public double TaxeExport { get; set; }
    public double RedevanceExport { get; set; }
    public double DevisesRapatriees { get; set; }

    /// <summary>Total taxes export générées ce jour</summary>
    public double RecettesFiscalesExport => TaxeExport + RedevanceExport;
}
