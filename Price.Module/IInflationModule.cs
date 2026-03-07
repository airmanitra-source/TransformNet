namespace Price.Module;

/// <summary>
/// Module d'inflation endogène pour la simulation économique de Madagascar.
///
/// L'inflation n'est plus un paramètre fixe : elle émerge de l'interaction
/// entre l'offre agrégée, la demande agrégée, la masse monétaire et les chocs de coûts.
///
/// Composantes du modèle :
///   1. Demand-pull  (courbe de Phillips) : π_dp = -β × (u - u*)
///      Excès de demande → pression haussière sur les prix
///   2. Cost-push    (chocs d'offre) : π_cp = δ × Δcarburant/carburant_ref + ε × Δimport/import_ref
///      Chocs de coûts (carburant, importations) transmis aux prix
///   3. Monétaire    (théorie quantitative) : π_m = λ × (ΔM3/M3 - ΔPIB/PIB)
///      Croissance monétaire excédant la croissance réelle → inflation
///   4. Anticipations adaptatives : π_e(t) = α × π(t-1) + (1-α) × π_ancrage
///      L'inflation passée influence les anticipations futures (inertie)
///
/// Formule finale : π(t) = w_e×π_e + w_dp×π_dp + w_cp×π_cp + w_m×π_m
///
/// Calibrage Madagascar :
///   - NAIRU (u*) ≈ 15-20% (sous-emploi structurel)
///   - Inflation d'ancrage ≈ 5-7% (cible implicite BCM)
///   - Forte transmission des chocs carburant (économie très dépendante)
/// </summary>
public interface IInflationModule
{
    /// <summary>
    /// Calcule le taux d'inflation journalier endogène à partir de l'état courant de l'économie.
    /// Doit être appelé une fois par jour de simulation, après les flux ménages/entreprises/État.
    /// </summary>
    /// <param name="context">État macroéconomique du jour courant.</param>
    /// <returns>Résultat détaillé avec le taux d'inflation et ses composantes.</returns>
    InflationResult CalculerInflationJournaliere(InflationContext context);

    /// <summary>
    /// Réinitialise l'état interne du module (anticipations, historique).
    /// Appelé au début de chaque simulation.
    /// </summary>
    /// <param name="tauxInflationInitial">Taux d'inflation annuel initial (ex: 0.08 = 8%).</param>
    /// <param name="pibPotentielJour">PIB potentiel journalier estimé (MGA).</param>
    /// <param name="m3Initial">Masse monétaire M3 initiale (MGA).</param>
    void Initialiser(double tauxInflationInitial, double pibPotentielJour, double m3Initial);
}

/// <summary>
/// Contexte macroéconomique fourni au module d'inflation pour le calcul journalier.
/// Regroupe toutes les variables observées nécessaires au calcul.
/// </summary>
public class InflationContext
{
    // ═══════════════════════════════════════════
    //  MARCHÉ DU TRAVAIL (Phillips)
    // ═══════════════════════════════════════════

    /// <summary>Taux d'emploi courant (0-1). Taux de chômage = 1 - TauxEmploi.</summary>
    public double TauxEmploi { get; set; }

    // ═══════════════════════════════════════════
    //  PRODUCTION (Output gap)
    // ═══════════════════════════════════════════

    /// <summary>PIB journalier effectif (par la demande, MGA).</summary>
    public double PIBJourEffectif { get; set; }

    // ═══════════════════════════════════════════
    //  MONÉTAIRE
    // ═══════════════════════════════════════════

    /// <summary>Masse monétaire M3 courante (MGA).</summary>
    public double MasseMonetaireM3 { get; set; }

    // ═══════════════════════════════════════════
    //  CHOCS DE COÛTS (Cost-push)
    // ═══════════════════════════════════════════

    /// <summary>Prix du carburant courant (MGA/litre).</summary>
    public double PrixCarburantCourant { get; set; }

    /// <summary>Prix du carburant de référence (MGA/litre).</summary>
    public double PrixCarburantReference { get; set; }

    /// <summary>Importations CIF journalières (MGA).</summary>
    public double ImportationsCIFJour { get; set; }

    /// <summary>Importations CIF de référence journalières (MGA, baseline).</summary>
    public double ImportationsCIFReference { get; set; }

    // ═══════════════════════════════════════════
    //  TAUX DE CHANGE (canal d'inflation importée)
    // ═══════════════════════════════════════════

    /// <summary>
    /// Variation journalière du taux de change MGA/USD (ex: 0.0002 = +0.02%/jour).
    /// Positif = dépréciation MGA → renchérissement des imports → inflation.
    /// </summary>
    public double VariationChangeJournaliere { get; set; }

    /// <summary>
    /// Élasticité du taux de change vers l'inflation (pass-through, ζ).
    /// Part de la dépréciation transmise aux prix intérieurs.
    /// Madagascar ≈ 0.20-0.40 (pass-through modéré à élevé).
    /// </summary>
    public double ElasticiteChangeInflation { get; set; } = 0.30;

    // ═══════════════════════════════════════════
    //  PARAMÈTRES DE CALIBRAGE
    // ═══════════════════════════════════════════

    /// <summary>NAIRU : taux de chômage naturel (0-1). Madagascar ≈ 0.15-0.20.</summary>
    public double NAIRU { get; set; } = 0.18;

    /// <summary>Sensibilité Phillips : impact de l'écart de chômage sur l'inflation (β).</summary>
    public double CoefficientPhillips { get; set; } = 0.03;

    /// <summary>Poids des anticipations dans l'inflation totale (w_e).</summary>
    public double PoidsAnticipations { get; set; } = 0.40;

    /// <summary>Poids de la composante demand-pull Phillips (w_dp).</summary>
    public double PoidsDemandPull { get; set; } = 0.20;

    /// <summary>Poids de la composante cost-push (w_cp).</summary>
    public double PoidsCostPush { get; set; } = 0.25;

    /// <summary>Poids de la composante monétaire (w_m).</summary>
    public double PoidsMonetaire { get; set; } = 0.15;

    /// <summary>Élasticité du carburant vers l'inflation (δ). Madagascar ≈ 0.15-0.25.</summary>
    public double ElasticiteCarburantInflation { get; set; } = 0.20;

    /// <summary>Élasticité des importations vers l'inflation (ε). Madagascar ≈ 0.10-0.15.</summary>
    public double ElasticiteImportInflation { get; set; } = 0.12;

    /// <summary>Poids monétaire (λ). Part de l'excès monétaire transmis aux prix.</summary>
    public double CoefficientMonetaire { get; set; } = 0.30;

    /// <summary>Vitesse d'adaptation des anticipations (α). 0=ancrage fixe, 1=adaptatif pur.</summary>
    public double VitesseAdaptationAnticipations { get; set; } = 0.60;

    /// <summary>Taux d'inflation cible / ancrage (annuel). BCM cible implicite ≈ 5-7%.</summary>
    public double InflationAncrage { get; set; } = 0.06;

    /// <summary>Taux de croissance du PIB potentiel (annuel). Madagascar ≈ 4-5%.</summary>
    public double CroissancePIBPotentielAnnuel { get; set; } = 0.045;
}
