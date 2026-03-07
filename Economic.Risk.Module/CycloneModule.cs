using Economic.Risk.Module.Models;

namespace Economic.Risk.Module;

/// <summary>
/// Implémentation du module de chocs cycloniques stochastiques.
/// Gère le cycle complet : probabilité → déclenchement → impact → reconstruction.
/// </summary>
public class CycloneModule : ICycloneModule
{
    // ═══════════════════════════════════════════
    //  ÉTAT INTERNE
    // ═══════════════════════════════════════════

    /// <summary>True si un cyclone est en cours.</summary>
    private bool _cycloneEnCours;

    /// <summary>Jour de début du cyclone en cours.</summary>
    private int _jourDebutCyclone;

    /// <summary>Durée du cyclone en cours (jours).</summary>
    private int _dureeCycloneJours;

    /// <summary>Intensité du cyclone en cours (0-1).</summary>
    private double _intensiteCyclone;

    /// <summary>Nom du cyclone en cours.</summary>
    private string _nomCyclone = "";

    /// <summary>True si on est en phase de reconstruction.</summary>
    private bool _enReconstruction;

    /// <summary>Jour de début de la reconstruction.</summary>
    private int _jourDebutReconstruction;

    /// <summary>Durée de la reconstruction (jours).</summary>
    private int _dureeReconstructionJours;

    /// <summary>Part des ménages affectés par le dernier cyclone.</summary>
    private double _partMenagesAffectes;

    /// <summary>Budget de reconstruction journalier par ménage affecté.</summary>
    private double _budgetReconstructionJour;

    /// <summary>Nombre de cyclones survenus dans cette simulation.</summary>
    private int _nbCyclones;

    /// <summary>
    /// Noms de cyclones malgaches typiques (alphabétiques par saison).
    /// Source : Centre Météorologique Régional Spécialisé de La Réunion.
    /// </summary>
    private static readonly string[] NomsCyclones =
    [
        "Alvina", "Batsirai", "Cheneso", "Dingani", "Enawo",
        "Freddy", "Gamane", "Haruna", "Ivan", "Jasmine",
        "Kamisy", "Lila", "Mohelina", "Nilakshi", "Olivier"
    ];

    /// <inheritdoc/>
    public void Reinitialiser()
    {
        _cycloneEnCours = false;
        _jourDebutCyclone = 0;
        _dureeCycloneJours = 0;
        _intensiteCyclone = 0;
        _nomCyclone = "";
        _enReconstruction = false;
        _jourDebutReconstruction = 0;
        _dureeReconstructionJours = 0;
        _partMenagesAffectes = 0;
        _budgetReconstructionJour = 0;
        _nbCyclones = 0;
    }

