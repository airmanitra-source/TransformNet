using Company.Module.Models;

namespace Company.Module;

/// <summary>
/// Implémentation du module de saisonnalité agricole de Madagascar.
///
/// Modélise les cycles naturels qui rythment l'économie malgache :
///   - Riz bimodal (vary aloha + vary be) avec période de soudure
///   - Cultures d'exportation (vanille, girofle, café, crevettes)
///   - Tourisme (haute saison sèche / basse saison cyclones)
///
/// Chaque cycle est modélisé par une courbe sinusoïdale centrée sur le pic
/// de la saison, modulée par une amplitude calibrée sur les données INSTAT.
///
/// La saisonnalité est le principal déterminant de :
///   - La productivité agricole (VA/employé/jour)
///   - Les prix alimentaires (riz local surtout)
///   - Les volumes d'exportation FOB par catégorie
///   - La demande touristique
///   - L'emploi rural (journaliers agricoles)
/// </summary>
public class SeasonalityModule : ISeasonalityModule
{
    /// <summary>
    /// Amplitude de la variation saisonnière de la productivité agricole.
    /// 0.5 = la productivité oscille entre 0.5× et 1.5× la moyenne.
    /// Calibrage : rendements rizicoles Madagascar varient de 40-60% entre saisons.
    /// </summary>
    public double AmplitudeProductiviteAgricole { get; set; } = 0.50;

    /// <summary>
    /// Amplitude de la variation saisonnière du prix du riz.
    /// 0.35 = le prix oscille entre 0.65× et 1.35× la moyenne.
    /// Données INSTAT : prix du riz local varie de 25-40% entre soudure et post-récolte.
    /// </summary>
    public double AmplitudePrixRiz { get; set; } = 0.35;

    /// <summary>
    /// Amplitude de la variation des prix alimentaires généraux.
    /// Plus faible que le riz (panier diversifié).
    /// </summary>
    public double AmplitudePrixAlimentaire { get; set; } = 0.18;

    /// <summary>
    /// Amplitude de la variation touristique.
    /// 0.45 = oscillation de 0.55× à 1.45× la moyenne.
    /// Données Ministère Tourisme : fréquentation ×2-3 entre basse et haute saison.
    /// </summary>
    public double AmplitudeTourisme { get; set; } = 0.45;

    /// <summary>
    /// Amplitude de la variation de l'emploi agricole saisonnier.
    /// </summary>
    public double AmplitudeEmploiAgricole { get; set; } = 0.25;

