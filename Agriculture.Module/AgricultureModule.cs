using Agriculture.Module.Models;

namespace Agriculture.Module;

/// <summary>
/// Implémentation du module agricole.
/// Gère l'autoconsommation agricole et les sécheresses du Grand Sud (kere).
/// </summary>
public class AgricultureModule : IAgricultureModule
{
    // ═══════════════════════════════════════════
    //  ÉTAT INTERNE — SÉCHERESSE
    // ═══════════════════════════════════════════

    private bool _secheresseEnCours;
    private int _jourDebutSecheresse;
    private int _dureeSecheresseJours;
    private double _partMenagesAffectes;
    private int _nbSecheresses;

    /// <inheritdoc/>
    public void Reinitialiser()
    {
        _secheresseEnCours = false;
        _jourDebutSecheresse = 0;
        _dureeSecheresseJours = 0;
        _partMenagesAffectes = 0;
        _nbSecheresses = 0;
    }

    /// <inheritdoc/>
    public AutoconsommationResult CalculerAutoconsommation(
        Household.Module.Models.Household menage,
        double valeurAutoconsommationJourBase,
        double facteurSaisonnier = 1.0,
        double facteurSecheresse = 1.0)
    {
        if (!menage.PratiqueAutoconsommation || menage.Zone != Household.Module.Models.ZoneResidence.Rural)
        {
            return new AutoconsommationResult
            {
                ValeurImputee = 0,
                ReductionDepensesAlimentaires = 0,
                ValeurAjouteeImputeePIB = 0
            };
        }

        // Valeur de l'autoconsommation ajustée par la saison et la sécheresse
        double valeur = valeurAutoconsommationJourBase * facteurSaisonnier * facteurSecheresse;

        // La réduction des dépenses alimentaires monétaires est plafonnée
        // à la valeur de l'autoconsommation (on ne peut pas réduire au-delà)
        double reductionAlim = Math.Min(valeur, menage.DepensesAlimentairesJour * 0.40);

        // Mettre à jour les compteurs du ménage
        menage.AutoconsommationJour = valeur;
        menage.TotalAutoconsommation += valeur;

        return new AutoconsommationResult
        {
            ValeurImputee = valeur,
            ReductionDepensesAlimentaires = reductionAlim,
            ValeurAjouteeImputeePIB = valeur
        };
    }

    /// <inheritdoc/>
    public SecheresseShockResult EvaluerSecheresse(
        int jourCourant,
        int moisCalendaire,
        int nbMenagesRuraux,
        Random random,
        double probabiliteSecheresseJourSaison = 0.001,
        double partMenagesAffectes = 0.08,
        int dureeSecheresseJours = 120,
        double reductionProduction = 0.60,
        double aideAlimentaireJourParMenage = 3_000,
        double probabiliteMigration = 0.12)
    {
        var result = new SecheresseShockResult();

        // Vérifier si la sécheresse en cours est terminée
        if (_secheresseEnCours)
        {
            int jourDansSecheresse = jourCourant - _jourDebutSecheresse + 1;
            if (jourDansSecheresse > _dureeSecheresseJours)
            {
                _secheresseEnCours = false;
            }
            else
            {
                // Sécheresse en cours
                result.SecheresseActive = true;
                result.JourDansSecheresse = jourDansSecheresse;
                result.DureeSecheresseJours = _dureeSecheresseJours;
                result.PartMenagesAffectes = _partMenagesAffectes;

                // Impact sur la production agricole (décroissance progressive)
                double progression = (double)jourDansSecheresse / _dureeSecheresseJours;
                // La production se dégrade progressivement puis se rétablit en fin de sécheresse
                double facteurProd = progression < 0.5
                    ? 1.0 - reductionProduction * (progression * 2)     // dégradation
                    : (1.0 - reductionProduction) + reductionProduction * ((progression - 0.5) * 2); // rétablissement
                result.FacteurProductionAgricole = Math.Clamp(facteurProd, 0.1, 1.0);

                // Hausse modérée des prix alimentaires (+10 à +25%)
                result.FacteurPrixAlimentaire = 1.0 + 0.25 * (1.0 - result.FacteurProductionAgricole);

                // Aide alimentaire
                int nbAffectes = (int)(nbMenagesRuraux * _partMenagesAffectes);
                result.AideAlimentaireJour = nbAffectes * aideAlimentaireJourParMenage;

                // Migration (quelques ménages migrent chaque jour pendant la crise)
                // Probabilité quotidienne très faible pour étaler la migration
                double probMigrationJour = probabiliteMigration / _dureeSecheresseJours;
                int nbMigrants = 0;
                for (int i = 0; i < nbAffectes; i++)
                {
                    if (random.NextDouble() < probMigrationJour)
                        nbMigrants++;
                }
                result.NbMenagesMigrants = nbMigrants;

                return result;
            }
        }

        // Tirage stochastique pour un nouveau kere
        // Saison sèche : mai à novembre (mois 5-11)
        bool estSaisonSeche = moisCalendaire >= 5 && moisCalendaire <= 11;
        double probabilite = estSaisonSeche ? probabiliteSecheresseJourSaison : 0;

        if (probabilite > 0 && random.NextDouble() < probabilite)
        {
            // Nouvelle sécheresse !
            _secheresseEnCours = true;
            _jourDebutSecheresse = jourCourant;
            _dureeSecheresseJours = dureeSecheresseJours + (int)((random.NextDouble() - 0.5) * 60); // ±30 jours
            _dureeSecheresseJours = Math.Max(60, _dureeSecheresseJours);
            _partMenagesAffectes = partMenagesAffectes * (0.7 + random.NextDouble() * 0.6);
            _nbSecheresses++;

            result.SecheresseActive = true;
            result.JourDansSecheresse = 1;
            result.DureeSecheresseJours = _dureeSecheresseJours;
            result.PartMenagesAffectes = _partMenagesAffectes;
            result.FacteurProductionAgricole = 1.0; // début, pas encore d'impact
            result.FacteurPrixAlimentaire = 1.05; // légère hausse anticipatoire
        }

        return result;
    }
}
