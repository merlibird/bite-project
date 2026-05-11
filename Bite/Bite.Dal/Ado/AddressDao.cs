using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Bite.Domain;
using Bite.Dal.Common;

namespace Bite.Dal.Ado;

public class AddressDao
{
    private readonly string connectionString;

    public AddressDao(DatabaseConfig config)
    {
        connectionString = config.TargetConnectionString;
    }

    public async Task<Address?> FindByIdAsync(int id)
    {
        const string sql = @"
            SELECT id, street, number, zip_code, city, country,
                   additional_info, longitude, latitude
            FROM Address
            WHERE id = @id";

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@id", id);

        await using var reader = await cmd.ExecuteReaderAsync();
        return reader.Read() ? MapAddress(reader) : null;
    }

    public async Task<int> InsertAsync(Address address)
    {
        const string sql = @"
            INSERT INTO Address (street, number, zip_code, city, country,
                                 additional_info, longitude, latitude)
            OUTPUT INSERTED.id
            VALUES (@street, @number, @zip, @city, @country,
                    @info, @lon, @lat)";

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@street", address.Street);
        cmd.Parameters.AddWithValue("@number", address.Number);
        cmd.Parameters.AddWithValue("@zip", address.ZipCode);
        cmd.Parameters.AddWithValue("@city", address.City);
        cmd.Parameters.AddWithValue("@country", address.Country);
        cmd.Parameters.AddWithValue("@info", (object?)address.AdditionalInfo ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@lon", address.Longitude);
        cmd.Parameters.AddWithValue("@lat", address.Latitude);

        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<bool> UpdateAsync(Address address)
    {
        const string sql = @"
            UPDATE Address
            SET street          = @street,
                number          = @number,
                zip_code        = @zip,
                city            = @city,
                country         = @country,
                additional_info = @info,
                longitude       = @lon,
                latitude        = @lat
            WHERE id = @id";

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@street", address.Street);
        cmd.Parameters.AddWithValue("@number", address.Number);
        cmd.Parameters.AddWithValue("@zip", address.ZipCode);
        cmd.Parameters.AddWithValue("@city", address.City);
        cmd.Parameters.AddWithValue("@country", address.Country);
        cmd.Parameters.AddWithValue("@info", (object?)address.AdditionalInfo ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@lon", address.Longitude);
        cmd.Parameters.AddWithValue("@lat", address.Latitude);
        cmd.Parameters.AddWithValue("@id", address.Id);

        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    private static Address MapAddress(SqlDataReader r) => new()
    {
        Id = r.GetInt32(0),
        Street = r.GetString(1),
        Number = r.GetString(2),
        ZipCode = r.GetString(3),
        City = r.GetString(4),
        Country = r.GetString(5),
        AdditionalInfo = r.IsDBNull(6) ? null : r.GetString(6),
        Longitude = r.GetDouble(7),
        Latitude = r.GetDouble(8),
    };
}
