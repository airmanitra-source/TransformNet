namespace Simulation.Module.Models;

/// <summary>
/// Données macroéconomiques de référence pour Madagascar.
/// Utilisées par le moteur de validation pour comparer les résultats simulés
/// aux grandeurs réelles observées (INSTAT, BCM, FMI, Banque Mondiale).
///
/// Toutes les valeurs monétaires sont en MGA (Ariary malgache).
/// Les valeurs annuelles sont à l'échelle nationale (6M ménages, ~28M habitants).
///
/// Sources principales :
///   - PIB : INSTAT SCN / FMI Article IV / Banque Mondiale WDI
///   - Inflation : INSTAT IPC mensuel / BCM
///   - Commerce extérieur : INSTAT Tableaux 32-33
///   - M3 : BCM bulletin monétaire mensuel
///   - Recettes fiscales : DGI/TOFE mensuel
///   - Gini : Banque Mondiale / INSTAT EPM
///   - Emploi : INSTAT ENEMPSI / CNaPS
/// </summary>
public class MacroReferenceData
{
    /// <summary>Année de référence (ex: 2024).</summary>
    public int Annee { get; set; } = 2024;

    /// <summary>Source des données (ex: "INSTAT/BCM/FMI 2024").</summary>
    public string Source { get; set; } = "INSTAT/BCM/FMI 2024";

    // ═══════════════════════════════════════════
    //  PRODUCTION (PIB)
    // ═══════════════════════════════════════════

    /// <summary>PIB nominal annuel (MGA). FMI 2024 ≈ 72 000 Mds MGA.</summary>
    public double PIBNominalAnnuel { get; set; } = 72_000_000_000_000;

    /// <summary>Croissance PIB réel (%). FMI 2024 ≈ 4.0-4.5%.</summary>
    public double CroissancePIBReel { get; set; } = 0.042;

    /// <summary>PIB par habitant (MGA). ≈ 2 500 000 MGA (~$500).</summary>
    public double PIBParHabitant { get; set; } = 2_500_000;

    // ═══════════════════════════════════════════
    //  STRUCTURE PIB (parts sectorielles)
    // ═══════════════════════════════════════════

    /// <summary>Part agriculture dans le PIB (%). INSTAT ≈ 24-26%.</summary>
    public double PartAgriculturePIB { get; set; } = 0.25;

    /// <summary>Part industrie dans le PIB (%). INSTAT ≈ 15-18%.</summary>
    public double PartIndustriePIB { get; set; } = 0.16;

    /// <summary>Part services dans le PIB (%). INSTAT ≈ 50-55%.</summary>
    public double PartServicesPIB { get; set; } = 0.53;

    /// <summary>Part mines dans le PIB (%). ≈ 3-5%.</summary>
    public double PartMinesPIB { get; set; } = 0.04;

    // ═══════════════════════════════════════════
    //  PRIX ET INFLATION
    // ═══════════════════════════════════════════

    /// <summary>Taux d'inflation annuel IPC (%). INSTAT 2024 ≈ 7-9%.</summary>
    public double TauxInflationAnnuel { get; set; } = 0.08;

    /// <summary>Prix du riz local moyen (MGA/kg). INSTAT ≈ 2 200-2 600.</summary>
    public double PrixRizLocalKg { get; set; } = 2_400;

    // ═══════════════════════════════════════════
    //  COMMERCE EXTÉRIEUR
    // ═══════════════════════════════════════════

    /// <summary>Exportations FOB annuelles (MGA). INSTAT ≈ 5 400 Mds.</summary>
    public double ExportationsFOBAnnuelles { get; set; } = 5_400_000_000_000;

    /// <summary>Importations CIF annuelles (MGA). INSTAT ≈ 9 600 Mds.</summary>
    public double ImportationsCIFAnnuelles { get; set; } = 9_600_000_000_000;

    /// <summary>Balance commerciale annuelle (MGA). ≈ -4 200 Mds (déficitaire).</summary>
    public double BalanceCommercialeAnnuelle => ExportationsFOBAnnuelles - ImportationsCIFAnnuelles;

    // ═══════════════════════════════════════════
    //  MONÉTAIRE
    // ═══════════════════════════════════════════

    /// <summary>Masse monétaire M3 (MGA). BCM fin 2024 ≈ 28 000-32 000 Mds.</summary>
    public double MasseMonetaireM3 { get; set; } = 30_000_000_000_000;

    /// <summary>Croissance M3 annuelle (%). BCM ≈ 12-16%.</summary>
    public double CroissanceM3Annuelle { get; set; } = 0.14;

