USE [$(DbName)];
GO

IF EXISTS (SELECT 1 FROM Restaurant)
BEGIN
    -- PRINT 'Database already contains data. Skipping fill-data.sql.';
    GOTO EndOfScript;
END

SET NOCOUNT ON;

-- ============================================================
-- 1. RESTAURANT DEFINITIONS (table-driven instead of copy/paste)
-- ============================================================
-- Using a table variable lets us loop over restaurants instead of
-- repeating the same 6 INSERTs by hand for every single one.
DECLARE @Restaurants TABLE (
    Id            INT IDENTITY(1,1),
    Name          NVARCHAR(100),
    Street        NVARCHAR(100),
    Number        VARCHAR(10),
    Zip           VARCHAR(10),
    City          NVARCHAR(50),
    Lon           FLOAT,
    Lat           FLOAT,
    WebhookUrl    NVARCHAR(255),
    ImagePath     NVARCHAR(255),
    RawApiKey     VARCHAR(100),
    CategoryName  NVARCHAR(50),
    MinOrderValue DECIMAL(10,2),
    MaxDistance   FLOAT,
    OpenTime      VARCHAR(5),
    CloseTime     VARCHAR(5)
);

INSERT INTO @Restaurants (Name, Street, Number, Zip, City, Lon, Lat, WebhookUrl, ImagePath, RawApiKey, CategoryName, MinOrderValue, MaxDistance, OpenTime, CloseTime)
VALUES
-- Hagenberg / Mühlviertel cluster
('Restaurant Nimmersatt',     'Softwarepark',        '11', '4232', 'Hagenberg', 14.5144, 48.3684, 'https://api.nimmersatt.at/bite',        'img/nimmersatt.png',     'nimmersatt-api-key-2026',     'Pizza & Pasta', 15.00, 15.0, '11:00', '22:00'),
('Pizzeria Da Mario',         'Linzer Strasse',      '5',  '4232', 'Hagenberg', 14.5180, 48.3670, 'https://hooks.damario.at/bite',         'img/damario.png',        'damario-api-key-2026',        'Pizza',         12.00, 12.0, '11:00', '23:00'),
('Gasthaus Goldener Hirsch',  'Stadtplatz',          '3',  '4230', 'Pregarten', 14.5300, 48.3550, 'https://hooks.goldenerhirsch.at/bite',  'img/goldenerhirsch.png', 'goldenerhirsch-api-key-2026', 'Hausmannskost', 18.00, 20.0, '10:00', '21:00'),
('China Restaurant Lotus',    'Boehmergasse',        '7',  '4240', 'Freistadt', 14.5050, 48.5110, 'https://hooks.lotus.at/bite',           'img/lotus.png',          'lotus-api-key-2026',          'Asiatisch',     14.00, 18.0, '11:30', '21:30'),
-- Linz cluster
('Pasta Fresca Linz',         'Landstrasse',         '20', '4020', 'Linz',      14.2900, 48.3000, 'https://hooks.pastafresca.at/bite',     'img/pastafresca.png',    'pastafresca-api-key-2026',    'Pasta',         13.00, 10.0, '11:00', '22:00'),
('Linzer Wirtshaus',          'Hauptplatz',          '1',  '4020', 'Linz',      14.2860, 48.3060, 'https://hooks.linzerwirtshaus.at/bite', 'img/linzerwirtshaus.png','linzerwirtshaus-api-key-2026','Hausmannskost', 16.00, 12.0, '10:30', '22:30'),
('Curry Palace Urfahr',       'Hauptstrasse',        '45', '4040', 'Linz',      14.2850, 48.3150, 'https://hooks.currypalace.at/bite',     'img/currypalace.png',    'currypalace-api-key-2026',    'Indisch',       15.00, 9.0,  '11:30', '21:30'),
-- Wien cluster (multiple districts for realistic geo testing)
('Burger Bude Wien',          'Hauptstrasse',        '42', '1010', 'Wien',      16.3738, 48.2082, 'https://hooks.burgerbude.at/bite',      'img/burgerbude.png',     'burger-bude-api-key-2026',    'Burger',        20.00, 10.0, '11:00', '23:00'),
('Sakura Sushi',              'Mariahilfer Strasse', '88', '1060', 'Wien',      16.3540, 48.1970, 'https://webhooks.sakura-sushi.at/orders','img/sakura.png',        'sakura-sushi-api-key-2026',   'Sushi',         25.00, 12.0, '12:00', '22:00'),
('Pizzeria Napoli Wien',      'Praterstrasse',       '15', '1020', 'Wien',      16.3920, 48.2180, 'https://hooks.napoli.at/bite',          'img/napoli.png',         'napoli-api-key-2026',         'Pizza',         15.00, 8.0,  '11:00', '23:30'),
('Falafel King Ottakring',    'Thaliastrasse',       '30', '1160', 'Wien',      16.3100, 48.2120, 'https://hooks.falafelking.at/bite',     'img/falafelking.png',    'falafelking-api-key-2026',    'Vegetarisch',   10.00, 7.0,  '10:00', '22:00'),
('Steakhouse Favoriten',      'Favoritenstrasse',    '120','1100', 'Wien',      16.3750, 48.1700, 'https://hooks.steakhouse.at/bite',      'img/steakhouse.png',     'steakhouse-api-key-2026',     'Steaks',        30.00, 9.0,  '17:00', '23:00'),
('Ramen Bar Donaustadt',      'Donaustadtstrasse',   '8',  '1220', 'Wien',      16.4400, 48.2300, 'https://hooks.ramenbar.at/bite',        'img/ramenbar.png',       'ramenbar-api-key-2026',       'Asiatisch',     18.00, 11.0, '11:30', '21:00');

