using Bite.Api.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bite.Services.Interface;
public interface IMenuService
{
    public Task<MenuDto> GetMenuAsync(int restaurantId);
}

