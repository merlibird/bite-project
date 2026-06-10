using Bite.Domain;

using System.Threading;

namespace Bite.Dal.Interface;

public interface IStatusHistoryEntryDao
{
    Task<int> InsertAsync(StatusHistoryEntry entry, CancellationToken cancellationToken = default);
}