-- PRINT (SELECT CAST(COUNT(*) AS VARCHAR) FROM @Restaurants) + ' restaurants defined.';

-- ============================================================
-- 2. INSERT RESTAURANTS, ADDRESSES, CATEGORIES, MENU ITEMS,
--    OPENING HOURS AND DELIVERY ZONES/FEES (loop over @Restaurants)
-- ============================================================
DECLARE @rIdx INT = 1;
DECLARE @rCount INT = (SELECT COUNT(*) FROM @Restaurants);

-- Keep a map of restaurant_id -> name so later sections can look items up by name.
DECLARE @RestaurantMap TABLE (RestaurantId INT, Name NVARCHAR(100), CategoryId INT);

WHILE @rIdx <= @rCount
BEGIN
    DECLARE @name NVARCHAR(100), @street NVARCHAR(100), @number VARCHAR(10), @zip VARCHAR(10),
            @city NVARCHAR(50), @lon FLOAT, @lat FLOAT, @webhook NVARCHAR(255), @img NVARCHAR(255),
            @rawKey VARCHAR(100), @catName NVARCHAR(50), @minOrder DECIMAL(10,2), @maxDist FLOAT,
            @openT VARCHAR(5), @closeT VARCHAR(5);

    SELECT @name = Name, @street = Street, @number = Number, @zip = Zip, @city = City,
           @lon = Lon, @lat = Lat, @webhook = WebhookUrl, @img = ImagePath, @rawKey = RawApiKey,
           @catName = CategoryName, @minOrder = MinOrderValue, @maxDist = MaxDistance,
           @openT = OpenTime, @closeT = CloseTime
    FROM @Restaurants WHERE Id = @rIdx;

    DECLARE @hashedKey VARCHAR(64) = UPPER(CONVERT(NVARCHAR(64), HASHBYTES('SHA2_256', @rawKey), 2));

    DECLARE @addrId INT;
    INSERT INTO Address (street, number, zip_code, city, country, longitude, latitude)
    VALUES (@street, @number, @zip, @city, 'AT', @lon, @lat);
    SET @addrId = SCOPE_IDENTITY();

    DECLARE @restId INT;
    INSERT INTO Restaurant (name, address_id, webhook_url, title_image_path, api_key)
    VALUES (@name, @addrId, @webhook, @img, @hashedKey);
    SET @restId = SCOPE_IDENTITY();

    DECLARE @catId INT;
    INSERT INTO MenuCategory (restaurant_id, name) VALUES (@restId, @catName);
    SET @catId = SCOPE_IDENTITY();

    INSERT INTO @RestaurantMap (RestaurantId, Name, CategoryId) VALUES (@restId, @name, @catId);

    -- Opening hours: open every day at the given window, Sun/Mon shortened slightly
    -- to keep some variety (some restaurants closed Mondays).
    IF @rIdx % 5 = 0
    BEGIN
        -- every 5th restaurant: closed on Mondays (day_of_week = 1)
        INSERT INTO OpeningHourSlot (restaurant_id, day_of_week, open_time, close_time)
        SELECT @restId, d, @openT, @closeT FROM (VALUES (0),(2),(3),(4),(5),(6)) AS Days(d);
    END
    ELSE
    BEGIN
        INSERT INTO OpeningHourSlot (restaurant_id, day_of_week, open_time, close_time)
        SELECT @restId, d, @openT, @closeT FROM (VALUES (0),(1),(2),(3),(4),(5),(6)) AS Days(d);
    END

    -- Delivery zone + tiered fee rules (free above a higher order value, flat fee below).
    DECLARE @zoneId INT;
    INSERT INTO DeliveryZone (restaurant_id, min_order_value, max_distance) VALUES (@restId, @minOrder, @maxDist);
    SET @zoneId = SCOPE_IDENTITY();

    DECLARE @baseFee DECIMAL(10,2) = CASE
        WHEN @city = 'Wien' THEN 2.90
        WHEN @city = 'Linz' THEN 2.20
        ELSE 1.90
    END;

    -- Tiered fees: cheap orders pay full fee, mid-range pay half, large orders ship free.
    INSERT INTO DeliveryFeeRule (delivery_zone_id, max_order_value, delivery_fee) VALUES
        (@zoneId, 19.99, @baseFee),
        (@zoneId, 39.99, ROUND(@baseFee / 2, 2)),
        (@zoneId, 999.00, 0.00);

    SET @rIdx = @rIdx + 1;
