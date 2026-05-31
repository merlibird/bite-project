using Bite.Api.Dtos;
using Bite.Dal.Ado;
using Bite.Dal.Interface;
using Bite.Domain;
using Bite.Services.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bite.Services.Implementation;

public class MenuService : IMenuService
{
    private readonly IMenuCategoryDao menuCategoryDao;
    private readonly IMenuItemDao menuItemDao;

    public MenuService(IMenuCategoryDao menuCategoryDao, IMenuItemDao menuItemDao)
    {
        this.menuCategoryDao = menuCategoryDao;
        this.menuItemDao = menuItemDao;
    }

    public async Task<MenuDto> GetMenuAsync(int restaurantId)
    {
        var categories = await menuCategoryDao.FindAllByRestaurantIdAsync(restaurantId);
        var items = await menuItemDao.FindAllByRestaurantIdAsync(restaurantId);

        return new MenuDto
        {
            RestaurantId = restaurantId,
            Categories = categories.Select(category => new MenuCategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Items = items
                    .Where(item => item.MenuCategoryIds.Contains(category.Id))
                    .Select(item => new MenuItemDto
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Description = item.Description,
                        Price = item.Price,
                        IsActive = item.IsActive
                    })
                    .ToList()
            }).ToList()
        };
    }


}

