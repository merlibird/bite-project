using Bite.Dal.Common;
using Bite.Dal.Interface;
using Bite.Domain;
using System.Data;
using System.Linq;
using System.Transactions;

namespace Bite.Dal.Ado;

public class MenuItemDao(IConnectionFactory connectionFactory) : IMenuItemDao
{
    private readonly AdoTemplate template = new AdoTemplate(connectionFactory);

    private static readonly string MenuItemSelect =
        """
        select
            mi.id,
            mi.restaurant_id,
            mi.name,
            mi.description,
            mi.price,
            mi.is_active,
            mi.created_at,
            mi.updated_at,
            STRING_AGG(CONVERT(nvarchar(20), mimc.menu_category_id), ',') as menu_category_ids
        from MenuItem mi
        left join MenuItemMenuCategory mimc on mimc.menu_item_id = mi.id
        """;

    private static readonly string MenuItemGroupBy =
        """
        group by
            mi.id,
            mi.restaurant_id,
            mi.name,
            mi.description,
            mi.price,
            mi.is_active,
            mi.created_at,
            mi.updated_at
        """;

    private static IReadOnlyCollection<int> MapMenuCategoryIds(IDataRecord row)
    {
        if (row["menu_category_ids"] is not string categoryIds)
            return [];

        return categoryIds
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(int.Parse)
            .ToArray();
    }

    private MenuItem MapRowToMenuItem(IDataRecord row)
    {
        return new MenuItem(
            id: (int)row["id"],
            restaurantId: (int)row["restaurant_id"],
            name: (string)row["name"],
            description: row["description"] as string,
            price: (decimal)row["price"],
            isActive: (bool)row["is_active"],
            createdAt: (DateTime)row["created_at"],
            updatedAt: (DateTime)row["updated_at"],
            menuCategoryIds: MapMenuCategoryIds(row));
    }

    private static object NullableParam(object? value) => value ?? DBNull.Value;

    public async Task<IEnumerable<MenuItem>> FindAllAsync(CancellationToken cancellationToken = default)
    {
        return await template.QueryAsync(
            $"""
            {MenuItemSelect}
            {MenuItemGroupBy}
            """,
            MapRowToMenuItem,
            [],
            cancellationToken);
    }

    public async Task<IEnumerable<MenuItem>> FindAllByRestaurantIdAsync(int restaurantId, CancellationToken cancellationToken = default)
    {
        return await template.QueryAsync(
            $"""
            {MenuItemSelect}
            where mi.restaurant_id = @restaurantId
            {MenuItemGroupBy}
            """,
            MapRowToMenuItem,
            [new QueryParameter("@restaurantId", restaurantId)],
            cancellationToken);
    }

    public async Task<IEnumerable<MenuItem>> FindAllByMenuCategoryIdAsync(int menuCategoryId, CancellationToken cancellationToken = default)
    {
        return await template.QueryAsync(
            $"""
            {MenuItemSelect}
            where exists (
                select 1
                from MenuItemMenuCategory mimcFilter
                where mimcFilter.menu_item_id = mi.id
                    and mimcFilter.menu_category_id = @menuCategoryId
            )
            {MenuItemGroupBy}
            """,
            MapRowToMenuItem,
            [new QueryParameter("@menuCategoryId", menuCategoryId)],
            cancellationToken);
    }

    public async Task<int> InsertAsync(MenuItem menuItem, CancellationToken cancellationToken = default)
    {
        string categoryIds = string.Join(",", menuItem.MenuCategoryIds.Distinct());

        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        int menuItemId = await template.QuerySingleAsync(
            """
            insert into MenuItem
                (restaurant_id, name, description, price, is_active)
            output inserted.id
            values
                (@restaurantId, @name, @description, @price, @isActive);
            """,
            row => (int)row[0],
            [
                new QueryParameter("@restaurantId", menuItem.RestaurantId),
                new QueryParameter("@name", menuItem.Name),
                new QueryParameter("@description", NullableParam(menuItem.Description)),
                new QueryParameter("@price", menuItem.Price),
                new QueryParameter("@isActive", menuItem.IsActive)
            ],
            cancellationToken);

        if (categoryIds != string.Empty)
        {
            await template.ExecuteAsync(
                """
                insert into MenuItemMenuCategory (menu_item_id, menu_category_id, restaurant_id)
                select
                    @menuItemId,
                    convert(int, value),
                    @restaurantId
                from string_split(@categoryIds, ',');
                """,
                [
                    new QueryParameter("@menuItemId", menuItemId),
                    new QueryParameter("@restaurantId", menuItem.RestaurantId),
                    new QueryParameter("@categoryIds", categoryIds)
                ],
                cancellationToken);
        }

        scope.Complete();
        return menuItemId;
    }

    public async Task<bool> UpdateAsync(MenuItem menuItem, CancellationToken cancellationToken = default)
    {
        return await template.ExecuteAsync(
            """
            update MenuItem
            set name = @name,
                description = @description,
                price = @price,
                is_active = @isActive
            where id = @id
            """,
            [
                new QueryParameter("@name", menuItem.Name),
                new QueryParameter("@description", NullableParam(menuItem.Description)),
                new QueryParameter("@price", menuItem.Price),
                new QueryParameter("@isActive", menuItem.IsActive),
                new QueryParameter("@id", menuItem.Id)
            ],
            cancellationToken
        ) == 1;
    }

    public async Task<bool> SetMenuCategoriesAsync(int menuItemId, IEnumerable<int> menuCategoryIds, CancellationToken cancellationToken = default)
    {
        var distinctIds = menuCategoryIds.Distinct().ToList();
        string categoryIds = string.Join(",", distinctIds);
        int expectedCount = distinctIds.Count;

        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        await template.ExecuteAsync(
            "delete from MenuItemMenuCategory where menu_item_id = @menuItemId;",
            [new QueryParameter("@menuItemId", menuItemId)],
            cancellationToken);

        if (categoryIds != string.Empty)
        {
            await template.ExecuteAsync(
                """
                insert into MenuItemMenuCategory (menu_item_id, menu_category_id, restaurant_id)
                select
                    @menuItemId,
                    convert(int, s.value),
                    mi.restaurant_id
                from string_split(@categoryIds, ',') s
                join MenuItem mi on mi.id = @menuItemId;
                """,
                [
                    new QueryParameter("@menuItemId", menuItemId),
                    new QueryParameter("@categoryIds", categoryIds)
                ],
                cancellationToken);
        }

        int? assignedCount = await template.QuerySingleAsync(
            "select count(*) from MenuItemMenuCategory where menu_item_id = @menuItemId;",
            row => (int)row[0],
            [new QueryParameter("@menuItemId", menuItemId)],
            cancellationToken);

        scope.Complete();
        return assignedCount == expectedCount;
    }
}