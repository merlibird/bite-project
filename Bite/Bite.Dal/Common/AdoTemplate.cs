using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

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
            param.Value = p.Value;
            command.Parameters.Add(param);
        }
    }
    public async Task<IEnumerable<T>> QueryAsync<T>(
        string sql,
        RowMapper<T> rowMapper,
        params QueryParameter[] parameters)
    {
        using DbConnection connection = connectionFactory.CreateConnection();

        using DbCommand command = connection.CreateCommand();
        command.CommandText = sql;
        AddParameters(command, parameters);

        await using DbDataReader reader = await command.ExecuteReaderAsync();

        IList<T> items = [];
        while (reader.Read())
        {
            items.Add(rowMapper(reader));
        }

        return items;
    }

    public async Task<T?> QuerySingleAsync<T>(
        string sql,
        RowMapper<T> rowMapper,
        params QueryParameter[] parameters)
    {
        return (await this.QueryAsync(sql, rowMapper, parameters)).SingleOrDefault();
    }

    public async Task<int> ExecuteAsync(string sql, params QueryParameter[] parameters)
    {
        using DbConnection connection = connectionFactory.CreateConnection();
        using DbCommand command = connection.CreateCommand();
        command.CommandText = sql;
        AddParameters(command, parameters);
        return await command.ExecuteNonQueryAsync();
    }
}
