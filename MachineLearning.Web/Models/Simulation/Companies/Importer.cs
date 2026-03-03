namespace MachineLearning.Web.Models.Simulation.Companies;

/// <summary>
/// Entreprise importatrice à Madagascar.
/// Importe des marchandises, paie les droits de douane, droits d'accise et TVA import.
///
/// Réalités douanières Madagascar (DGD - Direction Générale des Douanes) :
/// - Droits de douane (DD) : 0% à 20% selon catégorie (moyenne pondérée ~12%)
/// - Droits d'accise : 5% à 60% (alcool, tabac, carburant, véhicules)
/// - TVA à l'importation : 20% sur (CIF + DD + Accise)
/// - Redevance statistique : 2% sur CIF
///
/// Principaux produits importés :
/// - Carburant/pétrole (~25% des imports) : DD 0%, Accise 30-60%
/// - Électronique/machines (~15%) : DD 5-10%, Accise 0%
/// - Véhicules (~10%) : DD 10-20%, Accise 5-15%
/// - Alimentaire (riz, huile) (~20%) : DD 5-20%, Accise 0-5%
/// - Textile/habillement (~8%) : DD 20%, Accise 0%
///
/// Source : Tarif des Douanes de Madagascar, INSTAT, BCM
/// </summary>
public class Importer : Company
{
    /// <summary>Catégorie principale d'importation</summary>
    public CategorieImport Categorie { get; set; } = CategorieImport.BienConsommation;

    /// <summary>Valeur CIF journalière des importations (en MGA)</summary>
    public double ValeurCIFJour { get; set; } = 500_000;

    /// <summary>Part du chiffre d'affaires provenant de la revente de produits importés</summary>
    public double PartReventeImport { get; set; } = 0.60;

    /// <summary>Marge sur la revente des produits importés</summary>
    public double MargeReventeImport { get; set; } = 0.25;

    // --- Cumuls ---
    public double TotalImportationsCIF { get; set; }
    public double TotalDroitsDouane { get; set; }
    public double TotalAccise { get; set; }
    public double TotalTVAImport { get; set; }
    public double TotalRedevanceStatistique { get; set; }

    /// <summary>
    /// Simule une journée pour un importateur :
    /// 1. Importation de marchandises (valeur CIF)
    /// 2. Paiement droits de douane, accise, TVA import, redevance
    /// 3. Revente sur le marché local (ventes B2C/B2B)
    /// 4. Charges + IS comme une entreprise normale
    /// </summary>
    public DailyImporterResult SimulerJourneeImport(
        double demandeConsommationMenages,
        double tauxIS,
        double tauxTVA,
        double tauxInflation,
        double tauxDirecteur,
        double tauxDroitsDouane,
        double tauxAccise)
    {
        var result = new DailyImporterResult();

        // 1. Importation CIF du jour (ajustée par l'inflation = coût de change)
        double facteurInflation = 1.0 + (tauxInflation / 365.0);
        double cifJour = ValeurCIFJour * facteurInflation;
        result.ValeurCIF = cifJour;
        TotalImportationsCIF += cifJour;

        // 2. Droits de douane sur la valeur CIF
        double droitsDouaneTaux = tauxDroitsDouane * CoefficientDDParCategorie();
        double droitsDouane = cifJour * droitsDouaneTaux;
        result.DroitsDouane = droitsDouane;
        TotalDroitsDouane += droitsDouane;

        // 3. Droits d'accise (appliqués sur CIF + DD)
        double acciseTaux = tauxAccise * CoefficientAcciseParCategorie();
        double baseAccise = cifJour + droitsDouane;
        double accise = baseAccise * acciseTaux;
        result.Accise = accise;
        TotalAccise += accise;

        // 4. TVA à l'importation (sur CIF + DD + Accise)
        double baseTVAImport = cifJour + droitsDouane + accise;
        double tvaImport = baseTVAImport * tauxTVA;
        result.TVAImport = tvaImport;
        TotalTVAImport += tvaImport;

        // 5. Redevance statistique (2% sur CIF)
        double redevance = cifJour * 0.02;
        result.RedevanceStatistique = redevance;
        TotalRedevanceStatistique += redevance;

        // 6. Coût total d'importation
        double coutTotalImport = cifJour + droitsDouane + accise + tvaImport + redevance;
        result.CoutTotalImport = coutTotalImport;

        // 7. Simulation entreprise classique (production + ventes locales)
        var baseResult = SimulerJournee(
            demandeConsommationMenages,
            tauxIS, tauxTVA, tauxInflation, tauxDirecteur);

        // Copier les résultats de l'entreprise de base
        result.VentesB2C = baseResult.VentesB2C;
        result.VentesB2B = baseResult.VentesB2B;
        result.ChargesSalariales = baseResult.ChargesSalariales;
        result.AchatsB2B = baseResult.AchatsB2B;
        result.TVACollectee = baseResult.TVACollectee;
        result.ImpotIS = baseResult.ImpotIS;
        result.BeneficeAvantImpot = baseResult.BeneficeAvantImpot;
        result.FluxNetJour = baseResult.FluxNetJour;
        result.Tresorerie = baseResult.Tresorerie;
        result.CoutFinancement = baseResult.CoutFinancement;

        // 8. Ajuster la trésorerie pour le coût d'importation
        Tresorerie -= coutTotalImport;
        result.Tresorerie = Tresorerie;

        // 9. Revenu supplémentaire de la revente des imports (marge)
        double reventeJour = cifJour * (1.0 + MargeReventeImport);
        result.ReventeImport = reventeJour;
        Tresorerie += reventeJour;
        result.Tresorerie = Tresorerie;

        return result;
    }

