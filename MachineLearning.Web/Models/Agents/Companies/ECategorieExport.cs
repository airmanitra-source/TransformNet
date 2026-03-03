namespace MachineLearning.Web.Models.Agents.Companies;

/// <summary>
/// Catégories d'exportation de Madagascar alignées sur les statistiques INSTAT.
/// Source : Tableau 32 — Évolution mensuelle des exportations FOB.
/// </summary>
public enum ECategorieExport
{
    /// <summary>Biens alimentaires (hors vanille/crevettes/café/girofle)</summary>
    BiensAlimentaires,
    /// <summary>Vanille — 1er producteur mondial</summary>
    Vanille,
    /// <summary>Crevettes, pêche</summary>
    Crevettes,
    /// <summary>Café</summary>
    Cafe,
    /// <summary>Girofle, épices</summary>
    Girofle,
    /// <summary>Produits miniers (nickel, cobalt, ilménite, saphir…)</summary>
    ProduitsMiniers,
    /// <summary>Zones franches (textile, confection, électronique)</summary>
    ZonesFranches
}