    /// <inheritdoc/>
    public CycloneShockResult EvaluerChocCyclonique(
        int jourCourant,
        int jourCalendaire,
        int moisCalendaire,
        Random random,
        double probabiliteCycloneJourSaison = 0.003,
        double probabiliteCycloneJourHorsSaison = 0.0002)
    {
        var result = new CycloneShockResult();

        // ═══════════════════════════════════════════
        //  1. CYCLONE EN COURS → CONTINUER L'IMPACT
        // ═══════════════════════════════════════════
        if (_cycloneEnCours)
        {
            int jourDansCyclone = jourCourant - _jourDebutCyclone + 1;

            if (jourDansCyclone > _dureeCycloneJours)
            {
                // Le cyclone est terminé → démarrer la reconstruction
                _cycloneEnCours = false;
                _enReconstruction = true;
                _jourDebutReconstruction = jourCourant;

                // Durée de reconstruction : fonction de l'intensité
                // Catégorie 1 (intensité 0.2) → ~45 jours
                // Catégorie 3 (intensité 0.6) → ~90 jours
                // Catégorie 5 (intensité 1.0) → ~120 jours
                _dureeReconstructionJours = (int)(45 + 75 * _intensiteCyclone);

                // Part des ménages affectés : fonction de l'intensité
                // Catégorie 1 → ~5%, Catégorie 3 → ~15%, Catégorie 5 → ~30%
                _partMenagesAffectes = 0.05 + 0.25 * _intensiteCyclone;

                // Budget reconstruction par ménage : toit tôle + briques + main d'œuvre
                // ~650 000 MGA total, étalé sur la durée de reconstruction
                double budgetTotalReconstruction = 650_000 * (0.5 + _intensiteCyclone);
                _budgetReconstructionJour = budgetTotalReconstruction / _dureeReconstructionJours;
            }
            else
            {
                // Cyclone actif → calculer l'impact immédiat
                result.CycloneActif = true;
                result.Intensite = _intensiteCyclone;
                result.NomCyclone = _nomCyclone;
                result.JourDansCyclone = jourDansCyclone;
                result.DureeCycloneJours = _dureeCycloneJours;

                CalculerImpactImmediat(result, jourDansCyclone);
                return result;
            }
        }

        // ═══════════════════════════════════════════
        //  2. PHASE DE RECONSTRUCTION → BOOST BTP
        // ═══════════════════════════════════════════
        if (_enReconstruction)
        {
            int jourReconstruction = jourCourant - _jourDebutReconstruction + 1;

            if (jourReconstruction > _dureeReconstructionJours)
            {
                // Fin de la reconstruction
                _enReconstruction = false;
                _partMenagesAffectes = 0;
                _budgetReconstructionJour = 0;
            }
            else
            {
                result.PhaseReconstruction = true;
                result.JourReconstruction = jourReconstruction;
                result.DureeReconstructionJours = _dureeReconstructionJours;
                result.NomCyclone = _nomCyclone;

                CalculerBoostReconstruction(result, jourReconstruction);
                return result;
            }
        }

        // ═══════════════════════════════════════════
        //  3. PAS DE CYCLONE → TIRAGE STOCHASTIQUE
        // ═══════════════════════════════════════════
        //
        // Saison cyclonique Madagascar : novembre à avril (mois 11,12,1,2,3,4)
        // Pic : janvier-mars
        // Hors saison : mai à octobre
        //
        bool estSaisonCyclonique = moisCalendaire >= 11 || moisCalendaire <= 4;

        // Modulation mensuelle (pic jan-mars)
        double modulationMensuelle = moisCalendaire switch
        {
            1 => 1.8,   // Janvier : pic cyclonique
            2 => 2.0,   // Février : pic absolu
            3 => 1.6,   // Mars : encore actif
            4 => 0.8,   // Avril : fin de saison
            11 => 0.4,  // Novembre : début de saison
            12 => 1.0,  // Décembre : saison active
            _ => 0.1    // Mai-octobre : résiduel
        };

        double probabiliteJour = estSaisonCyclonique
            ? probabiliteCycloneJourSaison * modulationMensuelle
            : probabiliteCycloneJourHorsSaison;

        // Ne pas permettre deux cyclones dans les 30 jours suivant une reconstruction
        if (_jourDebutReconstruction > 0 && jourCourant - _jourDebutReconstruction < 30)
        {
            return result;
        }

        if (random.NextDouble() < probabiliteJour)
        {
            // ═══════════════════════════════════════════
            //  NOUVEAU CYCLONE DÉCLENCHÉ
            // ═══════════════════════════════════════════
            _cycloneEnCours = true;
            _jourDebutCyclone = jourCourant;
            _nbCyclones++;

            // Durée : 3-7 jours (uniforme, typique pour Madagascar)
            _dureeCycloneJours = 3 + random.Next(5);

            // Intensité : distribution Beta(2, 5) simulée → majorité faible, quelques forts
            // La plupart des cyclones qui touchent Madagascar sont catégorie 1-2
            // Quelques-uns sont catégorie 3+ (Batsirai, Enawo)
            double u1 = random.NextDouble();
            double u2 = random.NextDouble();
            _intensiteCyclone = Math.Clamp(
                0.15 + 0.85 * Math.Pow(u1, 0.5) * Math.Pow(u2, 1.5),
                0.15, 1.0);

            // Nom
            _nomCyclone = NomsCyclones[(_nbCyclones - 1) % NomsCyclones.Length];

            result.CycloneActif = true;
            result.Intensite = _intensiteCyclone;
            result.NomCyclone = _nomCyclone;
            result.JourDansCyclone = 1;
            result.DureeCycloneJours = _dureeCycloneJours;

            CalculerImpactImmediat(result, 1);
        }

        return result;
    }

