using Bite.Dal.Common;
using Bite.Dal.Interface;
using Bite.Domain;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading;

namespace Bite.Dal.Ado;

public class MenuItemDao(IConnectionFactory connectionFactory) : IMenuItemDao
{
    private readonly AdoTemplate template = new AdoTemplate(connectionFactory);

    private MenuItem MapRowToMenuItem(IDataRecord row)
    {
        return new MenuItem(
            id: (int)row["id"],
            restaurantId: (int)row["restaurant_id"],
            categoryId: (int)row["category_id"],
            name: (string)row["name"],
            description: row["description"] as string,
            price: (decimal)row["price"],
            isActive: (bool)row["is_active"],
            createdAt: (DateTime)row["created_at"],
            updatedAt: (DateTime)row["updated_at"]);
    }

    public async Task<IEnumerable<MenuItem>> FindAllAsync(CancellationToken cancellationToken = default)
    {
        return await template.QueryAsync("select * from MenuItem", MapRowToMenuItem, [], cancellationToken);
    }

    public async Task<IEnumerable<MenuItem>> FindAllByRestaurantIdAsync(int restaurantId, CancellationToken cancellationToken = default)
    {
        return await template.QueryAsync(
            "select * from MenuItem where restaurant_id=@restaurantId",
            MapRowToMenuItem,
            [new QueryParameter("@restaurantId", restaurantId)],
            cancellationToken);
    }

    public async Task<int?> InsertAsync(MenuItem menuItem, CancellationToken cancellationToken = default)
    {
        return await template.QuerySingleAsync(
            """
            insert into MenuItem
            (restaurant_id, category_id, name, description, price, is_active)
            output inserted.id
            values
            (@restaurantId, @categoryId, @name, @description, @price, @isActive)
            """,
            row => (int)row[0],
            [
            new QueryParameter("@restaurantId", menuItem.RestaurantId),
            new QueryParameter("@categoryId", menuItem.CategoryId),
            new QueryParameter("@name", menuItem.Name),
            new QueryParameter("@description", menuItem.Description),
            new QueryParameter("@price", menuItem.Price),
            new QueryParameter("@isActive", menuItem.IsActive)
            ],
            cancellationToken);
    }

    public async Task<bool> UpdateAsync(MenuItem menuItem, CancellationToken cancellationToken = default)
    {
        return await template.ExecuteAsync(
            """
            update MenuItem
            set category_id=@categoryId, name=@name, description=@description, is_active=@isActive
            where id=@id
            """,
            [
            new QueryParameter("@categoryId", menuItem.CategoryId),
            new QueryParameter("@name", menuItem.Name),
            new QueryParameter("@description", menuItem.Description),
            new QueryParameter("@isActive", menuItem.IsActive),
            new QueryParameter("@id", menuItem.Id)
            ],
            cancellationToken
        ) == 1;
    }
}
