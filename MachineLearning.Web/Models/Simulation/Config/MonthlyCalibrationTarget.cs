namespace MachineLearning.Web.Models.Simulation.Config;

/// <summary>
/// Cible de recalibration mensuelle basée sur des données macroéconomiques observées.
/// À chaque fin de mois simulé, le moteur compare les valeurs simulées aux cibles
/// et ajuste les paramètres internes pour converger vers la réalité.
///
/// Sources fiables pour Madagascar :
///   - M3 : Banque Centrale de Madagascar (BCM), bulletin mensuel
///   - Recettes fiscales : Direction Générale des Impôts (DGI), TOFE mensuel
///   - Imports/Exports : INSTAT, statistiques du commerce extérieur (Tableaux 32-33)
///   - Devises tourisme : BCM, balance des paiements / Ministère du Tourisme
///   - Affiliés CNaPS : Caisse Nationale de Prévoyance Sociale, rapport mensuel
/// </summary>
public class MonthlyCalibrationTarget
{
    /// <summary>Mois (1 = premier mois de simulation, 2 = deuxième, etc.)</summary>
    public int Mois { get; set; }

    /// <summary>Label du mois (ex: "Janvier 2025").</summary>
    public string Label { get; set; } = "";

    // ═══════════════════════════════════════════
    //  CIBLES MACRO (valeurs cumulées depuis le début de la simulation)
    // ═══════════════════════════════════════════

    /// <summary>
    /// Masse monétaire M3 observée à la fin du mois (MGA).
    /// Source : BCM, bulletin mensuel des statistiques monétaires.
    /// null = pas de cible pour ce mois.
    /// </summary>
    public double? M3Cible { get; set; }

    /// <summary>
    /// Recettes fiscales cumulées observées à la fin du mois (MGA).
    /// Inclut IR + IS + TVA + Douanes (TOFE).
    /// Source : Direction Générale des Impôts, TOFE mensuel.
    /// </summary>
    public double? RecettesFiscalesCumuleesCible { get; set; }

    /// <summary>
    /// Importations CIF cumulées observées à la fin du mois (MGA).
    /// Source : INSTAT, Tableau 33 — Importations par catégorie.
    /// </summary>
    public double? ImportationsCIFCumuleesCible { get; set; }

    /// <summary>
    /// Exportations FOB cumulées observées à la fin du mois (MGA).
    /// Source : INSTAT, Tableau 32 — Exportations par catégorie.
    /// </summary>
    public double? ExportationsFOBCumuleesCible { get; set; }

    // ═══════════════════════════════════════════
    //  TOURISME & EMPLOI FORMEL
    // ═══════════════════════════════════════════

    /// <summary>
    /// Apport en devises des visiteurs non-résidents pour ce mois (MGA).
    /// Correspond aux recettes touristiques (dépenses des visiteurs étrangers
    /// sur le territoire malgache, converties en MGA).
    /// Source : BCM balance des paiements / Ministère du Tourisme / INSTAT.
    /// Levier : ProductiviteParEmployeJour des entreprises HôtellerieTourisme.
    /// </summary>
    public double? DevisesTourismeCible { get; set; }

    /// <summary>
    /// Nombre de travailleurs nouvellement affiliés à la CNaPS ce mois.
    /// Reflète la croissance de l'emploi formel (entreprises déclarées).
    /// Source : CNaPS, rapport mensuel d'affiliation.
    /// Levier : transition EstInformel → formel sur les entreprises borderline.
    /// </summary>
    public int? NouveauxAffiliesCNaPSCible { get; set; }
}

/// <summary>
/// Journal d'un événement de recalibration appliqué pendant la simulation.
/// </summary>
public class CalibrationEvent
{
    /// <summary>Jour de simulation où la recalibration a eu lieu.</summary>
    public int Jour { get; set; }

    /// <summary>Mois de calibration (1, 2, 3...).</summary>
    public int Mois { get; set; }

    /// <summary>Corrections appliquées.</summary>
    public List<CalibrationAdjustment> Ajustements { get; set; } = [];
}

/// <summary>
/// Un ajustement individuel appliqué lors d'une recalibration.
/// </summary>
public class CalibrationAdjustment
{
    /// <summary>Nom du paramètre ajusté (ex: "CroissanceCreditJour").</summary>
    public string Parametre { get; set; } = "";

    /// <summary>Valeur avant ajustement.</summary>
    public double AncienneValeur { get; set; }

    /// <summary>Valeur après ajustement.</summary>
    public double NouvelleValeur { get; set; }

    /// <summary>Valeur simulée comparée à la cible.</summary>
    public double ValeurSimulee { get; set; }

    /// <summary>Valeur cible observée.</summary>
    public double ValeurCible { get; set; }

    /// <summary>Écart en % : (simulé - cible) / cible × 100.</summary>
    public double EcartPourcent => ValeurCible != 0
        ? (ValeurSimulee - ValeurCible) / ValeurCible * 100.0
        : 0;
}
