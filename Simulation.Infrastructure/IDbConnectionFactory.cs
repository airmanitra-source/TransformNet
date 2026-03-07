using System.Data;

namespace Simulation.Infrastructure;

/// <summary>
/// Fabrique de connexions SQL Server injectée dans tous les providers.
/// Permet de changer le moteur de BD sans modifier les providers.
/// </summary>
public interface IDbConnectionFactory
{
    /// <summary>Ouvre une connexion SQL Server à partir de la chaîne de connexion configurée.</summary>
    IDbConnection CreateConnection();
}