END

-- PRINT 'Restaurants, addresses, categories, opening hours and delivery zones inserted.';

-- ============================================================
-- 3. MENU ITEMS PER RESTAURANT (kept explicit per restaurant
--    name for readable, sensible dish names per cuisine)
-- ============================================================
DECLARE @rId INT, @cId INT;

-- Helper macro pattern: look up restaurant + its single category, insert items, link them.
-- Restaurant Nimmersatt
SELECT @rId = RestaurantId, @cId = CategoryId FROM @RestaurantMap WHERE Name = 'Restaurant Nimmersatt';
INSERT INTO MenuItem (restaurant_id, name, description, price) VALUES
    (@rId, 'Margherita', 'Classic Tomato & Cheese', 9.50),
    (@rId, 'Spinaci', 'Spinat & Feta', 10.50),
    (@rId, 'Lasagne al Forno', 'Hausgemacht mit Rind', 12.00),
    (@rId, 'Tiramisu', 'Hausgemacht', 5.50);
INSERT INTO MenuItemMenuCategory (menu_item_id, menu_category_id, restaurant_id)
SELECT id, @cId, @rId FROM MenuItem WHERE restaurant_id = @rId;

-- Pizzeria Da Mario
SELECT @rId = RestaurantId, @cId = CategoryId FROM @RestaurantMap WHERE Name = 'Pizzeria Da Mario';
INSERT INTO MenuItem (restaurant_id, name, description, price) VALUES
    (@rId, 'Pizza Diavola', 'Scharfe Salami & Chili', 10.90),
    (@rId, 'Pizza Quattro Formaggi', 'Vier Käsesorten', 11.50),
    (@rId, 'Calzone Prosciutto', 'Gefüllt mit Schinken & Käse', 11.00);
INSERT INTO MenuItemMenuCategory (menu_item_id, menu_category_id, restaurant_id)
SELECT id, @cId, @rId FROM MenuItem WHERE restaurant_id = @rId;

