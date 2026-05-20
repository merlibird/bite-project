using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading;

namespace Bite.Dal.Common;

public class AdoTemplate(IConnectionFactory connectionFactory)
{
    public delegate T RowMapper<T>(IDataRecord row);

    private void AddParameters(DbCommand command, QueryParameter[] parameters)
    {
        foreach (var p in parameters)
        {
            DbParameter param = command.CreateParameter();
            param.ParameterName = p.Name;
            param.Value = p.Value ?? DBNull.Value;
            command.Parameters.Add(param);
        }
    }
    public async Task<IEnumerable<T>> QueryAsync<T>(
        string sql,
        RowMapper<T> rowMapper,
        QueryParameter[] parameters,
        CancellationToken cancellationToken = default)
    {
        await using DbConnection connection = await connectionFactory.CreateConnectionAsync(cancellationToken);

        using DbCommand command = connection.CreateCommand();
        command.CommandText = sql;
        AddParameters(command, parameters);

        await using DbDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        IList<T> items = [];
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(rowMapper(reader));
        }

        return items;
    }

    public async Task<T?> QuerySingleAsync<T>(
        string sql,
        RowMapper<T> rowMapper,
        QueryParameter[] parameters,
        CancellationToken cancellationToken = default)
    {
        return (await this.QueryAsync(sql, rowMapper, parameters, cancellationToken)).SingleOrDefault();
    }

    public async Task<int> ExecuteAsync(
        string sql,
        QueryParameter[] parameters,
        CancellationToken cancellationToken = default)
    {
        await using DbConnection connection = await connectionFactory.CreateConnectionAsync(cancellationToken);
        using DbCommand command = connection.CreateCommand();
        command.CommandText = sql;
        AddParameters(command, parameters);
        return await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
