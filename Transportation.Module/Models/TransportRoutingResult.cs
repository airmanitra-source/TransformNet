namespace Transportation.Module.Models;

/// <summary>
/// Résultat du routage d'une dépense de transport vers les secteurs économiques.
/// </summary>
public class TransportRoutingResult
{
    /// <summary>Dépense totale de transport du ménage (MGA)</summary>
    public double DepenseTotale { get; set; }

    /// <summary>
    /// Part routée vers le transport public formel (taxi-be, bus).
    /// Alimente les entreprises formelles du secteur Services/Commerces.
    /// </summary>
    public double PartTransportPublic { get; set; }

    /// <summary>
    /// Part routée vers le carburant (moto, voiture).
    /// Alimente les importateurs (carburant importé) et stations-service (informel/formel).
    /// </summary>
    public double PartCarburant { get; set; }

    /// <summary>
    /// Part routée vers l'entretien véhicule (voiture uniquement, ~500 MGA/jour).
    /// Alimente les entreprises informelles (garagistes).
    /// </summary>
    public double PartEntretien { get; set; }

    /// <summary>
    /// Part routée vers le transport pour paiement Jirama (déplacement mensuel).
    /// </summary>
    public double PartTransportJirama { get; set; }

    /// <summary>Part qui reste dans le secteur informel (pas de TVA)</summary>
    public double PartInformel { get; set; }

    /// <summary>Part qui va au secteur formel (avec TVA)</summary>
    public double PartFormel { get; set; }
}