-- Gasthaus Goldener Hirsch
SELECT @rId = RestaurantId, @cId = CategoryId FROM @RestaurantMap WHERE Name = 'Gasthaus Goldener Hirsch';
INSERT INTO MenuItem (restaurant_id, name, description, price) VALUES
    (@rId, 'Schweinsbraten', 'Mit Knödel & Kraut', 14.90),
    (@rId, 'Wiener Schnitzel', 'Vom Schwein, mit Pommes', 13.50),
    (@rId, 'Käsespätzle', 'Mit Röstzwiebeln', 11.90);
INSERT INTO MenuItemMenuCategory (menu_item_id, menu_category_id, restaurant_id)
SELECT id, @cId, @rId FROM MenuItem WHERE restaurant_id = @rId;

-- China Restaurant Lotus
SELECT @rId = RestaurantId, @cId = CategoryId FROM @RestaurantMap WHERE Name = 'China Restaurant Lotus';
INSERT INTO MenuItem (restaurant_id, name, description, price) VALUES
    (@rId, 'Gebratene Nudeln Singapur', 'Mit Huhn & Curry', 9.90),
    (@rId, 'Süß-Sauer Schwein', 'Mit Reis', 10.50),
    (@rId, 'Frühlingsrollen (4 Stk.)', 'Vegetarisch', 5.90);
INSERT INTO MenuItemMenuCategory (menu_item_id, menu_category_id, restaurant_id)
SELECT id, @cId, @rId FROM MenuItem WHERE restaurant_id = @rId;

-- Pasta Fresca Linz
SELECT @rId = RestaurantId, @cId = CategoryId FROM @RestaurantMap WHERE Name = 'Pasta Fresca Linz';
INSERT INTO MenuItem (restaurant_id, name, description, price) VALUES
    (@rId, 'Spaghetti Carbonara', 'Mit Speck & Ei', 10.90),
    (@rId, 'Tagliatelle al Tartufo', 'Mit Trüffelcreme', 13.90),
    (@rId, 'Penne Arrabbiata', 'Scharfe Tomatensauce', 9.50);
INSERT INTO MenuItemMenuCategory (menu_item_id, menu_category_id, restaurant_id)
SELECT id, @cId, @rId FROM MenuItem WHERE restaurant_id = @rId;

-- Linzer Wirtshaus
SELECT @rId = RestaurantId, @cId = CategoryId FROM @RestaurantMap WHERE Name = 'Linzer Wirtshaus';
INSERT INTO MenuItem (restaurant_id, name, description, price) VALUES
    (@rId, 'Linzer Bierbraten', 'Mit Erdäpfelknödel', 15.50),
    (@rId, 'Backhendl', 'Mit Erdäpfelsalat', 12.90),
    (@rId, 'Gulaschsuppe', 'Mit Brot', 7.50);
INSERT INTO MenuItemMenuCategory (menu_item_id, menu_category_id, restaurant_id)
SELECT id, @cId, @rId FROM MenuItem WHERE restaurant_id = @rId;

-- Curry Palace Urfahr
SELECT @rId = RestaurantId, @cId = CategoryId FROM @RestaurantMap WHERE Name = 'Curry Palace Urfahr';
INSERT INTO MenuItem (restaurant_id, name, description, price) VALUES
    (@rId, 'Chicken Tikka Masala', 'Mit Basmatireis', 11.90),
    (@rId, 'Lamm Vindaloo', 'Sehr scharf, mit Naan', 13.50),
    (@rId, 'Gemüse Korma', 'Vegetarisch, mild', 10.50);
INSERT INTO MenuItemMenuCategory (menu_item_id, menu_category_id, restaurant_id)
SELECT id, @cId, @rId FROM MenuItem WHERE restaurant_id = @rId;

-- Burger Bude Wien
SELECT @rId = RestaurantId, @cId = CategoryId FROM @RestaurantMap WHERE Name = 'Burger Bude Wien';
INSERT INTO MenuItem (restaurant_id, name, description, price) VALUES
    (@rId, 'Classic Burger', 'Beef, Salad, Tomato', 8.90),
    (@rId, 'Cheese Burger', 'Beef & extra Cheddar', 9.50),
    (@rId, 'Bacon BBQ Burger', 'Mit Bacon & BBQ-Sauce', 10.90),
    (@rId, 'Pommes Frites', 'Mit Mayo', 3.50);
