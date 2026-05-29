using Bite.Dal.Common;
using Bite.Dal.Interface;
using Bite.Domain;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading;

namespace Bite.Dal.Ado;

public class RestaurantDao(IConnectionFactory connectionFactory) : IRestaurantDao
{
    private readonly AdoTemplate template = new AdoTemplate(connectionFactory);

    private Restaurant MapRowToRestaurant(IDataRecord row)
    {
        return new Restaurant(
            id: (int)row["id"],
            name: (string)row["name"],
            addressId: (int)row["address_id"],
            webhookUrl: (string)row["webhook_url"],
            apiKey: (string)row["api_key"],
            titleImagePath: row["title_image_path"] as string,
            createdAt: (DateTime)row["created_at"],
            updatedAt: (DateTime)row["updated_at"]);
    }

    public async Task<IEnumerable<Restaurant>> FindAllAsync(CancellationToken cancellationToken = default)
    {
        return await template.QueryAsync("select * from Restaurant", MapRowToRestaurant, [], cancellationToken);
    }

    public async Task<Restaurant?> FindByApiKeyAsync(string hashedApiKey, CancellationToken cancellationToken = default)
    {
        return await template.QuerySingleAsync(
            "select * from Restaurant where api_key = @apiKey",
            MapRowToRestaurant,
            [new QueryParameter("@apiKey", hashedApiKey)],
            cancellationToken
        );
    }

    public async Task<Restaurant?> FindByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await template.QuerySingleAsync(
            $"select * from Restaurant where id=@id", 
            MapRowToRestaurant, 
            [
            new QueryParameter("@id", id)
            ],
            cancellationToken
        );
    }

    public async Task<int> InsertAsync(Restaurant restaurant, CancellationToken cancellationToken = default)
    {
        return await template.QuerySingleAsync(
            """
            insert into Restaurant
            (name, address_id, webhook_url, title_image_path, api_key)
            output inserted.id
            values
            (@name, @addressId, @webhook, @image, @apiKey)
            """,
            row => (int)row[0],
            [
            new QueryParameter("@name", restaurant.Name),
            new QueryParameter("@addressId", restaurant.AddressId),
            new QueryParameter("@webhook", restaurant.WebhookUrl),
            new QueryParameter("@image", restaurant.TitleImagePath),
            new QueryParameter("@apiKey", restaurant.ApiKey)
            ],
            cancellationToken
        );
    }

    public async Task<bool> UpdateAsync(Restaurant restaurant, CancellationToken cancellationToken = default)
    {
        return await template.ExecuteAsync(
            """
            update Restaurant
            set name=@name, address_id=@addressId, webhook_url=@webhook, title_image_path=@image
            where id=@id
            """,
            [
            new QueryParameter("@name", restaurant.Name),
            new QueryParameter("@addressId", restaurant.AddressId),
            new QueryParameter("@webhook", restaurant.WebhookUrl),
            new QueryParameter("@image", restaurant.TitleImagePath),
            new QueryParameter("@id", restaurant.Id)
            ],
            cancellationToken
        ) == 1;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        return await template.ExecuteAsync(
            "delete from Restaurant where id=@id",
            [
            new QueryParameter("@id", id)
            ],
            cancellationToken
        ) == 1;
    }
}