    /// <summary>
    /// Coefficient multiplicateur des droits de douane selon la catégorie.
    /// Le taux final = tauxDroitsDouane (config) × coefficient.
    /// </summary>
    private double CoefficientDDParCategorie() => Categorie switch
    {
        CategorieImport.Carburant => 0.0,           // DD 0% sur le carburant
        CategorieImport.Alimentaire => 0.80,         // DD ~5-20%
        CategorieImport.Electronique => 0.50,        // DD ~5-10%
        CategorieImport.Vehicule => 1.20,            // DD ~10-20%
        CategorieImport.BienConsommation => 1.00,    // DD taux normal
        CategorieImport.MatierePremiere => 0.30,     // DD réduit
        _ => 1.00
    };

    /// <summary>
    /// Coefficient multiplicateur des accises selon la catégorie.
    /// </summary>
    private double CoefficientAcciseParCategorie() => Categorie switch
    {
        CategorieImport.Carburant => 3.00,           // Accise très élevée (30-60%)
        CategorieImport.Alimentaire => 0.25,         // Accise faible (0-5%)
        CategorieImport.Electronique => 0.00,        // Pas d'accise
        CategorieImport.Vehicule => 0.75,            // Accise moyenne (5-15%)
        CategorieImport.BienConsommation => 0.50,    // Accise modérée
        CategorieImport.MatierePremiere => 0.00,     // Pas d'accise
        _ => 0.50
    };
}

/// <summary>
/// Catégories d'importation avec profils fiscaux différents.
/// Calibré sur les statistiques douanières de Madagascar.
/// </summary>
public enum CategorieImport
{
    /// <summary>Carburant, pétrole, gaz (~25% des imports)</summary>
    Carburant,
    /// <summary>Riz, huile, blé, produits alimentaires (~20%)</summary>
    Alimentaire,
    /// <summary>Machines, électronique, téléphones (~15%)</summary>
    Electronique,
    /// <summary>Véhicules, pièces auto (~10%)</summary>
    Vehicule,
    /// <summary>Biens de consommation courante (~22%)</summary>
    BienConsommation,
    /// <summary>Matières premières industrielles (~8%)</summary>
    MatierePremiere
}

/// <summary>
/// Résultat journalier d'un importateur (hérite du résultat entreprise + données douanières).
/// </summary>
public class DailyImporterResult : DailyCompanyResult
{
    public double ValeurCIF { get; set; }
    public double DroitsDouane { get; set; }
    public double Accise { get; set; }
    public double TVAImport { get; set; }
    public double RedevanceStatistique { get; set; }
    public double CoutTotalImport { get; set; }
    public double ReventeImport { get; set; }

    /// <summary>Total recettes douanières générées par cet importateur ce jour</summary>
    public double RecettesDouanieres => DroitsDouane + Accise + TVAImport + RedevanceStatistique;
}
