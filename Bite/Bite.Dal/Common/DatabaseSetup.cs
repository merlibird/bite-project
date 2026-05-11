using System;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;

namespace Bite.Dal.Common;

public class DatabaseSetup
{
    private string masterConnectionString; // = @"Server=(localdb)\MSSQLLocalDB;Database=master;Trusted_Connection=True;";
    private string targetConnectionString; // = @"Server=(localdb)\MSSQLLocalDB;Database=BiteTestDb;Trusted_Connection=True;";

    public DatabaseSetup(DatabaseConfig config)
    {
        masterConnectionString = config.MasterConnectionString;
        targetConnectionString = config.TargetConnectionString;
    }


    public void InitializeDatabase()
    {
        CreateDatabase();
        ResetTables();
        CreateTables();
    }

    private void CreateDatabase()
    {
        using var connection = new SqlConnection(masterConnectionString);
        connection.Open();
        string sql = @"
            IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'BiteTestDb')
            CREATE DATABASE BiteTestDb";

        using var command = new SqlCommand(sql, connection);
        command.ExecuteNonQuery();
    }

    private void CreateTables()
    {
        var sql = ReadEmbeddedSql("testing-db-tsql.sql");

        using var connection = new SqlConnection(targetConnectionString);
        connection.Open();

        // Statements am Semikolon trennen und einzeln ausführen
        foreach (var stmt in sql.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (string.IsNullOrWhiteSpace(stmt)) continue;

            using var command = new SqlCommand(stmt, connection);
            command.ExecuteNonQuery();
        }

        Console.WriteLine("Alle Tabellen wurden verarbeitet.");
    }

    private string ReadEmbeddedSql(string fileName)
    {
        var assembly = typeof(DatabaseSetup).Assembly;

        // DEBUG: Ressourcennamen 
        //foreach (var name in assembly.GetManifestResourceNames())
        //    Console.WriteLine($"Gefundene Ressource: {name}");

        // Ressourcenname = Namespace + Ordner + Dateiname
        var resourceName = assembly
            .GetManifestResourceNames()
            .Single(n => n.EndsWith(fileName));

        using var stream = assembly.GetManifestResourceStream(resourceName)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private void ResetTables()
    {
        using var connection = new SqlConnection(targetConnectionString);
        connection.Open();

        // Reihenfolge: zuerst abhängige Tabellen (Foreign Keys beachten :( )
        string sql = @"
        IF OBJECT_ID('DeliveryFeeRule',  'U') IS NOT NULL DROP TABLE DeliveryFeeRule;
        IF OBJECT_ID('DeliveryZone',     'U') IS NOT NULL DROP TABLE DeliveryZone;
        IF OBJECT_ID('MenuItem',         'U') IS NOT NULL DROP TABLE MenuItem;
        IF OBJECT_ID('MenuCategory',     'U') IS NOT NULL DROP TABLE MenuCategory;
        IF OBJECT_ID('OpeningHourSlot',  'U') IS NOT NULL DROP TABLE OpeningHourSlot;
        IF OBJECT_ID('Restaurant',       'U') IS NOT NULL DROP TABLE Restaurant;
        IF OBJECT_ID('Address',          'U') IS NOT NULL DROP TABLE Address;
        IF OBJECT_ID('Menu',             'U') IS NOT NULL DROP TABLE Menu;";

        foreach (var stmt in sql.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            using var cmd = new SqlCommand(stmt.Trim(), connection);
            cmd.ExecuteNonQuery();
        }
    }
}