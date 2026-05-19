using Bite.Dal.Common;
using Bite.Dal.Interface;
using Bite.Domain;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Bite.Dal.Ado;

public class MenuCategoryDao(IConnectionFactory connectionFactory) : IMenuCategoryDao
{
    private readonly AdoTemplate template = new AdoTemplate(connectionFactory);

    private MenuCategory MapRowToMenuCategory(IDataRecord row)
    {
        return new MenuCategory(
            id: (int)row["id"],
            restaurantId: (int)row["restaurant_id"],
            name: (string)row["name"]);
    }

    public async Task<IEnumerable<MenuCategory>> FindAllAsync()
    {
        return await template.QueryAsync("select * from MenuCategory", MapRowToMenuCategory);
    }

    public async Task<IEnumerable<MenuCategory>> FindAllByRestaurantIdAsync(int restaurantId)
    {
        return await template.QueryAsync(
            "select * from MenuCategory where restaurant_id=@restaurantId",
            MapRowToMenuCategory,
            new QueryParameter("@restaurantId", restaurantId));
    }

    public async Task<MenuCategory?> FindByIdAsync(int id)
    {
        return await template.QuerySingleAsync(
            "select * from MenuCategory where id=@id",
            MapRowToMenuCategory,
            new QueryParameter("@id", id));
    }

    public async Task<int?> InsertAsync(MenuCategory menuCategory)
    {
        return await template.QuerySingleAsync(
            """
            insert into MenuCategory
            (restaurant_id, name)
            output inserted.id
            values
            (@restaurantId, @name)
            """,
            row => (int)row[0],
            new QueryParameter("@restaurantId", menuCategory.RestaurantId),
            new QueryParameter("@name", menuCategory.Name));
    }

    public async Task<bool> UpdateAsync(MenuCategory menuCategory)
    {
        return await template.ExecuteAsync(
            """
            update MenuCategory
            set restaurant_id=@restaurantId, name=@name
            where id=@id
            """,
            new QueryParameter("@restaurantId", menuCategory.RestaurantId),
            new QueryParameter("@name", menuCategory.Name),
            new QueryParameter("@id", menuCategory.Id)
        ) == 1;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await template.ExecuteAsync(
            "delete from MenuCategory where id=@id",
            new QueryParameter("@id", id)
        ) == 1;
    }
}
