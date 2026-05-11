using Microsoft.Extensions.Configuration;

namespace Bite.Dal.Common;

public class DatabaseConfig
{
    public string MasterConnectionString { get; }
    public string TargetConnectionString { get; }

    public DatabaseConfig(IConfiguration config)
    {
        var mode = config["DatabaseMode"] ?? "LocalDb"; // "LocalDb" oder "SqlServer"

        MasterConnectionString = config[$"ConnectionStrings:{mode}:Master"]
            ?? throw new InvalidOperationException($"Master connection string für '{mode}' nicht gefunden.");

        TargetConnectionString = config[$"ConnectionStrings:{mode}:Target"]
            ?? throw new InvalidOperationException($"Target connection string für '{mode}' nicht gefunden.");

        Console.WriteLine($"Datenbankmode: {mode}");
    }
}