    /// <summary>Taux directeur BCM (%). ≈ 9-11%.</summary>
    public double TauxDirecteur { get; set; } = 0.095;

    // ═══════════════════════════════════════════
    //  FINANCES PUBLIQUES (TOFE)
    // ═══════════════════════════════════════════

    /// <summary>Recettes fiscales annuelles totales (MGA). DGI/TOFE ≈ 7 200 Mds.</summary>
    public double RecettesFiscalesAnnuelles { get; set; } = 7_200_000_000_000;

    /// <summary>Pression fiscale (recettes / PIB). Madagascar ≈ 10-12%.</summary>
    public double PressionFiscale { get; set; } = 0.10;

    /// <summary>Dépenses publiques annuelles (MGA). TOFE ≈ 9 500 Mds.</summary>
    public double DepensesPubliquesAnnuelles { get; set; } = 9_500_000_000_000;

    /// <summary>Solde budgétaire / PIB (%). ≈ -3% à -5%.</summary>
    public double SoldeBudgetairePIB { get; set; } = -0.035;

    /// <summary>Dette publique / PIB (%). FMI ≈ 40-45%.</summary>
    public double DettePubliquePIB { get; set; } = 0.42;

    // ═══════════════════════════════════════════
    //  INÉGALITÉS ET EMPLOI
    // ═══════════════════════════════════════════

    /// <summary>Coefficient de Gini. Banque Mondiale ≈ 0.42-0.44.</summary>
    public double Gini { get; set; } = 0.427;

    /// <summary>Ratio D9/D1 (revenu décile 9 / décile 1). ≈ 12-15.</summary>
    public double RatioD9D1 { get; set; } = 13.5;

    /// <summary>Part du secteur informel dans l'emploi (%). INSTAT ≈ 80-90%.</summary>
    public double PartSecteurInformel { get; set; } = 0.85;

    /// <summary>Taux d'emploi formel (affiliés CNaPS / population active). ≈ 5-8%.</summary>
    public double TauxEmploiFormel { get; set; } = 0.06;

    /// <summary>Nombre total d'affiliés CNaPS. ≈ 750 000-900 000.</summary>
    public int AffiliésCNaPS { get; set; } = 820_000;

    // ═══════════════════════════════════════════
    //  ÉNERGIE (Jirama)
    // ═══════════════════════════════════════════

    /// <summary>Production électrique annuelle (GWh). Jirama ≈ 1 800-2 000 GWh.</summary>
    public double ProductionElecAnnuelleGWh { get; set; } = 1_900;

    /// <summary>Taux de pertes de distribution (%). Jirama ≈ 25-30%.</summary>
    public double TauxPertesDistribution { get; set; } = 0.28;

    /// <summary>Taux d'accès à l'électricité (%). ≈ 28-33%.</summary>
    public double TauxAccesElectricite { get; set; } = 0.30;

    // ═══════════════════════════════════════════
    //  DÉMOGRAPHIE
    // ═══════════════════════════════════════════

    /// <summary>Population totale. INSTAT 2024 ≈ 30M.</summary>
    public int Population { get; set; } = 30_000_000;

    /// <summary>Nombre de ménages. ≈ 6 000 000.</summary>
    public int NombreMenages { get; set; } = 6_000_000;

    /// <summary>Taille moyenne des ménages. ≈ 4.5-5.</summary>
    public double TailleMoyenneMenage { get; set; } = 4.8;

    // ═══════════════════════════════════════════
    //  TAUX DE CHANGE
    // ═══════════════════════════════════════════

    /// <summary>Taux de change MGA/USD fin d'année. BCM 2024 ≈ 4 500.</summary>
    public double TauxChangeMGAParUSD { get; set; } = 4_500;

    /// <summary>Dépréciation annuelle du MGA (%). Historique ≈ 5-7%/an.</summary>
    public double DepreciationAnnuelleMGA { get; set; } = 0.06;

    /// <summary>Réserves de change BCM (USD). BCM 2024 ≈ 2.5 Mds.</summary>
    public double ReservesBCMUSD { get; set; } = 2_500_000_000;

    /// <summary>Réserves en mois d'imports. BCM 2024 ≈ 5 mois.</summary>
    public double ReservesMoisImports { get; set; } = 5.0;

    /// <summary>
    /// Crée les données de référence par défaut pour Madagascar 2024.
    /// </summary>
    public static MacroReferenceData Madagascar2024() => new();
}
