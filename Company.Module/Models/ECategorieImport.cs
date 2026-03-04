namespace Company.Module.Models;

/// <summary>
/// Catégories d'importation avec profils fiscaux différents.
/// Calibré sur les statistiques douanières de Madagascar.
/// </summary>
public enum ECategorieImport
{
    /// <summary>Carburant, pétrole, gaz (~25% des imports)</summary>
    Carburant,
    /// <summary>Riz, huile, blé, produits alimentaires (~20%)</summary>
    Alimentaire,
    /// <summary>Machines, électronique, téléphones (~15%)</summary>
    Electronique,
    /// <summary>Véhicules, pièces auto (~10%)</summary>
    Vehicule,
    /// <summary>Biens de consommation courante (~22%)</summary>
    BienConsommation,
    /// <summary>Matières premières industrielles (~8%)</summary>
    MatierePremiere
}



