using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace DataAccessLayer.Context;

/// <summary>
/// Ensures PostgreSQL timestamptz parameters are always sent as UTC.
/// API request binding produces <see cref="DateTimeKind.Unspecified"/> for
/// date-only/query-string values, which Npgsql 6+ correctly rejects.
/// </summary>
public sealed class UtcDateTimeCommandInterceptor : DbCommandInterceptor
{
    public override InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result)
    {
        NormalizeDateTimeParameters(command);
        return result;
    }

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        NormalizeDateTimeParameters(command);
        return ValueTask.FromResult(result);
    }

    public override InterceptionResult<object> ScalarExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<object> result)
    {
        NormalizeDateTimeParameters(command);
        return result;
    }

    public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<object> result,
        CancellationToken cancellationToken = default)
    {
        NormalizeDateTimeParameters(command);
        return ValueTask.FromResult(result);
    }

    public override InterceptionResult<int> NonQueryExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<int> result)
    {
        NormalizeDateTimeParameters(command);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        NormalizeDateTimeParameters(command);
        return ValueTask.FromResult(result);
    }

    private static void NormalizeDateTimeParameters(DbCommand command)
    {
        foreach (DbParameter parameter in command.Parameters)
        {
            if (parameter.Value is DateTime value)
            {
                parameter.Value = Normalize(value);
            }
        }
    }

    private static DateTime Normalize(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };
}
