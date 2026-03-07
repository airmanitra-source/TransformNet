namespace Agriculture.Module.Models;

/// <summary>
/// Résultat du calcul d'autoconsommation agricole pour un ménage rural.
/// L'autoconsommation représente ~40% de la production agricole rurale
/// consommée directement par le ménage sans transaction monétaire.
/// 
/// Impact sur la simulation :
///   - Réduit les dépenses alimentaires monétaires du ménage
///   - Doit être imputée au PIB (SCN 2008) pour éviter une sous-estimation
///   - Stabilise la consommation rurale en période de crise
/// Source : INSTAT EPM — comptes des ménages ruraux.
/// </summary>
public class AutoconsommationResult
{
    /// <summary>Valeur monétaire imputée de l'autoconsommation du jour (MGA).</summary>
    public double ValeurImputee { get; set; }

    /// <summary>Réduction effective des dépenses alimentaires monétaires (MGA).</summary>
    public double ReductionDepensesAlimentaires { get; set; }

    /// <summary>
    /// Valeur ajoutée imputée pour le PIB (SCN 2008).
    /// Cette production non marchande doit être incluse dans le PIB agricole.
    /// </summary>
    public double ValeurAjouteeImputeePIB { get; set; }
}
