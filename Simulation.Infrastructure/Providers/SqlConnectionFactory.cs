using System.Data;
using Microsoft.Data.SqlClient;

namespace Simulation.Infrastructure.Providers;

/// <summary>
/// Implémentation de la fabrique de connexions SQL Server.
/// La chaîne de connexion est lue depuis la configuration ASP.NET Core.
/// </summary>
public sealed class SqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("La chaîne de connexion ne peut pas être vide.", nameof(connectionString));
        _connectionString = connectionString;
    }

    public IDbConnection CreateConnection()
    {
        var connection = new SqlConnection(_connectionString);
        connection.Open();
        return connection;
    }
}
