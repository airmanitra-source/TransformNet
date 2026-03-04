using Company.Module.Models;
namespace Company.Module.Models;

/// <summary>
/// Mode de transport utilisé par le ménage pour aller travailler.
/// </summary>
public enum ModeTransport
{
    /// <summary>Transport en commun (taxi-be, bus) : 600 MGA l'aller, 1 200 MGA aller-retour</summary>
    TransportPublic,
    /// <summary>Moto personnelle : coût carburant variable</summary>
    Moto,
    /// <summary>Voiture personnelle : coût carburant + entretien</summary>
    Voiture
}



