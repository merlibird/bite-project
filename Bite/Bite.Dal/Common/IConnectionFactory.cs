using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Threading;

namespace Bite.Dal.Common
{
    public interface IConnectionFactory
    {
        public string ConnectionString { get; }
        public string ProviderName { get; }

        Task<DbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default);
    }
}
