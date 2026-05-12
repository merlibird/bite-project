using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace Bite.Dal.Common;
public static class DbUtil
{
    public static void RegisterAdoProviders()
    {
        DbProviderFactories.RegisterFactory("Microsoft.Data.SqlClient", Microsoft.Data.SqlClient.SqlClientFactory.Instance);
    }
}
