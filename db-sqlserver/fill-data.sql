-- ============================================================
-- fill-data.sql
-- Fills the BiteTestDb with sample data (restaurants, menu items, etc.).
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
-- 1. Restaurant Nimmersatt
-- ============================================================
DECLARE @addrNimmersatt    INT;
DECLARE @restNimmersatt    INT;
DECLARE @zone1Nimmersatt   INT;
DECLARE @zone2Nimmersatt   INT;
DECLARE @catNimPizza       INT;
DECLARE @catNimPasta       INT;
DECLARE @catNimGetraenke   INT;
DECLARE @catNimDessert     INT;

INSERT INTO Address (street, number, zip_code, city, country, longitude, latitude)
VALUES ('Softwarepark', '11', '4232', 'Hagenberg', 'Austria', 14.5144, 48.3684);
SET @addrNimmersatt = SCOPE_IDENTITY();

INSERT INTO Restaurant (name, address_id, webhook_url, title_image_path)
VALUES ('Restaurant Nimmersatt', @addrNimmersatt, 'https://api.nimmersatt.at/bite', 'img/nimmersatt.png');
SET @restNimmersatt = SCOPE_IDENTITY();

INSERT INTO MenuCategory (restaurant_id, name) VALUES (@restNimmersatt, 'Pizza');
SET @catNimPizza = SCOPE_IDENTITY();
INSERT INTO MenuCategory (restaurant_id, name) VALUES (@restNimmersatt, 'Pasta');
SET @catNimPasta = SCOPE_IDENTITY();
INSERT INTO MenuCategory (restaurant_id, name) VALUES (@restNimmersatt, 'Getraenke');
SET @catNimGetraenke = SCOPE_IDENTITY();
INSERT INTO MenuCategory (restaurant_id, name) VALUES (@restNimmersatt, 'Dessert');
SET @catNimDessert = SCOPE_IDENTITY();

DECLARE @itemsNimmersatt TABLE (
    name NVARCHAR(100) NOT NULL,
    description NVARCHAR(255),
    price DECIMAL(10, 2) NOT NULL,
    menu_category_id INT NOT NULL
);

INSERT INTO @itemsNimmersatt (name, description, price, menu_category_id) VALUES
    ('Margherita',               'Tomaten, Kaese',                              9.50, @catNimPizza),
    ('Al Tonno',                 'Tomaten, Kaese, Thunfisch, Zwiebel, Oliven', 11.00, @catNimPizza),
    ('Spinaci',                  'Tomaten, Kaese, Spinat, Feta',               10.00, @catNimPizza),
    ('Diavola',                  'Tomaten, Kaese, Salami, Chili',              10.50, @catNimPizza),
    ('Lasagne al Forno',         'Mit Rinderfaschiertem',                      12.00, @catNimPasta),
    ('Spaghetti Frutti di Mare', 'Meeresfruechte, Weissweinsauce',             14.00, @catNimPasta),
    ('Penne Arrabbiata',         'Tomatensauce, Chili, Knoblauch',              9.00, @catNimPasta),
    ('Cola',                     '0,5l',                                        2.50, @catNimGetraenke),
    ('Wasser',                   '0,5l still',                                  1.80, @catNimGetraenke),
    ('Bier',                     '0,5l Ottakringer',                            3.50, @catNimGetraenke),
    ('Tiramisu',                 'Hausgemacht',                                 5.50, @catNimDessert),
    ('Panna Cotta',              'Mit Beerensauce',                             4.90, @catNimDessert);

INSERT INTO MenuItem (restaurant_id, name, description, price)
SELECT @restNimmersatt, name, description, price
FROM @itemsNimmersatt;

INSERT INTO MenuItemMenuCategory (menu_item_id, menu_category_id, restaurant_id)
SELECT mi.id, i.menu_category_id, @restNimmersatt
FROM @itemsNimmersatt i
JOIN MenuItem mi ON mi.restaurant_id = @restNimmersatt AND mi.name = i.name;

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
-- 2. Restaurant Burger Bude Wien
-- ============================================================
DECLARE @addrBurger    INT;
DECLARE @restBurger    INT;
DECLARE @zone1Burger   INT;
DECLARE @zone2Burger   INT;
DECLARE @catBurBurger  INT;
DECLARE @catBurSalat   INT;
DECLARE @catBurGetrank INT;

