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

public class MenuCategoryDao(IConnectionFactory connectionFactory) : IMenuCategoryDao
{
    private readonly AdoTemplate template = new AdoTemplate(connectionFactory);

    private MenuCategory MapRowToMenuCategory(IDataRecord row)
    {
        return new MenuCategory(
            id: (int)row["id"],
            name: (string)row["name"]);
    }

    public async Task<IEnumerable<MenuCategory>> FindAllAsync(CancellationToken cancellationToken = default)
    {
        return await template.QueryAsync("select * from MenuCategory", MapRowToMenuCategory, [], cancellationToken);
    }

    public async Task<MenuCategory?> FindByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await template.QuerySingleAsync(
            "select * from MenuCategory where id=@id",
            MapRowToMenuCategory,
            [new QueryParameter("@id", id)],
            cancellationToken);
    }

    public async Task<int?> InsertAsync(MenuCategory menuCategory, CancellationToken cancellationToken = default)
    {
        return await template.QuerySingleAsync(
            """
            insert into MenuCategory
            (name)
            output inserted.id
            values
            (@name)
            """,
            row => (int)row[0],
            [new QueryParameter("@name", menuCategory.Name)],
            cancellationToken);
    }

    public async Task<bool> UpdateAsync(MenuCategory menuCategory, CancellationToken cancellationToken = default)
    {
        return await template.ExecuteAsync(
            """
            update MenuCategory
            set name=@name
            where id=@id
            """,
            [
            new QueryParameter("@name", menuCategory.Name),
            new QueryParameter("@id", menuCategory.Id)
            ],
            cancellationToken
        ) == 1;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        return await template.ExecuteAsync(
            "delete from MenuCategory where id=@id",
            [
            new QueryParameter("@id", id)
            ],
            cancellationToken
        ) == 1;
    }
}
