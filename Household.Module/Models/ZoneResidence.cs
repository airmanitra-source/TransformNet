namespace Household.Module.Models;

/// <summary>
/// Zone de résidence du ménage.
/// Madagascar : ~30% urbain, ~70% rural (INSTAT RGPH 2018).
/// LA distinction la plus structurante pour la simulation :
/// comportements de consommation, prix, accès aux services très différents.
/// </summary>
public enum ZoneResidence
{
    /// <summary>Ménage urbain (~30%) — accès services, prix plus élevés, salaires plus hauts</summary>
    Urbain = 0,

    /// <summary>Ménage rural (~70%) — autoconsommation agricole, prix plus bas, accès limité</summary>
    Rural = 1
}
