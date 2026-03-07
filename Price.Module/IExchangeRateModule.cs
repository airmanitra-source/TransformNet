namespace Price.Module;

/// <summary>
/// Module de taux de change dynamique MGA/USD pour la simulation économique de Madagascar.
///
/// Le taux de change est un canal majeur de transmission des chocs externes vers
/// l'économie malgache. L'Ariary (MGA) est un régime de flottement géré :
/// la BCM intervient sur le marché interbancaire des devises (MID) pour lisser
/// la volatilité, sans cibler un taux fixe.
///
/// ══════════════════════════════════════════════════════════════
///  DÉTERMINANTS DU TAUX MGA/USD — MODÈLE STRUCTUREL
/// ══════════════════════════════════════════════════════════════
///
///  1. BALANCE COMMERCIALE (flux courant)
///     Déficit commercial → demande nette de devises → dépréciation MGA
///     Madagascar : déficit structurel ~4 200 Mds MGA/an (INSTAT)
///
///  2. RÉSERVES DE CHANGE BCM
///     Stock de devises BCM → capacité d'intervention → stabilisation
///     Madagascar : ~2.5 Mds USD (≈5 mois d'imports), BCM 2024
///
///  3. DIFFÉRENTIEL D'INFLATION
///     PPA relative : Δe/e ≈ π_domestique - π_étranger
///     Si inflation MGA (8%) > inflation USD (3%) → dépréciation ~5%/an
///
///  4. FLUX DE CAPITAUX
///     Aide internationale + remittances + IDE → offre de devises → appréciation
///     Madagascar : remittances ~700M USD/an, aide ~600M USD/an
///
///  5. INTERVENTIONS BCM (lissage)
///     La BCM vend des devises pour freiner la dépréciation
///     Bande de tolérance implicite ~±2% par mois
///
///  Tendance historique :
///     MGA/USD 2018: 3 400 → 2020: 3 900 → 2022: 4 200 → 2024: 4 500
///     Dépréciation tendancielle ~5-7%/an
/// ══════════════════════════════════════════════════════════════
/// </summary>
public interface IExchangeRateModule
{
    /// <summary>
    /// Initialise le module avec le taux de change de départ et les réserves BCM.
    /// Appelé une fois au début de chaque simulation.
    /// </summary>
    void Initialiser(double tauxInitialMGAParUSD, double reservesBCMUSD);

    /// <summary>
    /// Calcule le taux de change du jour à partir des flux de devises.
    /// Appelé une fois par jour après les échanges import/export.
    /// </summary>
    ExchangeRateResult CalculerTauxChange(ExchangeRateContext context);
}

/// <summary>
/// Contexte de flux de devises pour le calcul journalier du taux de change.
/// </summary>
public class ExchangeRateContext
{
    // ═══════════════════════════════════════════
    //  FLUX DE DEVISES DU JOUR (en MGA)
    // ═══════════════════════════════════════════

    /// <summary>Exportations FOB du jour (en MGA). Génère une offre de devises.</summary>
    public double ExportationsFOBJour { get; set; }

    /// <summary>Importations CIF du jour (en MGA). Génère une demande de devises.</summary>
    public double ImportationsCIFJour { get; set; }

    /// <summary>Remittances du jour (en MGA, déjà converties). Offre de devises.</summary>
    public double RemittancesJour { get; set; }

    /// <summary>Aide internationale du jour (en MGA, déjà convertie). Offre de devises.</summary>
    public double AideInternationaleJour { get; set; }

    // ═══════════════════════════════════════════
    //  INFLATION
    // ═══════════════════════════════════════════

    /// <summary>Taux d'inflation annualisé domestique (MGA). Ex: 0.08 = 8%.</summary>
    public double InflationDomestique { get; set; } = 0.08;

    /// <summary>Taux d'inflation étrangère (USD). Ex: 0.03 = 3%.</summary>
    public double InflationEtrangere { get; set; } = 0.03;

    // ═══════════════════════════════════════════
    //  PARAMÈTRES DE CALIBRAGE
    // ═══════════════════════════════════════════

    /// <summary>
    /// Élasticité du taux de change à la balance commerciale (0-1).
    /// 0.3 = faible réactivité (intervention BCM forte)
    /// 0.8 = forte réactivité (flottement libre)
    /// Madagascar ≈ 0.4-0.6 (flottement géré)
    /// </summary>
    public double ElasticiteBalanceCommerciale { get; set; } = 0.50;

    /// <summary>
    /// Poids de la PPA relative (différentiel d'inflation) dans le taux de change.
    /// Madagascar : ~0.3 (convergence lente vers la PPA)
    /// </summary>
    public double PoidsPPA { get; set; } = 0.30;

    /// <summary>
    /// Intensité d'intervention de la BCM (0 = aucune, 1 = fixité totale).
    /// 0.5 = intervention modérée (corridor implicite ±2%/mois).
    /// La BCM utilise ses réserves pour freiner la dépréciation.
    /// </summary>
    public double IntensiteInterventionBCM { get; set; } = 0.50;

    /// <summary>
    /// Réserves minimales BCM (en mois d'imports). En dessous, l'intervention cesse.
    /// FMI recommande 3 mois minimum. Madagascar vise 5 mois.
    /// </summary>
    public double ReservesMinimalesMoisImports { get; set; } = 3.0;

    /// <summary>
    /// Tendance de dépréciation annuelle structurelle (ex: 0.05 = 5%/an).
    /// Reflète le différentiel de productivité et l'inflation structurelle.
    /// </summary>
    public double DepreciationStructurelleAnnuelle { get; set; } = 0.05;
}

/// <summary>
/// Résultat du calcul de taux de change pour un jour donné.
/// </summary>
public class ExchangeRateResult
{
    /// <summary>Taux de change MGA par USD (ex: 4500 = 1 USD = 4500 MGA).</summary>
    public double TauxMGAParUSD { get; set; }

    /// <summary>Variation journalière du taux (ex: 0.0002 = +0.02%/jour).</summary>
    public double VariationJournaliere { get; set; }

    /// <summary>Variation annualisée du taux (ex: 0.07 = +7%/an dépréciation).</summary>
    public double DepreciationAnnualisee { get; set; }

    /// <summary>Réserves de change BCM (en USD).</summary>
    public double ReservesBCMUSD { get; set; }

    /// <summary>Réserves BCM en mois d'importations.</summary>
    public double ReservesMoisImports { get; set; }

    /// <summary>Solde net de devises du jour (positif = excédent, négatif = déficit).</summary>
    public double SoldeDevisesJourUSD { get; set; }

    /// <summary>Montant d'intervention BCM (vente de devises, USD). Positif = intervention.</summary>
    public double InterventionBCMUSD { get; set; }

    /// <summary>
    /// Composante balance commerciale de la pression sur le change.
    /// Positif = pression dépréciative (déficit).
    /// </summary>
    public double PressionBalanceCommerciale { get; set; }

    /// <summary>
    /// Composante PPA (différentiel d'inflation).
    /// Positif = pression dépréciative (inflation MGA > USD).
    /// </summary>
    public double PressionPPA { get; set; }

    /// <summary>
    /// Indice de pression sur le change (agrégé, -1 à +1).
    /// Positif = pression dépréciative, négatif = appréciation.
    /// </summary>
    public double IndicePression { get; set; }
}
