using MachineLearning.Web.Models.Agents.Companies;

namespace MachineLearning.Web.Models.Simulation.Models;

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
    public ECategorieExport Categorie { get; set; } = ECategorieExport.Vanille;

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
    /// 1. Production pour l'export (valeur FOB) — si jour ouvrable pour le secteur
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
        double tauxTaxeExport,
        bool estJourOuvrable = true,
        Jirama? jirama = null,
        double consoElecParEmployeKWhJour = 0,
        double tauxCNaPSPatronale = 0)
    {
        var result = new DailyExporterResult();

        // Si jour de repos pour ce secteur, pas d'activité
        if (!estJourOuvrable)
        {
            result.Tresorerie = Tresorerie;
            return result;
        }

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
            tauxIS, tauxTVA, tauxInflation, tauxDirecteur, estJourOuvrable,
            jirama, consoElecParEmployeKWhJour, tauxCNaPSPatronale);

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
        result.ValeurAjoutee = baseResult.ValeurAjoutee;

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
        ECategorieExport.BiensAlimentaires => 0.95,
        ECategorieExport.Vanille => 1.00,
        ECategorieExport.Crevettes => 1.05,
        ECategorieExport.Cafe => 0.92,
        ECategorieExport.Girofle => 0.98,
        ECategorieExport.ProduitsMiniers => 0.95,
        ECategorieExport.ZonesFranches => 0.90,
        _ => 1.00
    };

    /// <summary>Coefficient de taxe à l'exportation par catégorie.</summary>
    private double CoefficientTaxeParCategorie() => Categorie switch
    {
        ECategorieExport.BiensAlimentaires => 0.80,
        ECategorieExport.Vanille => 1.50,
        ECategorieExport.Crevettes => 0.80,
        ECategorieExport.Cafe => 0.70,
        ECategorieExport.Girofle => 1.00,
        ECategorieExport.ProduitsMiniers => 1.20,
        ECategorieExport.ZonesFranches => 0.20,
        _ => 1.00
    };

    /// <summary>Redevance à l'exportation par catégorie (~1-2%).</summary>
    private double CoefficientRedevanceParCategorie() => Categorie switch
    {
        ECategorieExport.BiensAlimentaires => 0.015,
        ECategorieExport.Vanille => 0.02,
        ECategorieExport.Crevettes => 0.015,
        ECategorieExport.Cafe => 0.015,
        ECategorieExport.Girofle => 0.02,
        ECategorieExport.ProduitsMiniers => 0.025,
        ECategorieExport.ZonesFranches => 0.005,
        _ => 0.015
    };
}
