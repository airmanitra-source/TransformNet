using Agriculture.Module.Models;

namespace Agriculture.Module;

/// <summary>
/// Module agricole pour la simulation économique de Madagascar.
/// Gère deux mécanismes clés absents du modèle de base :
///
/// 1. AUTOCONSOMMATION AGRICOLE
///    Les ménages ruraux consomment ~40% de leur production sans transaction monétaire.
///    Le PIB est surestimé si cette production n'est pas imputée.
///    Source : INSTAT EPM — comptes des ménages ruraux.
///
/// 2. SÉCHERESSE DU GRAND SUD (KERE)
///    Famine récurrente dans les régions Androy/Anosy/Atsimo-Andrefana.
///    Contrairement aux cyclones :
///      - Pas de reconstruction BTP (pas de destruction d'infrastructures)
///      - Aide alimentaire (PAM, BNGRC)
///      - Migration interne vers les villes
///    Source : BNGRC, PAM Madagascar, IPC Phase 3-5.
/// </summary>
public interface IAgricultureModule
{
    /// <summary>
    /// Calcule l'autoconsommation agricole d'un ménage rural pour un jour donné.
    /// Réduit les dépenses alimentaires monétaires et impute la valeur au PIB.
    /// </summary>
    /// <param name="menage">Le ménage concerné.</param>
    /// <param name="valeurAutoconsommationJourBase">Valeur de base de l'autoconsommation (MGA).</param>
    /// <param name="facteurSaisonnier">Facteur saisonnier de productivité agricole (0-2).</param>
    /// <param name="facteurSecheresse">Facteur de réduction si sécheresse en cours (0-1).</param>
    /// <returns>Résultat avec valeur imputée et réduction des dépenses.</returns>
    AutoconsommationResult CalculerAutoconsommation(
        Household.Module.Models.Household menage,
        double valeurAutoconsommationJourBase,
        double facteurSaisonnier = 1.0,
        double facteurSecheresse = 1.0);

    /// <summary>
    /// Évalue le choc de sécheresse (kere) pour un jour donné de la simulation.
    /// Gère : déclenchement stochastique → impact sur production → aide alimentaire → migration.
    /// </summary>
    /// <param name="jourCourant">Jour de simulation (1-based).</param>
    /// <param name="moisCalendaire">Mois calendaire (1-12).</param>
    /// <param name="nbMenagesRuraux">Nombre de ménages ruraux dans la simulation.</param>
    /// <param name="random">Générateur aléatoire.</param>
    /// <param name="probabiliteSecheresseJourSaison">Probabilité journalière en saison sèche.</param>
    /// <param name="partMenagesAffectes">Part des ménages touchés (0-1).</param>
    /// <param name="dureeSecheresseJours">Durée de l'épisode (jours).</param>
    /// <param name="reductionProduction">Facteur de réduction de la production (0-1).</param>
    /// <param name="aideAlimentaireJourParMenage">Aide alimentaire journalière par ménage (MGA).</param>
    /// <param name="probabiliteMigration">Probabilité de migration par ménage touché.</param>
    /// <returns>Résultat avec facteurs d'impact, aide, migration.</returns>
    SecheresseShockResult EvaluerSecheresse(
        int jourCourant,
        int moisCalendaire,
        int nbMenagesRuraux,
        Random random,
        double probabiliteSecheresseJourSaison = 0.001,
        double partMenagesAffectes = 0.08,
        int dureeSecheresseJours = 120,
        double reductionProduction = 0.60,
        double aideAlimentaireJourParMenage = 3_000,
        double probabiliteMigration = 0.12);

    /// <summary>
    /// Réinitialise l'état interne du module (entre deux simulations).
    /// </summary>
    void Reinitialiser();
}