INSERT INTO MenuItemMenuCategory (menu_item_id, menu_category_id, restaurant_id)
SELECT id, @cId, @rId FROM MenuItem WHERE restaurant_id = @rId;

-- Sakura Sushi
SELECT @rId = RestaurantId, @cId = CategoryId FROM @RestaurantMap WHERE Name = 'Sakura Sushi';
INSERT INTO MenuItem (restaurant_id, name, description, price) VALUES
    (@rId, 'Sake Nigiri', 'Lachs (2 Stk.)', 4.80),
    (@rId, 'California Roll', 'Avocado & Krabbe (8 Stk.)', 8.90),
    (@rId, 'Sashimi Mix', 'Lachs, Thunfisch, Garnele', 14.90),
    (@rId, 'Miso Suppe', NULL, 3.20);
INSERT INTO MenuItemMenuCategory (menu_item_id, menu_category_id, restaurant_id)
SELECT id, @cId, @rId FROM MenuItem WHERE restaurant_id = @rId;

-- Pizzeria Napoli Wien
SELECT @rId = RestaurantId, @cId = CategoryId FROM @RestaurantMap WHERE Name = 'Pizzeria Napoli Wien';
INSERT INTO MenuItem (restaurant_id, name, description, price) VALUES
    (@rId, 'Pizza Napoli', 'Mit Sardellen & Kapern', 10.50),
    (@rId, 'Pizza Funghi', 'Mit frischen Champignons', 9.90),
    (@rId, 'Bruschetta', 'Tomate & Basilikum', 5.50);
INSERT INTO MenuItemMenuCategory (menu_item_id, menu_category_id, restaurant_id)
SELECT id, @cId, @rId FROM MenuItem WHERE restaurant_id = @rId;

-- Falafel King Ottakring
SELECT @rId = RestaurantId, @cId = CategoryId FROM @RestaurantMap WHERE Name = 'Falafel King Ottakring';
INSERT INTO MenuItem (restaurant_id, name, description, price) VALUES
    (@rId, 'Falafel Teller', 'Mit Hummus & Salat', 9.90),
    (@rId, 'Falafel Wrap', 'Mit Tahini-Sauce', 7.50),
    (@rId, 'Halloumi Spieße', 'Gegrillt, mit Gemüse', 8.90);
INSERT INTO MenuItemMenuCategory (menu_item_id, menu_category_id, restaurant_id)
SELECT id, @cId, @rId FROM MenuItem WHERE restaurant_id = @rId;

-- Steakhouse Favoriten
SELECT @rId = RestaurantId, @cId = CategoryId FROM @RestaurantMap WHERE Name = 'Steakhouse Favoriten';
INSERT INTO MenuItem (restaurant_id, name, description, price) VALUES
    (@rId, 'Ribeye Steak 300g', 'Mit Kräuterbutter', 28.90),
    (@rId, 'Sirloin Steak 250g', 'Mit Pfeffersauce', 24.50),
    (@rId, 'Gegrillter Mais', 'Beilage', 4.50);
INSERT INTO MenuItemMenuCategory (menu_item_id, menu_category_id, restaurant_id)
SELECT id, @cId, @rId FROM MenuItem WHERE restaurant_id = @rId;

-- Ramen Bar Donaustadt
SELECT @rId = RestaurantId, @cId = CategoryId FROM @RestaurantMap WHERE Name = 'Ramen Bar Donaustadt';
INSERT INTO MenuItem (restaurant_id, name, description, price) VALUES
    (@rId, 'Shoyu Ramen', 'Mit Schweinebauch & Ei', 12.90),
    (@rId, 'Miso Ramen', 'Mit Mais & Frühlingszwiebel', 12.50),
    (@rId, 'Gyoza (6 Stk.)', 'Gebraten', 6.50);
INSERT INTO MenuItemMenuCategory (menu_item_id, menu_category_id, restaurant_id)
SELECT id, @cId, @rId FROM MenuItem WHERE restaurant_id = @rId;

-- PRINT 'Menu items inserted for all restaurants.';

