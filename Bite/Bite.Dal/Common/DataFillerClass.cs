using System;
using Microsoft.Data.SqlClient;

namespace Bite.Dal.Common;

public class DataFillerClass
{
    private string connectionString;

    public DataFillerClass(DatabaseConfig config)
    {
        connectionString = config.TargetConnectionString;
    }

    public void FillTestData()
    {
        using var connection = new SqlConnection(connectionString);
        connection.Open();

        if (IsDatabaseFull(connection)) return;

        Console.WriteLine("Starte Befüllung der Testdaten...");

        // 1. MenuCategories (geteilt von allen Restaurants)
        int catPizza = InsertMenuCategory(connection, "Pizza");
        int catPasta = InsertMenuCategory(connection, "Pasta");
        int catBurger = InsertMenuCategory(connection, "Burger");
        int catSalat = InsertMenuCategory(connection, "Salat");
        int catGetraenk = InsertMenuCategory(connection, "Getränke");
        int catSushi = InsertMenuCategory(connection, "Sushi");
        int catDessert = InsertMenuCategory(connection, "Dessert");

        // 2. Drei "liebevoll" gepflegte Restaurants
        CreateNimmersatt(connection, catPizza, catPasta, catGetraenk, catDessert);
        CreateBurgerBude(connection, catBurger, catSalat, catGetraenk);
        CreateSakuraSushi(connection, catSushi, catSalat, catGetraenk, catDessert);

        // 3. Weitere 17 generische Restaurants auffüllen (damit Tests mit 20 funktionieren)
        for (int i = 4; i <= 20; i++)
        {
            int menuId = InsertMenu(connection);
            int addressId = InsertAddress(connection, $"Teststraße {i}", $"{i}a", "12345", "Teststadt",
                                          "Austria", 14.5150 + (i * 0.01), 48.3680 + (i * 0.01));
            int restId = InsertRestaurant(connection, $"Bite Palace {i}", menuId, addressId,
                                             $"https://hooks.bite.com/rest{i}",
                                             $"img/rest_{i}.png");

            // Einfache Speisekarte
            InsertMenuItem(connection, menuId, catBurger, $"Burger Classic {i}", "Rindfleisch, Salat, Tomate", 8.90m + i * 0.10m);
            InsertMenuItem(connection, menuId, catGetraenk, "Cola", "0,5l", 2.50m);

            // Öffnungszeiten Mo-Fr
            for (int day = 1; day <= 5; day++)
                InsertOpeningHourSlot(connection, restId, day, "11:00", "22:00");

            // Lieferzone
            int zoneId = InsertDeliveryZone(connection, restId, 15.00m, 10.0);
            InsertDeliveryFeeRule(connection, zoneId, 30.00m, 3.50m);
            InsertDeliveryFeeRule(connection, zoneId, 9999.00m, 0.00m);
        }

        Console.WriteLine("Testdaten erfolgreich befüllt.");
    }

    // -----------------------------------------------------------------------
    // Realistische Restaurants
    // -----------------------------------------------------------------------

