using Bite.Dal;
using Bite.Dal.Ado;
using Bite.Dal.Common;
using Microsoft.Data.SqlClient;

Console.WriteLine("Starting new tests of refactoring db");

Console.WriteLine("Testing RestaurantDao");


var configuration = ConfigurationUtil.GetConfiguration();
var connectionFactory = DefaultConnectionFactory.FromConfiguration(configuration, "PersonDbConnection", "ProviderName");

// Welche DB wird verwendet?
Console.WriteLine($"Connection String: {connectionFactory.ConnectionString}");
//Console.WriteLine($"Datenbank: {connectionFactory.ConnectionString.Database}");
//Console.WriteLine($"Server: {connection.DataSource}");
//Console.WriteLine($"Status: {connection.State}");

//// Welche Tabellen existieren überhaupt?
//var cmd = new SqlCommand(
//    "SELECT TABLE_SCHEMA, TABLE_NAME FROM INFORMATION_SCHEMA.TABLES ORDER BY TABLE_NAME",
//    connectionFactory.ConnectionString);
//using var reader = cmd.ExecuteReader();
//Console.WriteLine("\n--- Vorhandene Tabellen ---");
//while (reader.Read())
//    Console.WriteLine($"  {reader["TABLE_SCHEMA"]}.{reader["TABLE_NAME"]}");





RestaurantDao restaurantDao = new RestaurantDao(connectionFactory);

IEnumerable<Bite.Domain.Restaurant> allRestaurants = restaurantDao.FindAll();

foreach (var re in allRestaurants)
{
    Console.WriteLine(re.Name);
}









//var tester1 = new DalTester(new SimplePersonDao());
//await tester1.TestFindAllAsync();

//Console.WriteLine("--DAO--");

//var configuration = ConfigurationUtil.GetConfiguration();
//var connectionFactory = DefaultConnectionFactory.FromConfiguration(configuration, "PersonDbConnection", "ProviderName");
//var tester2 = new DalTester(new AdoPersonDao(connectionFactory));
//await tester2.TestFindAllAsync();
//await tester2.TestFindByIdAsync();

//Console.WriteLine();

//await tester2.TestUpdateAsync();
//await tester2.TestTransactionAsync();