-- ============================================================
-- 4. Create 25 Unique Student Locations spread across the same
--    regions as the restaurants, so distance-based search has
--    realistic near/far candidates everywhere.
-- ============================================================
DECLARE @Locs TABLE (Id INT IDENTITY(1,1), City NVARCHAR(50), Street NVARCHAR(50), Zip VARCHAR(10), Lat FLOAT, Lon FLOAT);
INSERT INTO @Locs (City, Street, Zip, Lat, Lon) VALUES
('Hagenberg', 'Softwarepark', '4232', 48.3692, 14.5125),
('Hagenberg', 'Hauptstrasse', '4232', 48.3650, 14.5180),
('Hagenberg', 'Mahrersdorf', '4232', 48.3750, 14.5250),
('Linz', 'Landstrasse', '4020', 48.3000, 14.2900),
('Linz', 'Hauptplatz', '4020', 48.3060, 14.2860),
('Linz', 'Urfahr', '4040', 48.3150, 14.2850),
('Linz', 'Bindermichl', '4020', 48.2750, 14.2950),
('Linz', 'Ebelsberg', '4030', 48.2450, 14.3300),
('Pregarten', 'Stadtplatz', '4230', 48.3550, 14.5300),
('Freistadt', 'Boehmergasse', '4240', 48.5110, 14.5050),
('Wien', 'Mariahilfer Strasse', '1070', 48.1990, 16.3450),
('Wien', 'Stephansplatz', '1010', 48.2085, 16.3731),
('Wien', 'Praterstern', '1020', 48.2180, 16.3920),
('Wien', 'Meidling', '1120', 48.1750, 16.3320),
('Wien', 'Ottakring', '1160', 48.2120, 16.3100),
('Wien', 'Donaustadt', '1220', 48.2300, 16.4400),
('Wien', 'Favoriten', '1100', 48.1700, 16.3750),
('Wien', 'Hietzing', '1130', 48.1850, 16.2700),
('Wien', 'Floridsdorf', '1210', 48.2550, 16.4000),
('Wien', 'Simmering', '1110', 48.1750, 16.4150),
('Wien', 'Leopoldstadt', '1020', 48.2200, 16.3850),
('Wien', 'Neubau', '1070', 48.2010, 16.3490),
('Wien', 'Wieden', '1040', 48.1930, 16.3700),
('Wien', 'Landstrasse', '1030', 48.1970, 16.3900),
('Wien', 'Brigittenau', '1200', 48.2370, 16.3780);

DECLARE @i INT = 1;
WHILE @i <= (SELECT COUNT(*) FROM @Locs)
BEGIN
    INSERT INTO Address (street, number, zip_code, city, country, longitude, latitude, additional_info)
    SELECT Street, CAST(@i AS NVARCHAR(10)), Zip, City, 'AT', Lon, Lat, 'Room ' + CAST(400+@i AS NVARCHAR(10))
    FROM @Locs WHERE Id = @i;
    SET @i = @i + 1;
END

-- PRINT (SELECT CAST(COUNT(*) AS VARCHAR) FROM @Locs) + ' unique student addresses inserted.';

-- ============================================================
-- 5. Create 50 Geographically Plausible Orders, each restaurant
--    only receiving orders from addresses in its own city/region
--    so distances stay realistic, with 1-4 items per order and
--    delivery fees computed from each restaurant's actual
--    DeliveryFeeRule tiers (matching what the application would do).
-- ============================================================
DECLARE @orderIter INT = 1;
DECLARE @totalOrders INT = 50;

