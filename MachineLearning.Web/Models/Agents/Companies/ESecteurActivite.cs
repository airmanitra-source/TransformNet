namespace MachineLearning.Web.Models.Agents.Companies;

/// <summary>
/// Secteurs d'activité simplifiés des entreprises.
/// Calibrés sur la structure du PIB de Madagascar (INSTAT 2024).
/// </summary>
public enum ESecteurActivite
{
    /// <summary>Agriculture, élevage, pêche (~25% du PIB)</summary>
    Agriculture,
    Textiles,
    Commerces,
    Services,
    SecteurMinier,
    /// <summary>BTP, construction, travaux publics (~5% du PIB)</summary>
    Construction
}