INSERT INTO Address (street, number, zip_code, city, country, longitude, latitude)
VALUES ('Hauptstrasse', '42', '1010', 'Wien', 'Austria', 16.3738, 48.2082);
SET @addrBurger = SCOPE_IDENTITY();

INSERT INTO Restaurant (name, address_id, webhook_url, title_image_path)
VALUES ('Burger Bude Wien', @addrBurger, 'https://hooks.burgerbude.at/bite', 'img/burgerbude.png');
SET @restBurger = SCOPE_IDENTITY();

INSERT INTO MenuCategory (restaurant_id, name) VALUES (@restBurger, 'Burger');
SET @catBurBurger = SCOPE_IDENTITY();
INSERT INTO MenuCategory (restaurant_id, name) VALUES (@restBurger, 'Salat');
SET @catBurSalat = SCOPE_IDENTITY();
INSERT INTO MenuCategory (restaurant_id, name) VALUES (@restBurger, 'Getraenke');
SET @catBurGetrank = SCOPE_IDENTITY();

DECLARE @itemsBurger TABLE (
    name NVARCHAR(100) NOT NULL,
    description NVARCHAR(255),
    price DECIMAL(10, 2) NOT NULL,
    menu_category_id INT NOT NULL
);

INSERT INTO @itemsBurger (name, description, price, menu_category_id) VALUES
    ('Classic Burger', 'Rindfleisch, Salat, Tomate, Gurke',       8.90, @catBurBurger),
    ('Cheese Burger',  'Rindfleisch, Cheddar, Zwiebeln',          9.50, @catBurBurger),
    ('BBQ Burger',     'Rindfleisch, BBQ-Sauce, Bacon, Cheddar', 11.90, @catBurBurger),
    ('Veggie Burger',  'Gemuesepatty, Avocado, Tomate',           9.90, @catBurBurger),
    ('Chicken Burger', 'Knuspriges Huehnchen, Coleslaw',         10.50, @catBurBurger),
    ('Caesar Salad',   'Roemerherz, Parmesan, Croutons',          7.90, @catBurSalat),
    ('Greek Salad',    'Tomate, Gurke, Feta, Oliven',             7.50, @catBurSalat),
    ('Cola',           '0,4l',                                    2.80, @catBurGetrank),
    ('Limo',           '0,4l, div. Sorten',                       2.80, @catBurGetrank),
    ('Milchshake',     'Schoko, Vanille oder Erdbeere',           4.50, @catBurGetrank);

INSERT INTO MenuItem (restaurant_id, name, description, price)
SELECT @restBurger, name, description, price
FROM @itemsBurger;

INSERT INTO MenuItemMenuCategory (menu_item_id, menu_category_id, restaurant_id)
SELECT mi.id, i.menu_category_id, @restBurger
FROM @itemsBurger i
JOIN MenuItem mi ON mi.restaurant_id = @restBurger AND mi.name = i.name;

INSERT INTO OpeningHourSlot (restaurant_id, day_of_week, open_time, close_time) VALUES
    (@restBurger, 0, '11:00', '23:00'),
    (@restBurger, 1, '11:00', '23:00'),
    (@restBurger, 2, '11:00', '23:00'),
    (@restBurger, 3, '11:00', '23:00'),
    (@restBurger, 4, '11:00', '23:00'),
    (@restBurger, 5, '11:00', '23:00'),
    (@restBurger, 6, '11:00', '23:00');

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
-- 3. Restaurant Sakura Sushi
-- ============================================================
DECLARE @addrSakura     INT;
DECLARE @restSakura     INT;
DECLARE @zone1Sakura    INT;
DECLARE @zone2Sakura    INT;
DECLARE @catSakSushi    INT;
DECLARE @catSakSalat    INT;
DECLARE @catSakGetrank  INT;
DECLARE @catSakDessert  INT;

INSERT INTO Address (street, number, zip_code, city, country, longitude, latitude)
VALUES ('Mariahilfer Strasse', '88', '1060', 'Wien', 'Austria', 16.3540, 48.1970);
SET @addrSakura = SCOPE_IDENTITY();