WHILE @orderIter <= @totalOrders
BEGIN
    DECLARE @sAddrId INT, @sCity NVARCHAR(50);

    -- Pick a random student address (skip restaurant addresses, which come first).
    SELECT TOP 1 @sAddrId = id, @sCity = city
    FROM Address
    WHERE id > (SELECT COUNT(*) FROM @Restaurants)
    ORDER BY NEWID();

    -- Restrict restaurant choice to ones in the same city, so we don't end up
    -- with a "Linz student orders from a Wien restaurant 100km away" nonsense order.
    DECLARE @candidateRestId INT;
    SELECT TOP 1 @candidateRestId = r.id
    FROM Restaurant r
    JOIN Address a ON a.id = r.address_id
    WHERE a.city = @sCity
    ORDER BY NEWID();

    -- Fallback: if no restaurant exists in that exact city (shouldn't happen given
    -- our data, but just in case), fall back to any restaurant.
    IF @candidateRestId IS NULL
        SELECT TOP 1 @candidateRestId = id FROM Restaurant ORDER BY NEWID();

    DECLARE @rId2 INT = @candidateRestId;

    DECLARE @oStatus NVARCHAR(30) = CASE (CAST(RAND()*6 AS INT) % 6)
        WHEN 0 THEN 'RECEIVED' WHEN 1 THEN 'SENT_TO_RESTAURANT' WHEN 2 THEN 'IN_PREPARATION'
        WHEN 3 THEN 'OUT_FOR_DELIVERY' WHEN 4 THEN 'DELIVERED' ELSE 'CANCELLED' END;

    DECLARE @oCode VARCHAR(16) = SUBSTRING(REPLACE(NEWID(), '-', ''), 1, 8);

    -- Insert the order first with a placeholder total/fee of 0; both get
    -- updated below once we know the actual subtotal.
    INSERT INTO CustomerOrder (restaurant_id, address_id, order_code, status, delivery_fee, total)
    VALUES (@rId2, @sAddrId, @oCode, @oStatus, 0, 0);

    DECLARE @oId INT = SCOPE_IDENTITY();
    DECLARE @subtotal DECIMAL(10,2) = 0;

    -- Add between 1 and 4 distinct order items from this restaurant's menu.
    DECLARE @itemCount INT = 1 + CAST(RAND() * 4 AS INT); -- 1..4
    IF @itemCount > 4 SET @itemCount = 4;

    DECLARE @ItemsToAdd TABLE (MenuItemId INT, Price DECIMAL(10,2));
    INSERT INTO @ItemsToAdd (MenuItemId, Price)
    SELECT TOP (@itemCount) id, price
    FROM MenuItem
    WHERE restaurant_id = @rId2
    ORDER BY NEWID();

    DECLARE @miId INT, @miPrice DECIMAL(10,2), @qty INT;
    DECLARE item_cursor CURSOR LOCAL FAST_FORWARD FOR SELECT MenuItemId, Price FROM @ItemsToAdd;
    OPEN item_cursor;
    FETCH NEXT FROM item_cursor INTO @miId, @miPrice;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @qty = 1 + CAST(RAND() * 2 AS INT); -- 1..2 per item
        IF @qty > 2 SET @qty = 2;

        INSERT INTO OrderItem (order_id, menu_item_id, quantity, unit_price)
        VALUES (@oId, @miId, @qty, @miPrice);

        SET @subtotal = @subtotal + (@miPrice * @qty);

        FETCH NEXT FROM item_cursor INTO @miId, @miPrice;
    END
    CLOSE item_cursor;
    DEALLOCATE item_cursor;
    DELETE FROM @ItemsToAdd;

    -- Compute the real delivery fee from this restaurant's DeliveryFeeRule tiers,
    -- mirroring how the application would price the order (cheapest matching tier
    -- by max_order_value, i.e. first tier whose ceiling the subtotal fits under).
    DECLARE @computedFee DECIMAL(10,2);
    SELECT TOP 1 @computedFee = dfr.delivery_fee
    FROM DeliveryZone dz
    JOIN DeliveryFeeRule dfr ON dfr.delivery_zone_id = dz.id
    WHERE dz.restaurant_id = @rId2
      AND dfr.max_order_value >= @subtotal
    ORDER BY dfr.max_order_value ASC;

    IF @computedFee IS NULL SET @computedFee = 0.00;

    UPDATE CustomerOrder
    SET delivery_fee = @computedFee,
        total = @subtotal + @computedFee
    WHERE id = @oId;

    SET @orderIter = @orderIter + 1;
END

-- PRINT CAST(@totalOrders AS VARCHAR) + ' geographically realistic orders inserted with computed delivery fees.';

-- PRINT 'Test data filled successfully.';

EndOfScript:
-- PRINT 'Script completed.';
GO