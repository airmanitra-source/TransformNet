namespace Simulation.Module.Models;

/// <summary>
/// Agrégats macroéconomiques scrapés depuis des sources publiques (Banque Mondiale, FMI, BCM).
/// Utilisés pour recalibrer automatiquement les paramètres de la simulation
/// afin que la trajectoire ne dérive pas des fondamentaux observés.
///
/// Toutes les valeurs monétaires sont en unités nationales sauf mention contraire.
/// Les taux sont exprimés en fraction (0.08 = 8%).
/// </summary>
public class MacroeconomicScrapedData
{
    /// <summary>Code pays ISO 3166-1 alpha-3 (ex: "MDG" pour Madagascar).</summary>
    public string CodePays { get; set; } = "MDG";

    /// <summary>Année de référence des données collectées.</summary>
    public int Annee { get; set; }

    /// <summary>Date/heure de la dernière collecte réussie (UTC).</summary>
    public DateTime DateCollecte { get; set; }

    /// <summary>Sources utilisées lors de la collecte (ex: "World Bank WDI, IMF WEO").</summary>
    public List<string> Sources { get; set; } = [];

    // ═══════════════════════════════════════════
    //  PRODUCTION (PIB)
    // ═══════════════════════════════════════════

    /// <summary>PIB nominal annuel (monnaie locale, LCU). World Bank: NY.GDP.MKTP.CN</summary>
    public double? PIBNominalAnnuelLCU { get; set; }

    /// <summary>PIB nominal annuel (USD courants). World Bank: NY.GDP.MKTP.CD</summary>
    public double? PIBNominalAnnuelUSD { get; set; }

    /// <summary>Croissance du PIB réel (%). World Bank: NY.GDP.MKTP.KD.ZG</summary>
    public double? CroissancePIBReel { get; set; }

    /// <summary>PIB par habitant (LCU). World Bank: NY.GDP.PCAP.CN</summary>
    public double? PIBParHabitantLCU { get; set; }

    // ═══════════════════════════════════════════
    //  PRIX ET INFLATION
    // ═══════════════════════════════════════════

    /// <summary>Inflation IPC annuelle (%). World Bank: FP.CPI.TOTL.ZG</summary>
    public double? TauxInflationAnnuel { get; set; }

    // ═══════════════════════════════════════════
    //  COMMERCE EXTÉRIEUR
    // ═══════════════════════════════════════════

    /// <summary>Exportations de biens et services (USD courants). World Bank: NE.EXP.GNFS.CD</summary>
    public double? ExportationsUSD { get; set; }

    /// <summary>Importations de biens et services (USD courants). World Bank: NE.IMP.GNFS.CD</summary>
    public double? ImportationsUSD { get; set; }

    /// <summary>Balance commerciale calculée (USD). Exports - Imports.</summary>
    public double? BalanceCommercialeUSD => ExportationsUSD.HasValue && ImportationsUSD.HasValue
        ? ExportationsUSD.Value - ImportationsUSD.Value
        : null;

    // ═══════════════════════════════════════════
    //  MONÉTAIRE ET TAUX DE CHANGE
    // ═══════════════════════════════════════════

    /// <summary>Masse monétaire M2 / PIB (%). World Bank: FM.LBL.BMNY.GD.ZS</summary>
    public double? RatioM2PIB { get; set; }

    /// <summary>Taux de change officiel (LCU par USD). World Bank: PA.NUS.FCRF</summary>
    public double? TauxChangeOfficielLCUParUSD { get; set; }

    /// <summary>Réserves totales incluant or (USD courants). World Bank: FI.RES.TOTL.CD</summary>
    public double? ReservesTotalesUSD { get; set; }

    /// <summary>Transferts personnels reçus (USD courants). World Bank: BX.TRF.PWKR.CD.DT</summary>
    public double? RemittancesRecuesUSD { get; set; }

    /// <summary>Recettes du tourisme international (USD courants). World Bank: ST.INT.RCPT.CD</summary>
    public double? RecettesTouristiquesUSD { get; set; }

    // ═══════════════════════════════════════════
    //  FINANCES PUBLIQUES
    // ═══════════════════════════════════════════

    /// <summary>Recettes fiscales (% du PIB). World Bank: GC.TAX.TOTL.GD.ZS</summary>
    public double? PressionFiscalePIB { get; set; }

    /// <summary>Dette publique totale (% du PIB). IMF WEO: GGXWDG_NGDP</summary>
    public double? DettePubliquePIB { get; set; }

    // ═══════════════════════════════════════════
    //  POPULATION ET EMPLOI
    // ═══════════════════════════════════════════

    /// <summary>Population totale. World Bank: SP.POP.TOTL</summary>
    public double? PopulationTotale { get; set; }

    /// <summary>Croissance démographique (%). World Bank: SP.POP.GROW</summary>
    public double? CroissanceDemographique { get; set; }

    /// <summary>Population urbaine (% du total). World Bank: SP.URB.TOTL.IN.ZS</summary>
    public double? PartPopulationUrbaine { get; set; }

    /// <summary>Emploi dans l'agriculture (% de l'emploi total). World Bank: SL.AGR.EMPL.ZS</summary>
    public double? PartEmploiAgriculture { get; set; }

    // ═══════════════════════════════════════════
    //  ÉNERGIE
    // ═══════════════════════════════════════════

    /// <summary>Accès à l'électricité (% de la population). World Bank: EG.ELC.ACCS.ZS</summary>
    public double? TauxAccesElectricite { get; set; }

    // ═══════════════════════════════════════════
    //  AIDE ET DÉVELOPPEMENT
    // ═══════════════════════════════════════════

    /// <summary>Aide publique au développement nette reçue (USD). World Bank: DT.ODA.ODAT.CD</summary>
    public double? AidePubliqueDevUSD { get; set; }

    // ═══════════════════════════════════════════
    //  ERREURS DE COLLECTE
    // ═══════════════════════════════════════════

    /// <summary>Liste des indicateurs qui n'ont pas pu être récupérés, avec la raison.</summary>
    public List<string> Erreurs { get; set; } = [];

    /// <summary>Indique si au moins les indicateurs critiques (PIB, inflation, change) sont disponibles.</summary>
    public bool EstExploitable =>
        PIBNominalAnnuelLCU.HasValue
        || TauxInflationAnnuel.HasValue
        || TauxChangeOfficielLCUParUSD.HasValue;
}