    private void CreateNimmersatt(SqlConnection conn,
        int catPizza, int catPasta, int catGetraenk, int catDessert)
    {
        int menuId = InsertMenu(conn);
        int addressId = InsertAddress(conn, "Softwarepark", "11", "4232", "Hagenberg",
                                      "Austria", 14.5144, 48.3684);
        int restId = InsertRestaurant(conn, "Restaurant Nimmersatt", menuId, addressId,
                                         "https://api.nimmersatt.at/bite",
                                         "img/nimmersatt.png");

        // Speisekarte
        InsertMenuItem(conn, menuId, catPizza, "Margherita", "Tomaten, Käse", 9.50m);
        InsertMenuItem(conn, menuId, catPizza, "Al Tonno", "Tomaten, Käse, Thunfisch, Zwiebel, Oliven", 11.00m);
        InsertMenuItem(conn, menuId, catPizza, "Spinaci", "Tomaten, Käse, Spinat, Feta", 10.00m);
        InsertMenuItem(conn, menuId, catPizza, "Diavola", "Tomaten, Käse, Salami, Chili", 10.50m);
        InsertMenuItem(conn, menuId, catPasta, "Lasagne al Forno", "Mit Rinderfaschiertem", 12.00m);
        InsertMenuItem(conn, menuId, catPasta, "Spaghetti Frutti di Mare", "Meeresfrüchte, Weißweinsauce", 14.00m);
        InsertMenuItem(conn, menuId, catPasta, "Penne Arrabbiata", "Tomatensauce, Chili, Knoblauch", 9.00m);
        InsertMenuItem(conn, menuId, catGetraenk, "Cola", "0,5l", 2.50m);
        InsertMenuItem(conn, menuId, catGetraenk, "Wasser", "0,5l still", 1.80m);
        InsertMenuItem(conn, menuId, catGetraenk, "Bier", "0,5l Ottakringer", 3.50m);
        InsertMenuItem(conn, menuId, catDessert, "Tiramisu", "Hausgemacht", 5.50m);
        InsertMenuItem(conn, menuId, catDessert, "Panna Cotta", "Mit Beerensauce", 4.90m);

        // Öffnungszeiten: Di-Fr 11-15 und 17-22, Sa-So 11-14
        for (int day = 2; day <= 5; day++) // Di=2 ... Fr=5
        {
            InsertOpeningHourSlot(conn, restId, day, "11:00", "15:00");
            InsertOpeningHourSlot(conn, restId, day, "17:00", "22:00");
        }
        InsertOpeningHourSlot(conn, restId, 6, "11:00", "14:00"); // Sa
        InsertOpeningHourSlot(conn, restId, 0, "11:00", "14:00"); // So

        // Lieferbedingungen
        int zone1 = InsertDeliveryZone(conn, restId, 20.00m, 10.0);
        InsertDeliveryFeeRule(conn, zone1, 9999.00m, 0.00m); // keine Lieferkosten unter 10km

        int zone2 = InsertDeliveryZone(conn, restId, 20.00m, 20.0);
        InsertDeliveryFeeRule(conn, zone2, 30.00m, 5.00m); // bis 30€: 5€ Lieferkosten
        InsertDeliveryFeeRule(conn, zone2, 9999.00m, 0.00m); // über 30€: gratis
    }

    private void CreateBurgerBude(SqlConnection conn,
        int catBurger, int catSalat, int catGetraenk)
    {
        int menuId = InsertMenu(conn);
        int addressId = InsertAddress(conn, "Hauptstraße", "42", "1010", "Wien",
                                      "Austria", 16.3738, 48.2082);
        int restId = InsertRestaurant(conn, "Burger Bude Wien", menuId, addressId,
                                         "https://hooks.burgerbude.at/bite",
                                         "img/burgerbude.png");

        InsertMenuItem(conn, menuId, catBurger, "Classic Burger", "Rindfleisch, Salat, Tomate, Gurke", 8.90m);
        InsertMenuItem(conn, menuId, catBurger, "Cheese Burger", "Rindfleisch, Cheddar, Zwiebeln", 9.50m);
        InsertMenuItem(conn, menuId, catBurger, "BBQ Burger", "Rindfleisch, BBQ-Sauce, Bacon, Cheddar", 11.90m);
        InsertMenuItem(conn, menuId, catBurger, "Veggie Burger", "Gemüsepatty, Avocado, Tomate", 9.90m);
        InsertMenuItem(conn, menuId, catBurger, "Chicken Burger", "Knuspriges Hühnchen, Coleslaw", 10.50m);
        InsertMenuItem(conn, menuId, catSalat, "Caesar Salad", "Römerherz, Parmesan, Croutons", 7.90m);
        InsertMenuItem(conn, menuId, catSalat, "Greek Salad", "Tomate, Gurke, Feta, Oliven", 7.50m);
        InsertMenuItem(conn, menuId, catGetraenk, "Cola", "0,4l", 2.80m);
        InsertMenuItem(conn, menuId, catGetraenk, "Limo", "0,4l, div. Sorten", 2.80m);
        InsertMenuItem(conn, menuId, catGetraenk, "Milchshake", "Schoko, Vanille oder Erdbeere", 4.50m);

        // So-Sa 11-23
        for (int day = 0; day <= 6; day++)
            InsertOpeningHourSlot(conn, restId, day, "11:00", "23:00");

        int zone1 = InsertDeliveryZone(conn, restId, 15.00m, 5.0);
        InsertDeliveryFeeRule(conn, zone1, 25.00m, 2.99m);
        InsertDeliveryFeeRule(conn, zone1, 9999.00m, 0.00m);

        int zone2 = InsertDeliveryZone(conn, restId, 20.00m, 15.0);
        InsertDeliveryFeeRule(conn, zone2, 9999.00m, 4.99m);
    }

