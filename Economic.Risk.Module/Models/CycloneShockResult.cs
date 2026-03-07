namespace Economic.Risk.Module.Models;

/// <summary>
/// Résultat du calcul de choc cyclonique pour un jour donné.
///
/// Madagascar subit en moyenne 1.5 cyclones tropicaux par an (BNGRC/Météo Madagascar).
/// Saison cyclonique : novembre à avril (pic : janvier-mars).
///
/// Impacts économiques documentés (BNGRC, Banque Mondiale, FMI) :
///   - Perte PIB : 1-3% par cyclone majeur (catégorie 3+)
///   - Destruction d'infrastructures : routes, ponts, bâtiments
///   - Perte agricole : 30-70% des récoltes dans les zones touchées
///   - Perturbation commerce : routes coupées 5-30 jours
///   - Hausse prix alimentaires : +15-40% post-cyclone
///   - MAIS : effet de reconstruction (BTP, quincaillerie, tôle, briques)
///     → boost du secteur construction +20-50% pendant 60-120 jours
///
/// Exemples historiques :
///   - Cyclone Enawo (2017) : 81 morts, 434 000 affectés, ~$400M de dégâts
///   - Cyclone Batsirai (2022) : 121 morts, 112 000 déplacés, ~$155M
///   - Cyclone Freddy (2023) : 17 morts à Madagascar, inondations majeures
/// </summary>
public class CycloneShockResult
{
    /// <summary>True si un cyclone frappe ce jour.</summary>
    public bool CycloneActif { get; set; }

    /// <summary>Intensité du cyclone (0-1). 0 = pas de cyclone, 1 = catégorie 5.</summary>
    public double Intensite { get; set; }

    /// <summary>Nom du cyclone (généré aléatoirement pour le suivi).</summary>
    public string NomCyclone { get; set; } = "";

    /// <summary>Jour depuis le début de l'événement cyclonique (1-based, 0 si pas de cyclone).</summary>
    public int JourDansCyclone { get; set; }

    /// <summary>Durée totale du cyclone en jours (3-7 pour un cyclone typique).</summary>
    public int DureeCycloneJours { get; set; }

    // ═══════════════════════════════════════════
    //  FACTEURS D'IMPACT IMMÉDIAT (pendant le cyclone)
    // ═══════════════════════════════════════════

    /// <summary>
    /// Facteur de productivité globale (0.2-1.0).
    /// Pendant le cyclone : 0.2-0.5 (routes coupées, pas de travail).
    /// Agriculture particulièrement touchée.
    /// </summary>
    public double FacteurProductivite { get; set; } = 1.0;

    /// <summary>
    /// Facteur de prix alimentaires (1.0-1.5).
    /// Hausse immédiate due à la rupture d'approvisionnement.
    /// </summary>
    public double FacteurPrixAlimentaire { get; set; } = 1.0;

    /// <summary>
    /// Facteur de production agricole (0.1-1.0).
    /// Destruction des cultures sur pied.
    /// </summary>
    public double FacteurProductionAgricole { get; set; } = 1.0;

    /// <summary>
    /// Facteur de tourisme (0.0-1.0).
    /// Le tourisme s'arrête complètement pendant un cyclone.
    /// </summary>
    public double FacteurTourisme { get; set; } = 1.0;

    /// <summary>
    /// Perte de trésorerie infligée aux entreprises touchées (MGA).
    /// Dégâts matériels, stocks détruits.
    /// </summary>
    public double PerteTresorerieEntreprises { get; set; }

    // ═══════════════════════════════════════════
    //  RECONSTRUCTION POST-CYCLONE
    // ═══════════════════════════════════════════

    /// <summary>True si on est en phase de reconstruction post-cyclone.</summary>
    public bool PhaseReconstruction { get; set; }

    /// <summary>Jour depuis le début de la reconstruction (1-based).</summary>
    public int JourReconstruction { get; set; }

    /// <summary>Durée totale de la phase de reconstruction (jours).</summary>
    public int DureeReconstructionJours { get; set; }

    /// <summary>
    /// Facteur de boost de la demande BTP/construction (1.0-1.5).
    /// La reconstruction crée un afflux de demande vers le secteur BTP.
    /// Décroissance progressive (pic au début, retour à 1.0 en fin de reconstruction).
    /// </summary>
    public double FacteurDemandeBTP { get; set; } = 1.0;

    /// <summary>
    /// Facteur de boost de la demande en quincaillerie/matériaux (1.0-2.0).
    /// Tôle, briques, ciment, bois — en forte demande post-cyclone.
    /// </summary>
    public double FacteurDemandeQuincaillerie { get; set; } = 1.0;

    /// <summary>
    /// Facteur de boost du transport informel de matériaux (1.0-1.5).
    /// Charrettes, camionnettes pour acheminer les matériaux de reconstruction.
    /// </summary>
    public double FacteurDemandeTransportMateriaux { get; set; } = 1.0;

    /// <summary>
    /// Part des ménages affectés devant reconstruire (0-1).
    /// Fonction de l'intensité du cyclone. Catégorie 3+ → 15-30%.
    /// </summary>
    public double PartMenagesAffectes { get; set; }

    /// <summary>
    /// Budget de reconstruction journalier par ménage affecté (MGA).
    /// Toit en tôle (~200 000 MGA), briques, main d'œuvre.
    /// Étalé sur la durée de reconstruction.
    /// </summary>
    public double BudgetReconstructionJourParMenage { get; set; }
}
