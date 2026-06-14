using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Restaurant.Infrastructure;

/// <summary>
/// Applies a SMB-safe SQLite configuration to every connection:
/// <list type="bullet">
///   <item>rollback journal instead of WAL — WAL's shared-memory file doesn't work on Azure
///   App Service's network-backed <c>/home</c> share, where it makes writes fail/lock;</item>
///   <item>a busy timeout so brief lock contention retries instead of throwing.</item>
/// </list>
/// </summary>
public sealed class SqlitePragmaInterceptor : DbConnectionInterceptor
{
    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
        => Apply(connection);

    public override Task ConnectionOpenedAsync(DbConnection connection, ConnectionEndEventData eventData, CancellationToken cancellationToken = default)
    {
        Apply(connection);
        return Task.CompletedTask;
    }

    static void Apply(DbConnection connection)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "PRAGMA journal_mode=DELETE; PRAGMA busy_timeout=5000;";
        cmd.ExecuteNonQuery();
    }
}
