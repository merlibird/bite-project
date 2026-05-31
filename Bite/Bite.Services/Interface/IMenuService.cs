using Bite.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bite.Services.Interface;
public interface IMenuService
{
    public Task<Menu?> GetMenuAsync(int restaurantId, CancellationToken cancellationToken = default);
}