    /// <summary>
    /// Calcule les facteurs d'impact immédiat pendant le cyclone.
    /// L'intensité est maximale au milieu du cyclone et s'atténue aux extrémités.
    /// </summary>
    private void CalculerImpactImmediat(CycloneShockResult result, int jourDansCyclone)
    {
        // Profil d'intensité en cloche : pic au milieu du cyclone
        double milieu = _dureeCycloneJours / 2.0;
        double distanceAuPic = Math.Abs(jourDansCyclone - milieu) / milieu;
        double profilJour = 1.0 - 0.4 * distanceAuPic; // 0.6 aux bords, 1.0 au pic

        double intensiteEffective = _intensiteCyclone * profilJour;

        // Productivité globale : chute de 50-80% selon l'intensité
        result.FacteurProductivite = Math.Max(0.20, 1.0 - 0.80 * intensiteEffective);

        // Agriculture : particulièrement vulnérable (cultures sur pied détruites)
        result.FacteurProductionAgricole = Math.Max(0.10, 1.0 - 0.90 * intensiteEffective);

        // Prix alimentaires : hausse immédiate (rupture d'approvisionnement)
        result.FacteurPrixAlimentaire = 1.0 + 0.40 * intensiteEffective;

        // Tourisme : arrêt quasi-total pendant le cyclone
        result.FacteurTourisme = Math.Max(0.05, 1.0 - 0.95 * intensiteEffective);

        // Perte de trésorerie des entreprises : proportionnelle à l'intensité
        result.PerteTresorerieEntreprises = 2_000_000 * intensiteEffective;
    }

    /// <summary>
    /// Calcule les facteurs de boost de la demande pendant la reconstruction.
    /// La demande BTP/quincaillerie/transport suit un profil décroissant.
    /// </summary>
    private void CalculerBoostReconstruction(CycloneShockResult result, int jourReconstruction)
    {
        // Profil décroissant : urgence forte au début, normalisation progressive
        double progression = (double)jourReconstruction / _dureeReconstructionJours;
        double facteurTemporel = Math.Max(0.2, 1.0 - 0.8 * progression);

        // Boost BTP : maçons, charpentiers, peintres
        double boostBTP = 0.20 + 0.30 * _intensiteCyclone;
        result.FacteurDemandeBTP = 1.0 + boostBTP * facteurTemporel;

        // Boost quincaillerie : tôle ondulée, briques, ciment, bois, clous
        double boostQuincaillerie = 0.30 + 0.50 * _intensiteCyclone;
        result.FacteurDemandeQuincaillerie = 1.0 + boostQuincaillerie * facteurTemporel;

        // Boost transport matériaux : charrettes, camionnettes, pousse-pousse
        double boostTransport = 0.20 + 0.20 * _intensiteCyclone;
        result.FacteurDemandeTransportMateriaux = 1.0 + boostTransport * facteurTemporel;

        // Ménages affectés et budget
        result.PartMenagesAffectes = _partMenagesAffectes;
        result.BudgetReconstructionJourParMenage = _budgetReconstructionJour;

        // Hausse résiduelle des prix alimentaires post-cyclone (s'estompe)
        result.FacteurPrixAlimentaire = 1.0 + 0.15 * _intensiteCyclone * facteurTemporel;

        // Légère baisse de productivité résiduelle (routes encore endommagées)
        result.FacteurProductivite = 1.0 - 0.10 * _intensiteCyclone * facteurTemporel;
    }
}
