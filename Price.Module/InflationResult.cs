namespace Price.Module;

/// <summary>
/// Résultat détaillé du calcul d'inflation endogène pour un jour donné.
/// Permet le suivi et l'analyse des contributions de chaque composante.
/// </summary>
public class InflationResult
{
    /// <summary>
    /// Taux d'inflation annualisé résultant (ex: 0.08 = 8%/an).
    /// C'est cette valeur qui remplace l'ancien paramètre statique TauxInflation.
    /// </summary>
    public double TauxInflationAnnuel { get; set; }

    /// <summary>
    /// Taux d'inflation journalier (TauxInflationAnnuel / 365).
    /// Utilisé directement dans les calculs de facteurInflationJour.
    /// </summary>
    public double TauxInflationJournalier { get; set; }

    // ═══════════════════════════════════════════
    //  COMPOSANTES DÉCOMPOSÉES
    // ═══════════════════════════════════════════

    /// <summary>
    /// Composante demand-pull (Phillips) : pression inflationniste due à l'excès de demande.
    /// Positif = économie en surchauffe, négatif = sous-emploi désinflation.
    /// </summary>
    public double ComposanteDemandPull { get; set; }

    /// <summary>
    /// Composante cost-push : inflation importée via carburant et prix des importations.
    /// Toujours ≥ 0 si les prix montent.
    /// </summary>
    public double ComposanteCostPush { get; set; }

    /// <summary>
    /// Composante monétaire : excès de création monétaire par rapport à la croissance réelle.
    /// Positif = expansion monétaire inflationniste.
    /// </summary>
    public double ComposanteMonetaire { get; set; }

    /// <summary>
    /// Composante anticipations : inertie inflationniste (inflation passée + ancrage).
    /// </summary>
    public double ComposanteAnticipations { get; set; }

    // ═══════════════════════════════════════════
    //  INDICATEURS DIAGNOSTIQUES
    // ═══════════════════════════════════════════

    /// <summary>
    /// Output gap = (PIB_effectif - PIB_potentiel) / PIB_potentiel.
    /// Positif = surchauffe, négatif = sous-utilisation des capacités.
    /// </summary>
    public double OutputGap { get; set; }

    /// <summary>
    /// Écart de chômage = taux_chômage - NAIRU.
    /// Positif = chômage excédentaire (désinflation), négatif = surchauffe.
    /// </summary>
    public double EcartChomage { get; set; }

    /// <summary>
    /// Croissance de la masse monétaire M3 (annualisée, ex: 0.15 = +15%).
    /// </summary>
    public double CroissanceM3 { get; set; }

    /// <summary>
    /// Croissance du PIB réel (annualisée, estimée).
    /// </summary>
    public double CroissancePIB { get; set; }

    /// <summary>
    /// Anticipations d'inflation (annualisées) utilisées dans le calcul.
    /// </summary>
    public double AnticipationsInflation { get; set; }
}