    private void CreateSakuraSushi(SqlConnection conn,
        int catSushi, int catSalat, int catGetraenk, int catDessert)
    {
        int menuId = InsertMenu(conn);
        int addressId = InsertAddress(conn, "Mariahilfer Straße", "88", "1060", "Wien",
                                      "Austria", 16.3540, 48.1970);
        int restId = InsertRestaurant(conn, "Sakura Sushi", menuId, addressId,
                                         "https://webhooks.sakura-sushi.at/orders",
                                         "img/sakura.png");

        InsertMenuItem(conn, menuId, catSushi, "Sake Nigiri (2 St.)", "Lachs", 4.80m);
        InsertMenuItem(conn, menuId, catSushi, "Maguro Nigiri (2 St.)", "Thunfisch", 5.20m);
        InsertMenuItem(conn, menuId, catSushi, "California Roll (8 St.)", "Krabben, Avocado, Gurke", 8.90m);
        InsertMenuItem(conn, menuId, catSushi, "Spicy Tuna Roll (8 St.)", "Thunfisch, Sriracha", 9.50m);
        InsertMenuItem(conn, menuId, catSushi, "Veggie Roll (8 St.)", "Gurke, Avocado, Karotte", 7.90m);
        InsertMenuItem(conn, menuId, catSushi, "Sashimi Mix (10 St.)", "Lachs, Thunfisch, Garnele", 16.90m);
        InsertMenuItem(conn, menuId, catSushi, "Dragon Roll (8 St.)", "Garnele, Avocado, Teriyaki", 12.50m);
        InsertMenuItem(conn, menuId, catSalat, "Edamame", "Gesalzen", 3.50m);
        InsertMenuItem(conn, menuId, catSalat, "Wakame Salat", "Meeresalgen, Sesam", 4.90m);
        InsertMenuItem(conn, menuId, catGetraenk, "Grüner Tee", "Kanne 0,5l", 3.20m);
        InsertMenuItem(conn, menuId, catGetraenk, "Japanisches Bier", "Asahi 0,33l", 4.00m);
        InsertMenuItem(conn, menuId, catGetraenk, "Sake", "0,1l warm", 5.50m);
        InsertMenuItem(conn, menuId, catDessert, "Mochi Eis", "Grüner Tee oder Mango", 4.50m);
        InsertMenuItem(conn, menuId, catDessert, "Dorayaki", "Japanischer Pancake mit Anko", 3.90m);

        // Di-Sa 12-15 und 17:30-22:30
        for (int day = 2; day <= 6; day++)
        {
            InsertOpeningHourSlot(conn, restId, day, "12:00", "15:00");
            InsertOpeningHourSlot(conn, restId, day, "17:30", "22:30");
        }
        //So 
        InsertOpeningHourSlot(conn, restId, 0, "12:00", "22:00"); // So durchgehend

        int zone1 = InsertDeliveryZone(conn, restId, 25.00m, 8.0);
        InsertDeliveryFeeRule(conn, zone1, 40.00m, 3.90m);
        InsertDeliveryFeeRule(conn, zone1, 9999.00m, 0.00m);

        int zone2 = InsertDeliveryZone(conn, restId, 30.00m, 15.0);
        InsertDeliveryFeeRule(conn, zone2, 9999.00m, 5.90m);
    }

    // -----------------------------------------------------------------------
    // Insert-Hilfsmethoden
    // -----------------------------------------------------------------------

    private bool IsDatabaseFull(SqlConnection conn)
    {
        using var cmd = new SqlCommand("SELECT COUNT(*) FROM Restaurant", conn);
        return (int)cmd.ExecuteScalar() > 0;
    }

