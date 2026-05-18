using Bite.Dal.Common;
using Bite.Dal.Interface;
using Bite.Domain;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Text;

namespace Bite.Dal.Ado;

public class AddressDao(IConnectionFactory connectionFactory) : IAddressDao
{
    private readonly AdoTemplate template = new AdoTemplate(connectionFactory);

    private Address MapRowToAddress(IDataRecord row)
    {
        return new Address(
            id: (int)row["id"],
            street: (string)row["street"],
            number: (string)row["number"],
            zipCode: (string)row["zip_code"],
            city: (string)row["city"],
            country: (string)row["country"],
            additionalInfo: row["additional_info"] as string,
            longitude: (double)row["longitude"],
            latitude: (double)row["latitude"]);
    }

    public async Task<Address?> FindByIdAsync(int id)
    {
        return await template.QuerySingleAsync(
            "select * from Address where id=@id",
            MapRowToAddress,
            new QueryParameter("@id", id));
    }

    public async Task<int?> InsertAsync(Address address)
    {
        return await template.QuerySingleAsync(
            """
            insert into Address
            (street, number, zip_code, city, country, additional_info, longitude, latitude)
            output inserted.id
            values
            (@street, @number, @zipCode, @city, @country, @additionalInfo, @longitude, @latitude)
            """,
            row => (int)row[0],
            new QueryParameter("@street", address.Street),
            new QueryParameter("@number", address.Number),
            new QueryParameter("@zipCode", address.ZipCode),
            new QueryParameter("@city", address.City),
            new QueryParameter("@country", address.Country),
            new QueryParameter("@additionalInfo", address.AdditionalInfo),
            new QueryParameter("@longitude", address.Longitude),
            new QueryParameter("@latitude", address.Latitude));
    }

    public async Task<bool> UpdateAsync(Address address)
    {
        return await template.ExecuteAsync(
            """
            update Address
            set additional_info=@additionalInfo
            where id=@id
            """,
            new QueryParameter("@additionalInfo", address.AdditionalInfo),
            new QueryParameter("@id", address.Id)
        ) == 1;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await template.ExecuteAsync(
            "delete from Address where id=@id",
            new QueryParameter("@id", id)
        ) == 1;
    }
}
