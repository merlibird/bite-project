using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace Bite.Dal.Common
{
    public interface IConnectionFactory
    {
        public string ConnectionString { get; }
        public string ProviderName { get; }

        DbConnection CreateConnection();
    }
}