    public SeasonalityResult CalculerSaisonnalite(int jourCourant, int jourDebutSimulation = 1)
    {
        // Convertir le jour de simulation en jour calendaire (1-365)
        int jourCalendaire = ((jourDebutSimulation - 1 + jourCourant - 1) % 365) + 1;
        int mois = JourVsMois(jourCalendaire);

        var result = new SeasonalityResult
        {
            JourCalendaire = jourCalendaire,
            Mois = mois
        };

        // ═══════════════════════════════════════════
        //  1. PRODUCTIVITÉ AGRICOLE (riz bimodal)
        // ═══════════════════════════════════════════
        //
        // Deux pics de récolte :
        //   - Vary aloha (précoce) : pic fin janvier (jour ~30)
        //   - Vary be (principal)  : pic fin avril (jour ~120)
        //
        // Le vary be représente ~70% de la production totale.
        // Inter-saison (jul-sept) : productivité au plus bas (entretien, préparation).
        //
        double picVaryAloha = CycleGaussien(jourCalendaire, jourPic: 30, largeur: 40);
        double picVaryBe = CycleGaussien(jourCalendaire, jourPic: 120, largeur: 50);

        // Combinaison pondérée : vary be dominant (70/30)
        double cycleProdAgricole = 0.30 * picVaryAloha + 0.70 * picVaryBe;

        // Transformer en facteur autour de 1.0
        // cycleProdAgricole ∈ [0, 1] → facteur ∈ [1-amplitude, 1+amplitude]
        result.FacteurProductiviteAgricole = 1.0 + AmplitudeProductiviteAgricole * (2.0 * cycleProdAgricole - 1.0);
        result.FacteurProductiviteAgricole = Math.Clamp(result.FacteurProductiviteAgricole, 0.30, 1.80);

        // ═══════════════════════════════════════════
        //  2. PRIX DU RIZ (inverse de la production)
        // ═══════════════════════════════════════════
        //
        // Soudure (fév-avril) : stocks épuisés → prix au plus haut
        //   Le pic de prix du riz se situe vers mars (jour ~75)
        //   car les récoltes du vary aloha sont limitées et le vary be n'est pas encore moissonné.
        //
        // Post-récolte (mai-jul) : abondance → prix au plus bas
        //
        double picSoudure = CycleGaussien(jourCalendaire, jourPic: 75, largeur: 45);
        result.FacteurPrixRiz = 1.0 + AmplitudePrixRiz * (2.0 * picSoudure - 1.0);
        result.FacteurPrixRiz = Math.Clamp(result.FacteurPrixRiz, 0.70, 1.60);

        // Période de soudure : fév (32) à avril (120)
        result.EstPeriodeSoudure = mois >= 2 && mois <= 4;

        // Prix alimentaire général : corrélé au riz mais atténué
        result.FacteurPrixAlimentaire = 1.0 + AmplitudePrixAlimentaire * (2.0 * picSoudure - 1.0);
        result.FacteurPrixAlimentaire = Math.Clamp(result.FacteurPrixAlimentaire, 0.85, 1.30);

        // Import riz : pic pendant la soudure
        result.FacteurImportRiz = 1.0 + 0.50 * (2.0 * picSoudure - 1.0);
        result.FacteurImportRiz = Math.Clamp(result.FacteurImportRiz, 0.50, 2.00);

        // ═══════════════════════════════════════════
        //  3. SAISON COURANTE (label)
        // ═══════════════════════════════════════════
        result.SaisonCourante = mois switch
        {
            1 => "Récolte vary aloha",
            2 or 3 => "Soudure (kere)",
            4 => "Fin soudure / début récolte vary be",
            5 or 6 => "Récolte vary be (pic)",
            7 or 8 => "Post-récolte (abondance)",
            9 => "Inter-saison (préparation)",
            10 or 11 => "Semis / début cycle",
            12 => "Semis vary be",
            _ => "Transition"
        };

        // ═══════════════════════════════════════════
        //  4. EXPORTATIONS PAR CATÉGORIE
        // ═══════════════════════════════════════════
        //
        // Chaque culture d'exportation a son propre calendrier.
        // Les facteurs modulent la ValeurFOBJour des exportateurs.
        //
        var facteursExport = new Dictionary<ECategorieExport, double>();

        // Vanille : export pic jul-sept (après récolte mai-jul)
        double cycleVanille = CycleGaussien(jourCalendaire, jourPic: 228, largeur: 50); // ~mi-août
        facteursExport[ECategorieExport.Vanille] = 1.0 + 0.60 * (2.0 * cycleVanille - 1.0);

        // Girofle : récolte oct-déc, export nov-fév
        double cycleGirofle = CycleGaussien(jourCalendaire, jourPic: 335, largeur: 45); // ~début déc
        facteursExport[ECategorieExport.Girofle] = 1.0 + 0.50 * (2.0 * cycleGirofle - 1.0);

        // Café : récolte mai-sep, export jun-oct
        double cycleCafe = CycleGaussien(jourCalendaire, jourPic: 213, largeur: 55); // ~début août
        facteursExport[ECategorieExport.Cafe] = 1.0 + 0.40 * (2.0 * cycleCafe - 1.0);

        // Crevettes : pêche avr-nov (fermeture déc-mars)
        double cycleCrevettes = CycleGaussien(jourCalendaire, jourPic: 213, largeur: 70); // large saison
        // Fermeture biologique déc-mars → facteur très bas
        bool fermeturePeche = mois == 12 || mois <= 3;
        facteursExport[ECategorieExport.Crevettes] = fermeturePeche
            ? 0.20
            : 1.0 + 0.40 * (2.0 * cycleCrevettes - 1.0);

        // Biens alimentaires : corrélé à la production agricole (inversé vs soudure)
        facteursExport[ECategorieExport.BiensAlimentaires] = result.FacteurProductiviteAgricole;

        // Produits miniers : peu saisonnier (opération continue)
        // Légère baisse pendant saison des pluies (jan-mars, accès routes difficile)
        double cyclePluies = CycleGaussien(jourCalendaire, jourPic: 45, largeur: 50);
        facteursExport[ECategorieExport.ProduitsMiniers] = 1.0 - 0.10 * cyclePluies;

        // Zones franches (textile) : peu saisonnier (commandes internationales)
        // Légère hausse avant fêtes (sept-nov pour livraisons Noël)
        double cycleTextile = CycleGaussien(jourCalendaire, jourPic: 290, largeur: 45);
        facteursExport[ECategorieExport.ZonesFranches] = 1.0 + 0.10 * (2.0 * cycleTextile - 1.0);

        // Clamper tous les facteurs d'export
        foreach (var key in facteursExport.Keys.ToList())
        {
            facteursExport[key] = Math.Clamp(facteursExport[key], 0.15, 2.50);
        }

        result.FacteursExport = facteursExport;

        // ═══════════════════════════════════════════
        //  5. TOURISME
        // ═══════════════════════════════════════════
        //
        // Haute saison : jul-oct (hiver austral, sec, baleines, faune)
        // Basse saison : jan-mars (cyclones, pluies torrentielles)
        //
        double cycleTourisme = CycleGaussien(jourCalendaire, jourPic: 244, largeur: 55); // ~début sept
        double cycleCyclones = CycleGaussien(jourCalendaire, jourPic: 45, largeur: 40);  // ~mi-fév
        result.FacteurTourisme = 1.0
            + AmplitudeTourisme * (2.0 * cycleTourisme - 1.0)
            - 0.30 * cycleCyclones; // pénalité cyclones
        result.FacteurTourisme = Math.Clamp(result.FacteurTourisme, 0.35, 1.60);

        // ═══════════════════════════════════════════
        //  6. EMPLOI AGRICOLE
        // ═══════════════════════════════════════════
        //
        // Périodes de fort emploi : semis (oct-déc) + récoltes (jan-fév, avr-jun)
        // Inter-saison (jul-sept) : sous-emploi rural, migration vers villes
        //
        double cycleSemis = CycleGaussien(jourCalendaire, jourPic: 305, largeur: 40);     // ~début nov
        double cycleRecolteAloha = CycleGaussien(jourCalendaire, jourPic: 30, largeur: 30);
        double cycleRecolteBe = CycleGaussien(jourCalendaire, jourPic: 135, largeur: 40); // ~mi-mai
        double cycleEmploi = Math.Max(cycleSemis, Math.Max(cycleRecolteAloha, cycleRecolteBe));
        result.FacteurEmploiAgricole = 1.0 + AmplitudeEmploiAgricole * (2.0 * cycleEmploi - 1.0);
        result.FacteurEmploiAgricole = Math.Clamp(result.FacteurEmploiAgricole, 0.60, 1.40);

        return result;
    }