    private int InsertMenu(SqlConnection conn)
    {
        using var cmd = new SqlCommand("INSERT INTO Menu DEFAULT VALUES; SELECT SCOPE_IDENTITY();", conn);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    private int InsertMenuCategory(SqlConnection conn, string name)
    {
        string sql = @"
            IF NOT EXISTS (SELECT 1 FROM MenuCategory WHERE name = @name)
                INSERT INTO MenuCategory (name) VALUES (@name);
            SELECT id FROM MenuCategory WHERE name = @name;";
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@name", name);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    private int InsertAddress(SqlConnection conn,
        string street, string number, string zip, string city,
        string country, double longitude, double latitude)
    {
        string sql = @"
            INSERT INTO Address (street, number, zip_code, city, country, longitude, latitude)
            OUTPUT INSERTED.id
            VALUES (@street, @num, @zip, @city, @country, @long, @lat)";
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@street", street);
        cmd.Parameters.AddWithValue("@num", number);
        cmd.Parameters.AddWithValue("@zip", zip);
        cmd.Parameters.AddWithValue("@city", city);
        cmd.Parameters.AddWithValue("@country", country);
        cmd.Parameters.AddWithValue("@long", longitude);
        cmd.Parameters.AddWithValue("@lat", latitude);
        return (int)cmd.ExecuteScalar();
    }

    private int InsertRestaurant(SqlConnection conn,
        string name, int menuId, int addressId, string webhookUrl, string imagePath)
    {
        string sql = @"
            INSERT INTO Restaurant (name, menu_id, address_id, webhook_url, title_image_path)
            OUTPUT INSERTED.id
            VALUES (@name, @menuId, @addressId, @webhook, @image)";
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@name", name);
        cmd.Parameters.AddWithValue("@menuId", menuId);
        cmd.Parameters.AddWithValue("@addressId", addressId);
        cmd.Parameters.AddWithValue("@webhook", webhookUrl);
        cmd.Parameters.AddWithValue("@image", imagePath);
        return (int)cmd.ExecuteScalar();
    }

    private void InsertMenuItem(SqlConnection conn,
        int menuId, int categoryId, string name, string description, decimal price)
    {
        string sql = @"
            INSERT INTO MenuItem (menu_id, category_id, name, description, price)
            VALUES (@menuId, @catId, @name, @desc, @price)";
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@menuId", menuId);
        cmd.Parameters.AddWithValue("@catId", categoryId);
        cmd.Parameters.AddWithValue("@name", name);
        cmd.Parameters.AddWithValue("@desc", description);
        cmd.Parameters.AddWithValue("@price", price);
        cmd.ExecuteNonQuery();
    }

    private void InsertOpeningHourSlot(SqlConnection conn,
        int restaurantId, int dayOfWeek, string openTime, string closeTime)
    {
        string sql = @"
            INSERT INTO OpeningHourSlot (restaurant_id, day_of_week, open_time, close_time)
            VALUES (@restId, @day, @open, @close)";
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@restId", restaurantId);
        cmd.Parameters.AddWithValue("@day", dayOfWeek);
        cmd.Parameters.AddWithValue("@open", TimeSpan.Parse(openTime));
        cmd.Parameters.AddWithValue("@close", TimeSpan.Parse(closeTime));
        cmd.ExecuteNonQuery();
    }

    private int InsertDeliveryZone(SqlConnection conn,
        int restaurantId, decimal minOrderValue, double maxDistance)
    {
        string sql = @"
            INSERT INTO DeliveryZone (restaurant_id, min_order_value, max_distance)
            OUTPUT INSERTED.id
            VALUES (@restId, @minOrder, @maxDist)";
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@restId", restaurantId);
        cmd.Parameters.AddWithValue("@minOrder", minOrderValue);
        cmd.Parameters.AddWithValue("@maxDist", maxDistance);
        return (int)cmd.ExecuteScalar();
    }

    private void InsertDeliveryFeeRule(SqlConnection conn,
        int deliveryZoneId, decimal maxOrderValue, decimal deliveryFee)
    {
        string sql = @"
            INSERT INTO DeliveryFeeRule (delivery_zone_id, max_order_value, delivery_fee)
            VALUES (@zoneId, @maxOrder, @fee)";
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@zoneId", deliveryZoneId);
        cmd.Parameters.AddWithValue("@maxOrder", maxOrderValue);
        cmd.Parameters.AddWithValue("@fee", deliveryFee);
        cmd.ExecuteNonQuery();
    }
}