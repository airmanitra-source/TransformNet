using Company.Module;

namespace Company.Module.Models;

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
    public ECategorieImport Categorie { get; set; } = ECategorieImport.BienConsommation;

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
    /// 1. Importation de marchandises (valeur CIF) — les importateurs (Commerce) travaillent 7j/7
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
        double tauxAccise,
        ICompanyModule companyModule,
        bool estJourOuvrable = true,
        Jirama? Jirama = null,
        double consoElecParEmployeKWhJour = 0,
        double tauxCNaPSPatronale = 0,
        double prixCarburantCourant = 0,
        double prixCarburantReference = 0,
        double elasticitePrixParCarburant = 0)
    {
        var result = new DailyImporterResult();

        // Les importateurs (Commerce) travaillent 7j/7
        // mais si ce n'est pas un jour ouvrable et que le secteur n'est pas Commerce, pas d'activité
        if (!estJourOuvrable && SecteurActivite != ESecteurActivite.Commerces)
        {
            result.Tresorerie = Tresorerie;
            return result;
        }

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
        var baseResult = companyModule.SimulerJournee(
            this,
            demandeConsommationMenages,
            tauxIS, tauxTVA, tauxInflation, tauxDirecteur, estJourOuvrable,
            Jirama, consoElecParEmployeKWhJour, tauxCNaPSPatronale,
            prixCarburantCourant, prixCarburantReference, elasticitePrixParCarburant);

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
        result.ValeurAjoutee = baseResult.ValeurAjoutee;

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
    /// <remarks>
    /// SUPERSEDED : privilégier <c>ICompanyModule.GetCoefficientDroitsDouaneParCategorie()</c>
    /// injecté dans <c>EconomicSimulatorViewModel</c> pour centraliser les règles douanières.
    /// </remarks>
    public double CoefficientDDParCategorie() => Categorie switch
    {
        ECategorieImport.Carburant => 0.0,           // DD 0% sur le carburant
        ECategorieImport.Alimentaire => 0.80,         // DD ~5-20%
        ECategorieImport.Electronique => 0.50,        // DD ~5-10%
        ECategorieImport.Vehicule => 1.20,            // DD ~10-20%
        ECategorieImport.BienConsommation => 1.00,    // DD taux normal
        ECategorieImport.MatierePremiere => 0.30,     // DD réduit
        _ => 1.00
    };

    /// <summary>
    /// Coefficient multiplicateur des accises selon la catégorie.
    /// </summary>
    /// <remarks>
    /// SUPERSEDED : privilégier <c>ICompanyModule.GetCoefficientAcciseParCategorie()</c>
    /// injecté dans <c>EconomicSimulatorViewModel</c> pour centraliser les règles d'accise.
    /// </remarks>
    public double CoefficientAcciseParCategorie() => Categorie switch
    {
        ECategorieImport.Carburant => 3.00,           // Accise très élevée (30-60%)
        ECategorieImport.Alimentaire => 0.25,         // Accise faible (0-5%)
        ECategorieImport.Electronique => 0.00,        // Pas d'accise
        ECategorieImport.Vehicule => 0.75,            // Accise moyenne (5-15%)
        ECategorieImport.BienConsommation => 0.50,    // Accise modérée
        ECategorieImport.MatierePremiere => 0.00,     // Pas d'accise
        _ => 0.50
    };
}