    // ═══════════════════════════════════════════
    //  UTILITAIRES
    // ═══════════════════════════════════════════

    /// <summary>
    /// Courbe gaussienne cyclique (période 365 jours).
    /// Retourne une valeur ∈ [0, 1] avec un pic à jourPic.
    /// La largeur contrôle l'étalement de la saison.
    /// </summary>
    /// <param name="jourCalendaire">Jour dans l'année (1-365).</param>
    /// <param name="jourPic">Jour du pic de la saison (1-365).</param>
    /// <param name="largeur">Largeur de la gaussienne en jours (σ).</param>
    private static double CycleGaussien(int jourCalendaire, int jourPic, double largeur)
    {
        // Distance circulaire (min entre chemin direct et chemin par l'an neuf)
        double dist = Math.Abs(jourCalendaire - jourPic);
        dist = Math.Min(dist, 365.0 - dist);

        return Math.Exp(-0.5 * (dist / largeur) * (dist / largeur));
    }

    /// <summary>
    /// Convertit un jour calendaire (1-365) en mois (1-12).
    /// Approximation simplifiée (mois de 30/31 jours).
    /// </summary>
    private static int JourVsMois(int jourCalendaire)
    {
        return jourCalendaire switch
        {
            <= 31 => 1,                    // Janvier
            <= 59 => 2,                    // Février
            <= 90 => 3,                    // Mars
            <= 120 => 4,                   // Avril
            <= 151 => 5,                   // Mai
            <= 181 => 6,                   // Juin
            <= 212 => 7,                   // Juillet
            <= 243 => 8,                   // Août
            <= 273 => 9,                   // Septembre
            <= 304 => 10,                  // Octobre
            <= 334 => 11,                  // Novembre
            _ => 12                        // Décembre
        };
    }
}
