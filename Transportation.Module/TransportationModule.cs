using Company.Module.Models;
using Household.Module.Models;
using Transportation.Module.Models;

namespace Transportation.Module;

/// <summary>
/// Module de transport — Route les dépenses de transport des ménages vers les entreprises.
///
/// Réalités Madagascar :
/// - Transport public (taxi-be) : ~70% des déplacements urbains, 600 MGA/trajet
/// - Moto : ~20% des déplacements, consommation ~3L/100km
/// - Voiture : ~10%, consommation ~8L/100km + entretien ~500 MGA/jour
/// - Le carburant est 100% importé → chaque dépense carburant alimente les importateurs
/// - Le transport public est majoritairement informel (taxi-be non enregistrés)
///
/// Décomposition sectorielle des dépenses de transport :
/// - Transport public → 70% informel (taxi-be) / 30% formel (bus)
/// - Carburant → 100% importé puis redistribué (stations-service formelles ~60%)
/// - Entretien → 90% informel (garagistes de rue)
/// </summary>
public class TransportationModule : ITransportationModule
{
    // Ces champs DOIVENT être initialisés via Configurer() (appelé par SimulationModule.Initialiser())
    // avant tout usage. Zéro est une valeur délibérément invalide pour rendre visible un oubli.
    private double _partInformelTransportPublic;
    private double _partFormelCarburant;
    private double _partInformelEntretien;
    private double _entretienVoitureJour;
    private double _entretienFractionRevenuVoiture;

    /// <inheritdoc/>
    public void Configurer(
        double partInformelTransportPublic,
        double partFormelCarburant,
        double partInformelEntretien,
        double entretienVoitureJour,
        double entretienFractionRevenuVoiture)
    {
        _partInformelTransportPublic = partInformelTransportPublic;
        _partFormelCarburant = partFormelCarburant;
        _partInformelEntretien = partInformelEntretien;
        _entretienVoitureJour = entretienVoitureJour;
        _entretienFractionRevenuVoiture = entretienFractionRevenuVoiture;
    }

    public TransportRoutingResult RouterDepenseTransport(
        ModeTransport modeTransport,
        double depenseTransport,
        double depenseTransportJirama)
    {
        var result = new TransportRoutingResult
        {
            DepenseTotale = depenseTransport + depenseTransportJirama,
            PartTransportJirama = depenseTransportJirama,
        };

        switch (modeTransport)
        {
            case ModeTransport.TransportPublic:
                result.PartTransportPublic = depenseTransport;
                result.PartInformel = depenseTransport * _partInformelTransportPublic
                                    + depenseTransportJirama;
                result.PartFormel = depenseTransport * (1.0 - _partInformelTransportPublic);
                break;

            case ModeTransport.Moto:
                result.PartCarburant = depenseTransport;
                result.PartInformel = depenseTransport * (1.0 - _partFormelCarburant)
                                    + depenseTransportJirama;
                result.PartFormel = depenseTransport * _partFormelCarburant;
                break;

            case ModeTransport.Voiture:
                double entretien = Math.Min(_entretienVoitureJour, depenseTransport * _entretienFractionRevenuVoiture);
                double carburant = depenseTransport - entretien;
                result.PartCarburant = carburant;
                result.PartEntretien = entretien;
                result.PartInformel = carburant * (1.0 - _partFormelCarburant)
                                    + entretien * _partInformelEntretien
                                    + depenseTransportJirama;
                result.PartFormel = carburant * _partFormelCarburant
                                  + entretien * (1.0 - _partInformelEntretien);
                break;

            default:
                result.PartInformel = depenseTransportJirama;
                break;
        }

        return result;
    }

    public void AppliquerDemandeTransport(
        Company.Module.Models.Company entreprise,
        double demandeTransportAffectee)
    {
        if (demandeTransportAffectee <= 0) return;

        // La demande de transport s'ajoute au CA de l'entreprise
        entreprise.ChiffreAffairesCumule += demandeTransportAffectee;
        entreprise.Tresorerie += demandeTransportAffectee;
    }
}
