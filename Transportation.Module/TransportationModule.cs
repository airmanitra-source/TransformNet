using Company.Module.Models;
using Household.Module.Models;

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
    /// <summary>Part du transport public allant au secteur informel (taxi-be)</summary>
    private const double PartInformelTransportPublic = 0.70;

    /// <summary>Part du carburant allant au secteur formel (stations-service enregistrées)</summary>
    private const double PartFormelCarburant = 0.60;

    /// <summary>Part de l'entretien allant au secteur informel (garagistes de rue)</summary>
    private const double PartInformelEntretien = 0.90;

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
                // Transport public : tout va en "transport public"
                result.PartTransportPublic = depenseTransport;
                result.PartInformel = depenseTransport * PartInformelTransportPublic
                                    + depenseTransportJirama;
                result.PartFormel = depenseTransport * (1.0 - PartInformelTransportPublic);
                break;

            case ModeTransport.Moto:
                // Moto : tout est du carburant
                result.PartCarburant = depenseTransport;
                result.PartInformel = depenseTransport * (1.0 - PartFormelCarburant)
                                    + depenseTransportJirama;
                result.PartFormel = depenseTransport * PartFormelCarburant;
                break;

            case ModeTransport.Voiture:
                // Voiture : carburant + entretien (~500 MGA/jour)
                double entretien = Math.Min(500.0, depenseTransport * 0.15);
                double carburant = depenseTransport - entretien;
                result.PartCarburant = carburant;
                result.PartEntretien = entretien;
                result.PartInformel = carburant * (1.0 - PartFormelCarburant)
                                    + entretien * PartInformelEntretien
                                    + depenseTransportJirama;
                result.PartFormel = carburant * PartFormelCarburant
                                  + entretien * (1.0 - PartInformelEntretien);
                break;

            default:
                // À pied : aucune dépense
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
