namespace Company.Module.Models;

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

    /// <summary>Taux de change moyen MGA/$ (utilisé pour les devises rapatriées)</summary>
    public double TauxChangeMGA { get; set; } = 4_500;

    // --- Cumuls ---
    public double TotalExportationsFOB { get; set; }
    public double TotalTaxeExport { get; set; }
    public double TotalRedevanceExport { get; set; }
    public double TotalDevisesRapatriees { get; set; }

    /// <summary>
    /// Simule une journée pour un exportateur :
    /// 1. Ventes locales (B2C/B2B) + Exportations (FOB)
    /// 2. Taxes sur exportation (redevance statistique et prélèvement taxe à l'export)
    /// 3. Rapatriement de devises (flux financiers en USD -> MGA)
    /// 4. Charges + IS comme une entreprise normale
    /// </summary>
    public DailyExporterResult SimulerJourneeExport(
        double demandeConsommationMenages,
        double tauxIS,
        double tauxTVA,
        double tauxInflation,
        double tauxDirecteur,
        double tauxTaxeExport,
        bool estJourOuvrable = true,
        Jirama? Jirama = null,
        double consoElecParEmployeKWhJour = 0,
        double tauxCNaPSPatronale = 0)
    {
        var result = new DailyExporterResult();

        if (!estJourOuvrable)
        {
            result.Tresorerie = Tresorerie;
            return result;
        }

        // 1. Calcul de l'exportation FOB du jour (ajusté par l'inflation locale)
        double facteurInflation = 1.0 + (tauxInflation / 365.0);
        double fobJour = ValeurFOBJour * facteurInflation;
        result.ValeurFOB = fobJour;
        TotalExportationsFOB += fobJour;

        // 2. Taxes à l'exportation (redevance statistique 2% + prélèvement)
        double redevanceStat = fobJour * 0.02;
        double taxeExport = fobJour * tauxTaxeExport * CoefficientTaxeExportParCategorie();
        result.RedevanceExport = redevanceStat;
        result.TaxeExport = taxeExport;
        TotalRedevanceExport += redevanceStat;
        TotalTaxeExport += taxeExport;

        // 3. Simuler le reste du CA via le moteur de base (CA_total = VentesExport + VentesLocales)
        // Les ventes FOB ne sont pas soumises à la TVA locale (exonéré)
        double ventesLocalesDemande = demandeConsommationMenages * (1.0 - PartExport);
        
        // Simuler comme une entreprise normale pour les aspects communs
        var baseResult = base.SimulerJournee(
            ventesLocalesDemande,
            tauxIS, tauxTVA, tauxInflation, tauxDirecteur, 
            estJourOuvrable, Jirama, consoElecParEmployeKWhJour, tauxCNaPSPatronale);

        // Ajuster les résultats : le CA total inclut les exportations FOB
        baseResult.VentesB2C = ventesLocalesDemande; // baseResult a déjà calculé CA effectif basé sur demande
        double caReelLocal = baseResult.VentesB2C + baseResult.VentesB2B;
        double caTotal = caReelLocal + fobJour;
        
        // La trésorerie a déjà été mise à jour par base.SimulerJournee pour CA local
        // On rajoute le net de l'exportation
        double netExport = fobJour - redevanceStat - taxeExport;
        Tresorerie += netExport;
        ChiffreAffairesCumule += fobJour;
        ChargesCumulees += redevanceStat + taxeExport;

        // Rapatriement devises (on considère que 100% de la valeur FOB est rapatriée en MGA)
        TotalDevisesRapatriees += fobJour / TauxChangeMGA;

        // Mapper vers DailyExporterResult
        result.VentesB2C = baseResult.VentesB2C;
        result.VentesB2B = baseResult.VentesB2B;
        result.ChargesSalariales = baseResult.ChargesSalariales;
        result.CotisationsCNaPS = baseResult.CotisationsCNaPS;
        result.AchatsB2B = baseResult.AchatsB2B;
        result.DepensesElectricite = baseResult.DepensesElectricite;
        result.TVACollectee = baseResult.TVACollectee;
        result.ImpotIS = baseResult.ImpotIS;
        result.BeneficeAvantImpot = baseResult.BeneficeAvantImpot;
        result.FluxNetJour = baseResult.FluxNetJour;
        result.CoutFinancement = baseResult.CoutFinancement;
        result.ValeurAjoutee = baseResult.ValeurAjoutee;
        result.Tresorerie = Tresorerie;

        return result;
    }

    private double CoefficientTaxeExportParCategorie()
    {
        return Categorie switch
        {
            ECategorieExport.Vanille => 1.5,      // Fortement taxé
            ECategorieExport.Girofle => 1.2,
            ECategorieExport.ProduitsMiniers => 1.0,
            ECategorieExport.ZonesFranches => 0.5, // Zone franche (taxe faible/nulle)
            _ => 1.0
        };
    }
}
