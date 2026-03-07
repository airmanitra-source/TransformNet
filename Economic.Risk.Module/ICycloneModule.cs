using Economic.Risk.Module.Models;

namespace Economic.Risk.Module;

/// <summary>
/// Module de chocs climatiques stochastiques pour la simulation économique de Madagascar.
///
/// Madagascar est le 3e pays le plus exposé aux cyclones tropicaux au monde.
/// Saison cyclonique : novembre à avril (6 mois).
/// Fréquence : 1-3 cyclones touchant terre par an (moyenne 1.5).
///
/// ══════════════════════════════════════════════════════════════
///  MODÈLE CYCLONIQUE
/// ══════════════════════════════════════════════════════════════
///
///  1. TIRAGE STOCHASTIQUE
///     - Probabilité journalière pendant la saison (nov-avr) : ~0.3%/jour
///     - Hors saison (mai-oct) : ~0.02%/jour (résiduel)
///     - Intensité tirée selon distribution calibrée BNGRC
///
///  2. IMPACT IMMÉDIAT (pendant le cyclone, 3-7 jours)
///     - Productivité : -50 à -80% (routes coupées, pas de travail)
///     - Agriculture : -30 à -70% des récoltes sur pied
///     - Tourisme : arrêt complet
///     - Prix alimentaires : +15 à +40%
///     - Trésorerie entreprises : pertes matérielles
///
///  3. RECONSTRUCTION POST-CYCLONE (60-120 jours)
///     - Demande BTP : +20 à +50% (réparation toits, murs)
///     - Demande quincaillerie : +30 à +80% (tôle, briques, ciment)
///     - Transport matériaux : +20 à +40%
///     - Ménages affectés : dépenses de reconstruction
///       → Toit tôle : ~200 000 MGA
///       → Briques/murs : ~300 000 MGA
///       → Main d'œuvre : ~150 000 MGA
///       → Total moyen : ~650 000 MGA étalé sur 60-120 jours
///
///  Effet net sur le PIB :
///     Court terme : négatif (destruction, arrêt production)
///     Moyen terme : neutre à légèrement positif (reconstruction BTP)
///     Long terme : négatif (capital détruit, épargne puisée)
/// ══════════════════════════════════════════════════════════════
/// </summary>
public interface ICycloneModule
{
    /// <summary>
    /// Évalue le choc cyclonique pour un jour donné de la simulation.
    /// Gère le cycle complet : tirage stochastique → impact → reconstruction.
    /// </summary>
    /// <param name="jourCourant">Jour de simulation (1-based).</param>
    /// <param name="jourCalendaire">Jour dans l'année (1-365) pour la saisonnalité.</param>
    /// <param name="moisCalendaire">Mois calendaire (1-12).</param>
    /// <param name="random">Générateur aléatoire (reproductibilité).</param>
    /// <param name="probabiliteCycloneJourSaison">Probabilité journalière en saison (défaut 0.003).</param>
    /// <param name="probabiliteCycloneJourHorsSaison">Probabilité journalière hors saison (défaut 0.0002).</param>
    /// <returns>Résultat avec facteurs d'impact et état de reconstruction.</returns>
    CycloneShockResult EvaluerChocCyclonique(
        int jourCourant,
        int jourCalendaire,
        int moisCalendaire,
        Random random,
        double probabiliteCycloneJourSaison = 0.003,
        double probabiliteCycloneJourHorsSaison = 0.0002);

    /// <summary>
    /// Réinitialise l'état interne du module (entre deux simulations).
    /// </summary>
    void Reinitialiser();
}
