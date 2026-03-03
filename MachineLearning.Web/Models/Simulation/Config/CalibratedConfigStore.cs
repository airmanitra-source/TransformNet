namespace MachineLearning.Web.Models.Simulation.Config;

/// <summary>
/// Service singleton pour transférer une configuration calibrée
/// de la page de calibration ML vers le simulateur économique.
/// </summary>
public class CalibratedConfigStore
{
    /// <summary>Configuration calibrée en attente d'utilisation (null si aucune)</summary>
    public ScenarioConfig? PendingConfig { get; private set; }

    /// <summary>Indique si une config calibrée est prête à être consommée</summary>
    public bool HasPendingConfig => PendingConfig != null;

    /// <summary>Stocke une config calibrée pour utilisation par le simulateur</summary>
    public void Store(ScenarioConfig config)
    {
        PendingConfig = config;
    }

    /// <summary>Consomme la config en attente (la retourne puis la supprime)</summary>
    public ScenarioConfig? Consume()
    {
        var config = PendingConfig;
        PendingConfig = null;
        return config;
    }
}
