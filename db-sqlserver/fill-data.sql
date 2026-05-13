-- ============================================================
-- fill-data.sql
-- Fills the BiteTestDb with sample data (restaurants, menu items, etc.)
-- Called after init-db.sql to populate the database.
-- Called via sqlcmd:
--   sqlcmd -S db -U sa -P <password> -C -i fill-data.sql
-- ============================================================

USE BiteTestDb;
GO

-- Check if the database is already filled (by checking if there are any restaurants)
IF EXISTS (SELECT 1 FROM Restaurant)
BEGIN
    PRINT 'Database already contains data. Skipping fill-data.sql.';
    GOTO EndOfScript;
END

SET NOCOUNT ON;

-- ============================================================
-- 1. MenuCategories
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM MenuCategory WHERE name = 'Pizza')     INSERT INTO MenuCategory (name) VALUES ('Pizza');
IF NOT EXISTS (SELECT 1 FROM MenuCategory WHERE name = 'Pasta')     INSERT INTO MenuCategory (name) VALUES ('Pasta');
IF NOT EXISTS (SELECT 1 FROM MenuCategory WHERE name = 'Burger')    INSERT INTO MenuCategory (name) VALUES ('Burger');
IF NOT EXISTS (SELECT 1 FROM MenuCategory WHERE name = 'Salat')     INSERT INTO MenuCategory (name) VALUES ('Salat');
IF NOT EXISTS (SELECT 1 FROM MenuCategory WHERE name = 'Getränke')  INSERT INTO MenuCategory (name) VALUES ('Getränke');
IF NOT EXISTS (SELECT 1 FROM MenuCategory WHERE name = 'Sushi')     INSERT INTO MenuCategory (name) VALUES ('Sushi');
IF NOT EXISTS (SELECT 1 FROM MenuCategory WHERE name = 'Dessert')   INSERT INTO MenuCategory (name) VALUES ('Dessert');

DECLARE @catPizza    INT = (SELECT id FROM MenuCategory WHERE name = 'Pizza');
DECLARE @catPasta    INT = (SELECT id FROM MenuCategory WHERE name = 'Pasta');
DECLARE @catBurger   INT = (SELECT id FROM MenuCategory WHERE name = 'Burger');
DECLARE @catSalat    INT = (SELECT id FROM MenuCategory WHERE name = 'Salat');
DECLARE @catGetraenk INT = (SELECT id FROM MenuCategory WHERE name = 'Getränke');
DECLARE @catSushi    INT = (SELECT id FROM MenuCategory WHERE name = 'Sushi');
DECLARE @catDessert  INT = (SELECT id FROM MenuCategory WHERE name = 'Dessert');

PRINT 'Menu categories inserted.';

-- ============================================================
-- 2. Restaurant Nimmersatt
-- ============================================================
DECLARE @menuNimmersatt    INT;
DECLARE @addrNimmersatt    INT;
DECLARE @restNimmersatt    INT;
DECLARE @zone1Nimmersatt   INT;
DECLARE @zone2Nimmersatt   INT;

INSERT INTO Menu DEFAULT VALUES;
SET @menuNimmersatt = SCOPE_IDENTITY();

INSERT INTO Address (street, number, zip_code, city, country, longitude, latitude)
VALUES ('Softwarepark', '11', '4232', 'Hagenberg', 'Austria', 14.5144, 48.3684);
SET @addrNimmersatt = SCOPE_IDENTITY();

INSERT INTO Restaurant (name, menu_id, address_id, webhook_url, title_image_path)
VALUES ('Restaurant Nimmersatt', @menuNimmersatt, @addrNimmersatt, 'https://api.nimmersatt.at/bite', 'img/nimmersatt.png');
SET @restNimmersatt = SCOPE_IDENTITY();

-- Menu
INSERT INTO MenuItem (menu_id, category_id, name, description, price) VALUES
    (@menuNimmersatt, @catPizza,    'Margherita',               'Tomaten, Käse',                          9.50),
    (@menuNimmersatt, @catPizza,    'Al Tonno',                 'Tomaten, Käse, Thunfisch, Zwiebel, Oliven', 11.00),
    (@menuNimmersatt, @catPizza,    'Spinaci',                  'Tomaten, Käse, Spinat, Feta',            10.00),
    (@menuNimmersatt, @catPizza,    'Diavola',                  'Tomaten, Käse, Salami, Chili',           10.50),
    (@menuNimmersatt, @catPasta,    'Lasagne al Forno',         'Mit Rinderfaschiertem',                  12.00),
    (@menuNimmersatt, @catPasta,    'Spaghetti Frutti di Mare', 'Meeresfrüchte, Weißweinsauce',           14.00),
    (@menuNimmersatt, @catPasta,    'Penne Arrabbiata',         'Tomatensauce, Chili, Knoblauch',          9.00),
    (@menuNimmersatt, @catGetraenk, 'Cola',                     '0,5l',                                    2.50),
    (@menuNimmersatt, @catGetraenk, 'Wasser',                   '0,5l still',                              1.80),
    (@menuNimmersatt, @catGetraenk, 'Bier',                     '0,5l Ottakringer',                        3.50),
    (@menuNimmersatt, @catDessert,  'Tiramisu',                 'Hausgemacht',                             5.50),
    (@menuNimmersatt, @catDessert,  'Panna Cotta',              'Mit Beerensauce',                         4.90);

