namespace Agriculture.Module.Models;

/// <summary>
/// Résultat du calcul de choc de sécheresse (kere) dans le Grand Sud de Madagascar.
/// 
/// Le kere est une famine récurrente dans le Grand Sud (régions Androy, Anosy, Atsimo-Andrefana).
/// Contrairement aux cyclones :
///   - Pas de reconstruction BTP (pas de destruction d'infrastructure)
///   - Aide alimentaire (PAM, BNGRC, ONG)
///   - Migration interne vers les villes (Toliara, Fianarantsoa, Tana)
///   - Durée longue (3-6 mois)
///   - Production agricole quasi nulle dans les zones touchées
///
/// Impact économique :
///   - Court terme : chute de la production agricole, hausse des prix alimentaires locaux
///   - Moyen terme : migration interne → pression sur les villes, chômage urbain
///   - Long terme : décapitalisation rurale (vente bétail, épargne épuisée)
///
/// Source : BNGRC, PAM Madagascar, Banque Mondiale (IPC Phase 3-5).
/// </summary>
public class SecheresseShockResult
{
    /// <summary>True si un épisode de sécheresse est en cours.</summary>
    public bool SecheresseActive { get; set; }

    /// <summary>Jour dans l'épisode de sécheresse (1-based, 0 si pas de sécheresse).</summary>
    public int JourDansSecheresse { get; set; }

    /// <summary>Durée totale de l'épisode de sécheresse (jours).</summary>
    public int DureeSecheresseJours { get; set; }

    /// <summary>
    /// Facteur de production agricole dans les zones touchées (0-1).
    /// 0.4 = 60% de perte de production.
    /// </summary>
    public double FacteurProductionAgricole { get; set; } = 1.0;

    /// <summary>
    /// Facteur de prix alimentaires dans les zones touchées (1.0-1.5).
    /// Hausse modérée car aide alimentaire compense partiellement.
    /// </summary>
    public double FacteurPrixAlimentaire { get; set; } = 1.0;

    /// <summary>
    /// Aide alimentaire totale distribuée ce jour (MGA).
    /// Financée par l'aide internationale (PAM, UNICEF).
    /// </summary>
    public double AideAlimentaireJour { get; set; }

    /// <summary>
    /// Nombre de ménages devant migrer ce jour suite à la sécheresse.
    /// Ces ménages passent de Rural à Urbain avec un statut de migrant.
    /// </summary>
    public int NbMenagesMigrants { get; set; }

    /// <summary>Part des ménages affectés par la sécheresse (0-1).</summary>
    public double PartMenagesAffectes { get; set; }
}