INSERT INTO Restaurant (name,  address_id, webhook_url, title_image_path)
VALUES ('Sakura Sushi', @addrSakura, 'https://webhooks.sakura-sushi.at/orders', 'img/sakura.png');
SET @restSakura = SCOPE_IDENTITY();

INSERT INTO MenuCategory (restaurant_id, name) VALUES (@restSakura, 'Sushi');
SET @catSakSushi = SCOPE_IDENTITY();
INSERT INTO MenuCategory (restaurant_id, name) VALUES (@restSakura, 'Salat');
SET @catSakSalat = SCOPE_IDENTITY();
INSERT INTO MenuCategory (restaurant_id, name) VALUES (@restSakura, 'Getraenke');
SET @catSakGetrank = SCOPE_IDENTITY();
INSERT INTO MenuCategory (restaurant_id, name) VALUES (@restSakura, 'Dessert');
SET @catSakDessert = SCOPE_IDENTITY();

DECLARE @itemsSakura TABLE (
    name NVARCHAR(100) NOT NULL,
    description NVARCHAR(255),
    price DECIMAL(10, 2) NOT NULL,
    menu_category_id INT NOT NULL
);

INSERT INTO @itemsSakura (name, description, price, menu_category_id) VALUES
    ('Sake Nigiri (2 St.)',     'Lachs',                         4.80, @catSakSushi),
    ('Maguro Nigiri (2 St.)',   'Thunfisch',                     5.20, @catSakSushi),
    ('California Roll (8 St.)', 'Krabben, Avocado, Gurke',       8.90, @catSakSushi),
    ('Spicy Tuna Roll (8 St.)', 'Thunfisch, Sriracha',           9.50, @catSakSushi),
    ('Veggie Roll (8 St.)',     'Gurke, Avocado, Karotte',       7.90, @catSakSushi),
    ('Sashimi Mix (10 St.)',    'Lachs, Thunfisch, Garnele',    16.90, @catSakSushi),
    ('Dragon Roll (8 St.)',     'Garnele, Avocado, Teriyaki',   12.50, @catSakSushi),
    ('Edamame',                 'Gesalzen',                      3.50, @catSakSalat),
    ('Wakame Salat',            'Meeresalgen, Sesam',            4.90, @catSakSalat),
    ('Gruener Tee',             'Kanne 0,5l',                    3.20, @catSakGetrank),
    ('Japanisches Bier',        'Asahi 0,33l',                   4.00, @catSakGetrank),
    ('Sake',                    '0,1l warm',                     5.50, @catSakGetrank),
    ('Mochi Eis',               'Gruener Tee oder Mango',        4.50, @catSakDessert),
    ('Dorayaki',                'Japanischer Pancake mit Anko',  3.90, @catSakDessert);

INSERT INTO MenuItem (restaurant_id, name, description, price)
SELECT @restSakura, name, description, price
FROM @itemsSakura;

INSERT INTO MenuItemMenuCategory (menu_item_id, menu_category_id, restaurant_id)
SELECT mi.id, i.menu_category_id, @restSakura
FROM @itemsSakura i
JOIN MenuItem mi ON mi.restaurant_id = @restSakura AND mi.name = i.name;

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

INSERT INTO DeliveryZone (restaurant_id, min_order_value, max_distance) VALUES (@restSakura, 25.00, 8.0);
SET @zone1Sakura = SCOPE_IDENTITY();
INSERT INTO DeliveryFeeRule (delivery_zone_id, max_order_value, delivery_fee) VALUES
    (@zone1Sakura, 40.00,   3.90),
    (@zone1Sakura, 9999.00, 0.00);

INSERT INTO DeliveryZone (restaurant_id, min_order_value, max_distance) VALUES (@restSakura, 30.00, 15.0);
SET @zone2Sakura = SCOPE_IDENTITY();
INSERT INTO DeliveryFeeRule (delivery_zone_id, max_order_value, delivery_fee) VALUES (@zone2Sakura, 9999.00, 5.90);

PRINT 'Restaurant Sakura Sushi inserted.';

PRINT 'Test data filled successfully.';

EndOfScript:
PRINT 'Script completed.';
GO