-- Opening hours: Tuesday-Friday 11-15 and 17-22 (day 2-5), Saturday 11-14 (day 6), Sunday 11-14 (day 0) 
INSERT INTO OpeningHourSlot (restaurant_id, day_of_week, open_time, close_time) VALUES
    (@restNimmersatt, 2, '11:00', '15:00'),
    (@restNimmersatt, 2, '17:00', '22:00'),
    (@restNimmersatt, 3, '11:00', '15:00'),
    (@restNimmersatt, 3, '17:00', '22:00'),
    (@restNimmersatt, 4, '11:00', '15:00'),
    (@restNimmersatt, 4, '17:00', '22:00'),
    (@restNimmersatt, 5, '11:00', '15:00'),
    (@restNimmersatt, 5, '17:00', '22:00'),
    (@restNimmersatt, 6, '11:00', '14:00'),
    (@restNimmersatt, 0, '11:00', '14:00');

-- Delivery zones and fees
INSERT INTO DeliveryZone (restaurant_id, min_order_value, max_distance) VALUES (@restNimmersatt, 20.00, 10.0);
SET @zone1Nimmersatt = SCOPE_IDENTITY();
INSERT INTO DeliveryFeeRule (delivery_zone_id, max_order_value, delivery_fee) VALUES (@zone1Nimmersatt, 9999.00, 0.00);

INSERT INTO DeliveryZone (restaurant_id, min_order_value, max_distance) VALUES (@restNimmersatt, 20.00, 20.0);
SET @zone2Nimmersatt = SCOPE_IDENTITY();
INSERT INTO DeliveryFeeRule (delivery_zone_id, max_order_value, delivery_fee) VALUES
    (@zone2Nimmersatt, 30.00,   5.00),
    (@zone2Nimmersatt, 9999.00, 0.00);

PRINT 'Restaurant Nimmersatt inserted.';

-- ============================================================
-- 3. Restaurant Burger Bude Wien
-- ============================================================
DECLARE @menuBurger  INT;
DECLARE @addrBurger  INT;
DECLARE @restBurger  INT;
DECLARE @zone1Burger INT;
DECLARE @zone2Burger INT;

INSERT INTO Menu DEFAULT VALUES;
SET @menuBurger = SCOPE_IDENTITY();

INSERT INTO Address (street, number, zip_code, city, country, longitude, latitude)
VALUES ('Hauptstraße', '42', '1010', 'Wien', 'Austria', 16.3738, 48.2082);
SET @addrBurger = SCOPE_IDENTITY();

INSERT INTO Restaurant (name, menu_id, address_id, webhook_url, title_image_path)
VALUES ('Burger Bude Wien', @menuBurger, @addrBurger, 'https://hooks.burgerbude.at/bite', 'img/burgerbude.png');
SET @restBurger = SCOPE_IDENTITY();

-- Menu
INSERT INTO MenuItem (menu_id, category_id, name, description, price) VALUES
    (@menuBurger, @catBurger,   'Classic Burger',  'Rindfleisch, Salat, Tomate, Gurke',  8.90),
    (@menuBurger, @catBurger,   'Cheese Burger',   'Rindfleisch, Cheddar, Zwiebeln',     9.50),
    (@menuBurger, @catBurger,   'BBQ Burger',      'Rindfleisch, BBQ-Sauce, Bacon, Cheddar', 11.90),
    (@menuBurger, @catBurger,   'Veggie Burger',   'Gemüsepatty, Avocado, Tomate',       9.90),
    (@menuBurger, @catBurger,   'Chicken Burger',  'Knuspriges Hühnchen, Coleslaw',     10.50),
    (@menuBurger, @catSalat,    'Caesar Salad',    'Römerherz, Parmesan, Croutons',      7.90),
    (@menuBurger, @catSalat,    'Greek Salad',     'Tomate, Gurke, Feta, Oliven',        7.50),
    (@menuBurger, @catGetraenk, 'Cola',            '0,4l',                               2.80),
    (@menuBurger, @catGetraenk, 'Limo',            '0,4l, div. Sorten',                  2.80),
    (@menuBurger, @catGetraenk, 'Milchshake',      'Schoko, Vanille oder Erdbeere',      4.50);

