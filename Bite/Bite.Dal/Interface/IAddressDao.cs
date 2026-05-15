using Bite.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bite.Dal.Interface;

public interface IAddressDao
{
    Task<Address?> FindByIdAsync(int id);

    Task<int?> InsertAsync(Address address);

    Task<bool> UpdateAsync(Address address);

    Task<bool> DeleteAsync(int id);
}