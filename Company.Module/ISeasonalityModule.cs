using Company.Module.Models;

namespace Company.Module;

/// <summary>
/// Module de saisonnalité agricole pour la simulation économique de Madagascar.
///
/// L'agriculture malgache est fortement saisonnière. Le calendrier cultural
/// détermine la productivité des entreprises agricoles, les prix alimentaires,
/// les volumes d'exportation et l'emploi rural tout au long de l'année.
///
/// ══════════════════════════════════════════════════════════════
///  CALENDRIER AGRICOLE DE MADAGASCAR
/// ══════════════════════════════════════════════════════════════
///
///  Riz (65% de la surface cultivée) :
///    • Vary aloha (riz précoce)  : semis oct → récolte jan-fév
///    • Vary be (riz principal)   : semis nov-déc → récolte mars-juin
///    • Vary vakiambiaty (3e)     : semis jan → récolte avr-mai (irrigué)
///
///  Période de soudure (kere) : fév-avril
///    → Stocks de riz épuisés, prix alimentaires au plus haut
///    → Importations de riz en hausse, privation alimentaire
///
///  Vanille (1er producteur mondial) :
///    → Pollinisation : oct-déc | Récolte verte : mai-jul | Export : jul-sept
///
///  Girofle : Récolte oct-déc | Export : nov-fév
///  Café    : Récolte mai-sep | Export : jun-oct
///  Crevettes : Pêche avr-nov (fermeture déc-mars)
///  Litchis : Récolte nov-jan | Export : nov-jan
///
///  Tourisme : Haute saison jul-oct (hiver austral sec)
///             Basse saison jan-mars (cyclones)
/// ══════════════════════════════════════════════════════════════
/// </summary>
public interface ISeasonalityModule
{
    /// <summary>
    /// Calcule les facteurs saisonniers pour un jour donné de la simulation.
    /// </summary>
    /// <param name="jourCourant">Jour de simulation (1-based).</param>
    /// <param name="jourDebutSimulation">Jour calendaire du début (1=1er jan, défaut=1).</param>
    /// <returns>Facteurs saisonniers à appliquer à la productivité, prix et exports.</returns>
    SeasonalityResult CalculerSaisonnalite(int jourCourant, int jourDebutSimulation = 1);
}

/// <summary>
/// Résultat du calcul de saisonnalité pour un jour donné.
/// Contient les facteurs multiplicatifs à appliquer aux différents secteurs.
/// </summary>
public class SeasonalityResult
{
    // ═══════════════════════════════════════════
    //  CALENDRIER
    // ═══════════════════════════════════════════

    /// <summary>Jour calendaire dans l'année (1-365).</summary>
    public int JourCalendaire { get; set; }

    /// <summary>Mois calendaire (1-12).</summary>
    public int Mois { get; set; }

    /// <summary>Nom de la saison agricole courante.</summary>
    public string SaisonCourante { get; set; } = "";

    /// <summary>True si on est en période de soudure (kere) : fév-avril.</summary>
    public bool EstPeriodeSoudure { get; set; }

    // ═══════════════════════════════════════════
    //  FACTEURS MULTIPLICATIFS (1.0 = baseline)
    // ═══════════════════════════════════════════

    /// <summary>
    /// Facteur de productivité agricole (0.3 – 1.5).
    /// Récolte = pic (1.2-1.5), inter-saison = creux (0.3-0.5).
    /// Appliqué à ProductiviteParEmployeJour des entreprises Agriculture.
    /// </summary>
    public double FacteurProductiviteAgricole { get; set; } = 1.0;

    /// <summary>
    /// Facteur de prix du riz local (0.8 – 1.6).
    /// Soudure = prix élevé (1.3-1.6), post-récolte = prix bas (0.8-0.9).
    /// Appliqué au prix du riz dans les dépenses alimentaires des ménages.
    /// </summary>
    public double FacteurPrixRiz { get; set; } = 1.0;

    /// <summary>
    /// Facteur de prix alimentaire général (0.85 – 1.3).
    /// Moins volatile que le riz seul (panier diversifié).
    /// Appliqué aux dépenses alimentaires des ménages.
    /// </summary>
    public double FacteurPrixAlimentaire { get; set; } = 1.0;

    /// <summary>
    /// Facteurs d'exportation par catégorie (0.2 – 2.0).
    /// Appliqués aux ValeurFOBJour des exportateurs.
    /// </summary>
    public Dictionary<ECategorieExport, double> FacteursExport { get; set; } = new();

    /// <summary>
    /// Facteur de tourisme / hôtellerie (0.4 – 1.5).
    /// Haute saison = 1.3-1.5, basse saison cyclones = 0.4-0.6.
    /// Appliqué à la demande vers les entreprises HotellerieTourisme.
    /// </summary>
    public double FacteurTourisme { get; set; } = 1.0;

    /// <summary>
    /// Facteur d'importation de riz (0.5 – 2.0).
    /// Soudure = imports en hausse (1.5-2.0), post-récolte = faible (0.5-0.7).
    /// </summary>
    public double FacteurImportRiz { get; set; } = 1.0;

    /// <summary>
    /// Facteur d'emploi agricole saisonnier (0.6 – 1.3).
    /// Période de semis/récolte = embauche temporaire.
    /// Inter-saison = sous-emploi rural.
    /// </summary>
    public double FacteurEmploiAgricole { get; set; } = 1.0;
}