-- Opening hours: Monday-Sunday 11-23 (day 0-6)
INSERT INTO OpeningHourSlot (restaurant_id, day_of_week, open_time, close_time) VALUES
    (@restBurger, 0, '11:00', '23:00'),
    (@restBurger, 1, '11:00', '23:00'),
    (@restBurger, 2, '11:00', '23:00'),
    (@restBurger, 3, '11:00', '23:00'),
    (@restBurger, 4, '11:00', '23:00'),
    (@restBurger, 5, '11:00', '23:00'),
    (@restBurger, 6, '11:00', '23:00');

-- Delivery zones and fees
INSERT INTO DeliveryZone (restaurant_id, min_order_value, max_distance) VALUES (@restBurger, 15.00, 5.0);
SET @zone1Burger = SCOPE_IDENTITY();
INSERT INTO DeliveryFeeRule (delivery_zone_id, max_order_value, delivery_fee) VALUES
    (@zone1Burger, 25.00,   2.99),
    (@zone1Burger, 9999.00, 0.00);

INSERT INTO DeliveryZone (restaurant_id, min_order_value, max_distance) VALUES (@restBurger, 20.00, 15.0);
SET @zone2Burger = SCOPE_IDENTITY();
INSERT INTO DeliveryFeeRule (delivery_zone_id, max_order_value, delivery_fee) VALUES (@zone2Burger, 9999.00, 4.99);

PRINT 'Restaurant Burger Bude Wien inserted.';

-- ============================================================
-- 4. Restaurant Sakura Sushi
-- ============================================================
DECLARE @menuSakura  INT;
DECLARE @addrSakura  INT;
DECLARE @restSakura  INT;
DECLARE @zone1Sakura INT;
DECLARE @zone2Sakura INT;

INSERT INTO Menu DEFAULT VALUES;
SET @menuSakura = SCOPE_IDENTITY();

INSERT INTO Address (street, number, zip_code, city, country, longitude, latitude)
VALUES ('Mariahilfer Straße', '88', '1060', 'Wien', 'Austria', 16.3540, 48.1970);
SET @addrSakura = SCOPE_IDENTITY();

INSERT INTO Restaurant (name, menu_id, address_id, webhook_url, title_image_path)
VALUES ('Sakura Sushi', @menuSakura, @addrSakura, 'https://webhooks.sakura-sushi.at/orders', 'img/sakura.png');
SET @restSakura = SCOPE_IDENTITY();

-- Menu
INSERT INTO MenuItem (menu_id, category_id, name, description, price) VALUES
    (@menuSakura, @catSushi,    'Sake Nigiri (2 St.)',     'Lachs',                             4.80),
    (@menuSakura, @catSushi,    'Maguro Nigiri (2 St.)',   'Thunfisch',                         5.20),
    (@menuSakura, @catSushi,    'California Roll (8 St.)', 'Krabben, Avocado, Gurke',           8.90),
    (@menuSakura, @catSushi,    'Spicy Tuna Roll (8 St.)', 'Thunfisch, Sriracha',               9.50),
    (@menuSakura, @catSushi,    'Veggie Roll (8 St.)',     'Gurke, Avocado, Karotte',           7.90),
    (@menuSakura, @catSushi,    'Sashimi Mix (10 St.)',    'Lachs, Thunfisch, Garnele',        16.90),
    (@menuSakura, @catSushi,    'Dragon Roll (8 St.)',     'Garnele, Avocado, Teriyaki',       12.50),
    (@menuSakura, @catSalat,    'Edamame',                 'Gesalzen',                          3.50),
    (@menuSakura, @catSalat,    'Wakame Salat',            'Meeresalgen, Sesam',                4.90),
    (@menuSakura, @catGetraenk, 'Grüner Tee',              'Kanne 0,5l',                        3.20),
    (@menuSakura, @catGetraenk, 'Japanisches Bier',        'Asahi 0,33l',                       4.00),
    (@menuSakura, @catGetraenk, 'Sake',                    '0,1l warm',                         5.50),
    (@menuSakura, @catDessert,  'Mochi Eis',               'Grüner Tee oder Mango',             4.50),
    (@menuSakura, @catDessert,  'Dorayaki',                'Japanischer Pancake mit Anko',      3.90);

