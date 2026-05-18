using Bite.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bite.Dal.Interface;

public interface IOpeningHourSlotDao
{
    Task<IEnumerable<OpeningHourSlot>> FindByRestaurantIdAsync(int restaurantId);

    Task<int?> InsertAsync(OpeningHourSlot openingHourSlot);

    Task<bool> UpdateAsync(OpeningHourSlot openingHourSlot);

    Task<bool> DeleteAsync(int id);
}