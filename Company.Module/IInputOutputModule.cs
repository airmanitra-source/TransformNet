using Company.Module.Models;

namespace Company.Module;

/// <summary>
/// Module de matrice input-output (tableau des entrées-sorties) simplifié.
///
/// La matrice input-output (Leontief) modélise les flux inter-sectoriels :
/// quand un secteur produit, il achète des intrants à d'autres secteurs.
/// Cela crée des effets multiplicateurs : 1 MGA de demande finale génère
/// plus de 1 MGA de production totale via les achats inter-sectoriels.
///
/// ══════════════════════════════════════════════════════════════
///  MATRICE SIMPLIFIÉE 7×7 — MADAGASCAR (INSTAT/SCN)
/// ══════════════════════════════════════════════════════════════
///
///  Secteurs : Agriculture, Textiles, Commerces, Services,
///             SecteurMinier, Construction, HôtellerieTourisme
///
///  Coefficients techniques aᵢⱼ = part des achats du secteur j
///  provenant du secteur i (en % de la production totale de j).
///
///  Source : Tableau des Ressources et Emplois (TRE) INSTAT 2019,
///           recoupé avec le SCN Madagascar et la matrice GTAP.
///
///  Multiplicateurs sectoriels résultants :
///    - Agriculture : ~1.3-1.5 (faible transformation)
///    - Textiles : ~1.6-1.8 (chaîne d'approvisionnement)
///    - Construction : ~1.8-2.2 (fort effet d'entraînement)
///    - Minier : ~1.4-1.6
/// ══════════════════════════════════════════════════════════════
/// </summary>
public interface IInputOutputModule
{
    /// <summary>
    /// Retourne le coefficient technique aᵢⱼ : part des achats du secteur acheteur
    /// provenant du secteur fournisseur.
    /// </summary>
    double GetCoefficientTechnique(ESecteurActivite fournisseur, ESecteurActivite acheteur);

    /// <summary>
    /// Calcule les flux inter-sectoriels pour un jour donné à partir des achats B2B
    /// de chaque secteur. Ventile les consommations intermédiaires entre secteurs
    /// fournisseurs selon la matrice de coefficients techniques.
    /// </summary>
    /// <param name="achatsB2BParSecteur">Total des achats B2B par secteur acheteur (MGA).</param>
    /// <returns>Résultat avec flux inter-sectoriels et demande reçue par secteur.</returns>
    InputOutputResult CalculerFluxInterSectoriels(
        Dictionary<ESecteurActivite, double> achatsB2BParSecteur);

    /// <summary>
    /// Retourne le multiplicateur de production de Leontief simplifié pour un secteur.
    /// Le multiplicateur indique combien de MGA de production totale sont générés
    /// par 1 MGA de demande finale dans ce secteur.
    /// </summary>
    double GetMultiplicateurProduction(ESecteurActivite secteur);

    /// <summary>
    /// Retourne le multiplicateur d'emploi pour un secteur.
    /// Indique combien d'emplois indirects sont créés par la demande inter-sectorielle.
    /// </summary>
    double GetMultiplicateurEmploi(ESecteurActivite secteur);

    /// <summary>
    /// Retourne la demande supplémentaire reçue par un secteur fournisseur
    /// issue des achats inter-sectoriels (effet d'entraînement).
    /// </summary>
    double CalculerDemandeInduite(
        ESecteurActivite secteurFournisseur,
        Dictionary<ESecteurActivite, double> achatsB2BParSecteur);
}
