using Bite.Dal;
using Bite.Dal.Ado;
using Bite.Dal.Common;
using Microsoft.Data.SqlClient;

Console.WriteLine("Testing RestaurantDao");


var configuration = ConfigurationUtil.GetConfiguration();
var connectionFactory = DefaultConnectionFactory.FromConfiguration(configuration, "BiteDbConnection", "ProviderName");

Console.WriteLine($"Connection String: {connectionFactory.ConnectionString}");

RestaurantDao restaurantDao = new RestaurantDao(connectionFactory);

IEnumerable<Bite.Domain.Restaurant> allRestaurants = await restaurantDao.FindAllAsync();

foreach (var re in allRestaurants)
{
    Console.WriteLine(re.Name);
}