-- Opening hours: Tuesday-Saturday 12-15 and 17:30-22:30 (day 2-6), Sunday 12-22 (day 0)
INSERT INTO OpeningHourSlot (restaurant_id, day_of_week, open_time, close_time) VALUES
    (@restSakura, 2, '12:00', '15:00'),
    (@restSakura, 2, '17:30', '22:30'),
    (@restSakura, 3, '12:00', '15:00'),
    (@restSakura, 3, '17:30', '22:30'),
    (@restSakura, 4, '12:00', '15:00'),
    (@restSakura, 4, '17:30', '22:30'),
    (@restSakura, 5, '12:00', '15:00'),
    (@restSakura, 5, '17:30', '22:30'),
    (@restSakura, 6, '12:00', '15:00'),
    (@restSakura, 6, '17:30', '22:30'),
    (@restSakura, 0, '12:00', '22:00');

-- Delivery zones and fees
INSERT INTO DeliveryZone (restaurant_id, min_order_value, max_distance) VALUES (@restSakura, 25.00, 8.0);
SET @zone1Sakura = SCOPE_IDENTITY();
INSERT INTO DeliveryFeeRule (delivery_zone_id, max_order_value, delivery_fee) VALUES
    (@zone1Sakura, 40.00,   3.90),
    (@zone1Sakura, 9999.00, 0.00);

INSERT INTO DeliveryZone (restaurant_id, min_order_value, max_distance) VALUES (@restSakura, 30.00, 15.0);
SET @zone2Sakura = SCOPE_IDENTITY();
INSERT INTO DeliveryFeeRule (delivery_zone_id, max_order_value, delivery_fee) VALUES (@zone2Sakura, 9999.00, 5.90);

PRINT 'Restaurant Sakura Sushi inserted.';

-- ============================================================
-- 5. Generic Restaurants 4–20 (Bite Palace)
-- ============================================================
DECLARE @i       INT = 4;
DECLARE @menuG   INT;
DECLARE @addrG   INT;
DECLARE @restG   INT;
DECLARE @zoneG   INT;
DECLARE @day     INT;

WHILE @i <= 20
BEGIN
    INSERT INTO Menu DEFAULT VALUES;
    SET @menuG = SCOPE_IDENTITY();

    INSERT INTO Address (street, number, zip_code, city, country, longitude, latitude)
    VALUES (
        CONCAT('Teststraße ', @i),
        CONCAT(@i, 'a'),
        '12345',
        'Teststadt',
        'Austria',
        14.5150 + (@i * 0.01),
        48.3680 + (@i * 0.01)
    );
    SET @addrG = SCOPE_IDENTITY();

    INSERT INTO Restaurant (name, menu_id, address_id, webhook_url, title_image_path)
    VALUES (
        CONCAT('Bite Palace ', @i),
        @menuG,
        @addrG,
        CONCAT('https://hooks.bite.com/rest', @i),
        CONCAT('img/rest_', @i, '.png')
    );
    SET @restG = SCOPE_IDENTITY();

    -- Simple menu with 1 burger, 1 drink
    INSERT INTO MenuItem (menu_id, category_id, name, description, price)
    VALUES (@menuG, @catBurger, CONCAT('Burger Classic ', @i), 'Rindfleisch, Salat, Tomate', 8.90 + @i * 0.10);
    INSERT INTO MenuItem (menu_id, category_id, name, description, price)
    VALUES (@menuG, @catGetraenk, 'Cola', '0,5l', 2.50);

    -- Opening hours: Monday-Friday 11-22 (day 1-5)
    SET @day = 1;
    WHILE @day <= 5
    BEGIN
        INSERT INTO OpeningHourSlot (restaurant_id, day_of_week, open_time, close_time)
        VALUES (@restG, @day, '11:00', '22:00');
        SET @day = @day + 1;
    END

    -- Delivery zone: 15€ min order, 10km max distance, 3.50€ fee under 30€, free above
    INSERT INTO DeliveryZone (restaurant_id, min_order_value, max_distance) VALUES (@restG, 15.00, 10.0);
    SET @zoneG = SCOPE_IDENTITY();
    INSERT INTO DeliveryFeeRule (delivery_zone_id, max_order_value, delivery_fee) VALUES
        (@zoneG, 30.00,   3.50),
        (@zoneG, 9999.00, 0.00); -- Free delivery above 30€

    SET @i = @i + 1;
END
PRINT 'Generic Restaurants (Bite Palace 4-20) inserted.';

PRINT 'Test data filled successfully.';

EndOfScript:
PRINT 'Script completed.';
GO
