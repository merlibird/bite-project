using Bite.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Bite.Dal.Interface;

public interface IAddressDao
{
    Task<Address?> FindByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<int?> InsertAsync(Address address, CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(Address address, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
