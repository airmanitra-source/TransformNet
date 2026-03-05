using Company.Module.Models;
using Household.Module.Models;

namespace Transportation.Module;

/// <summary>
/// Contrat du module de transport.
/// Route les dépenses de transport des ménages vers les entreprises du secteur transport
/// et calcule les métriques de transport agrégées.
/// </summary>
public interface ITransportationModule
{
    /// <summary>
    /// Route la dépense de transport d'un ménage vers l'entreprise de transport appropriée.
    /// Décompose la dépense selon le mode de transport :
    /// - Transport public → entreprises formelles de transport
    /// - Moto/Voiture → secteur carburant (informel/formel)
    /// </summary>
    TransportRoutingResult RouterDepenseTransport(
        ModeTransport modeTransport,
        double depenseTransport,
        double depenseTransportJirama);

    /// <summary>
    /// Applique les dépenses de transport routées à une entreprise du secteur transport/commerce.
    /// Injecte la demande dans la trésorerie de l'entreprise.
    /// </summary>
    void AppliquerDemandeTransport(
        Company.Module.Models.Company entreprise,
        double demandeTransportAffectee);
}
