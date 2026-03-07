using Company.Module.Models;

namespace Company.Module;

/// <summary>
/// Résultat du calcul de la matrice input-output pour un jour donné.
/// Contient les consommations intermédiaires ventilées par secteur
/// et les multiplicateurs de Leontief simplifiés.
/// </summary>
public class InputOutputResult
{
    /// <summary>
    /// Consommations intermédiaires par secteur acheteur → secteur fournisseur.
    /// Clé : (secteur acheteur, secteur fournisseur), Valeur : montant MGA.
    /// </summary>
    public Dictionary<(ESecteurActivite Acheteur, ESecteurActivite Fournisseur), double> FluxInterSectoriels { get; set; } = new();

    /// <summary>
    /// Total des consommations intermédiaires par secteur fournisseur (demande reçue, MGA).
    /// </summary>
    public Dictionary<ESecteurActivite, double> DemandeRecueParSecteur { get; set; } = new();

    /// <summary>
    /// Multiplicateur de production par secteur (effet Leontief simplifié).
    /// Ex: 1 MGA de demande finale Agriculture génère ~1.4 MGA de production totale.
    /// </summary>
    public Dictionary<ESecteurActivite, double> MultiplicateursProduction { get; set; } = new();

    /// <summary>
    /// Total des consommations intermédiaires de la journée (MGA).
    /// </summary>
    public double TotalConsommationsIntermediaires { get; set; }
}
