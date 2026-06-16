-- ============================================================
-- fill-data.sql
-- Fills the BiteTestDb with sample data (restaurants, menu items, etc.).
-- ============================================================

USE BiteTestDb;
GO

IF EXISTS (SELECT 1 FROM Restaurant)
BEGIN
    PRINT 'Database already contains data. Skipping fill-data.sql.';
    GOTO EndOfScript;
END

SET NOCOUNT ON;

-- ============================================================
-- 1. API KEY HASHING STRATEGY
-- ============================================================
DECLARE @rawNim VARCHAR(100) = 'nimmersatt-api-key-2026';
DECLARE @rawBur VARCHAR(100) = 'burger-bude-api-key-2026';
DECLARE @rawSak VARCHAR(100) = 'sakura-sushi-api-key-2026';

DECLARE @hashNim VARCHAR(64) = UPPER(CONVERT(NVARCHAR(64), HASHBYTES('SHA2_256', @rawNim), 2));
DECLARE @hashBur VARCHAR(64) = UPPER(CONVERT(NVARCHAR(64), HASHBYTES('SHA2_256', @rawBur), 2));
DECLARE @hashSak VARCHAR(64) = UPPER(CONVERT(NVARCHAR(64), HASHBYTES('SHA2_256', @rawSak), 2));

-- ============================================================
-- 2. Restaurant Nimmersatt (Hagenberg)
-- ============================================================
DECLARE @addrNim INT;
DECLARE @restNim INT;
DECLARE @z1Nim   INT;
DECLARE @cNPiz   INT;
DECLARE @cNPas   INT;

INSERT INTO Address (street, number, zip_code, city, country, longitude, latitude)
VALUES ('Softwarepark', '11', '4232', 'Hagenberg', 'AT', 14.5144, 48.3684);
SET @addrNim = SCOPE_IDENTITY();

INSERT INTO Restaurant (name, address_id, webhook_url, title_image_path, api_key)
VALUES ('Restaurant Nimmersatt', @addrNim, 'https://api.nimmersatt.at/bite', 'img/nimmersatt.png', @hashNim);
SET @restNim = SCOPE_IDENTITY();

INSERT INTO MenuCategory (restaurant_id, name) VALUES (@restNim, 'Pizza'), (@restNim, 'Pasta');
SELECT @cNPiz = id FROM MenuCategory WHERE restaurant_id = @restNim AND name = 'Pizza';
SELECT @cNPas = id FROM MenuCategory WHERE restaurant_id = @restNim AND name = 'Pasta';

INSERT INTO MenuItem (restaurant_id, name, description, price) VALUES
    (@restNim, 'Margherita', 'Classic Tomato & Cheese', 9.50),
    (@restNim, 'Spinaci', 'Spinat & Feta', 10.50),
    (@restNim, 'Lasagne al Forno', 'Hausgemacht mit Rind', 12.00);

INSERT INTO MenuItemMenuCategory (menu_item_id, menu_category_id, restaurant_id)
SELECT id, @cNPiz, @restNim FROM MenuItem WHERE restaurant_id = @restNim AND name IN ('Margherita', 'Spinaci');
INSERT INTO MenuItemMenuCategory (menu_item_id, menu_category_id, restaurant_id)
SELECT id, @cNPas, @restNim FROM MenuItem WHERE restaurant_id = @restNim AND name = 'Lasagne al Forno';

INSERT INTO OpeningHourSlot (restaurant_id, day_of_week, open_time, close_time) VALUES
    (@restNim, 1, '11:00', '22:00'), (@restNim, 2, '11:00', '22:00'), (@restNim, 3, '11:00', '22:00'),
    (@restNim, 4, '11:00', '22:00'), (@restNim, 5, '11:00', '22:00'), (@restNim, 6, '11:00', '14:00'), (@restNim, 0, '11:00', '14:00');

INSERT INTO DeliveryZone (restaurant_id, min_order_value, max_distance) VALUES (@restNim, 15.00, 15.0);
SET @z1Nim = SCOPE_IDENTITY();
INSERT INTO DeliveryFeeRule (delivery_zone_id, max_order_value, delivery_fee) VALUES (@z1Nim, 999.00, 0.00);

-- ============================================================
-- 3. Restaurant Burger Bude Wien
-- ============================================================
DECLARE @addrBur INT;
DECLARE @restBur INT;
DECLARE @z1Bur   INT;
DECLARE @cBBur   INT;

INSERT INTO Address (street, number, zip_code, city, country, longitude, latitude)
VALUES ('Hauptstrasse', '42', '1010', 'Wien', 'AT', 16.3738, 48.2082);
SET @addrBur = SCOPE_IDENTITY();

INSERT INTO Restaurant (name, address_id, webhook_url, title_image_path, api_key)
VALUES ('Burger Bude Wien', @addrBur, 'https://hooks.burgerbude.at/bite', 'img/burgerbude.png', @hashBur);
SET @restBur = SCOPE_IDENTITY();

INSERT INTO MenuCategory (restaurant_id, name) VALUES (@restBur, 'Burger');
SET @cBBur = SCOPE_IDENTITY();

INSERT INTO MenuItem (restaurant_id, name, description, price) VALUES
    (@restBur, 'Classic Burger', 'Beef, Salad, Tomato', 8.90),
    (@restBur, 'Cheese Burger', 'Beef & extra Cheddar', 9.50);

INSERT INTO MenuItemMenuCategory (menu_item_id, menu_category_id, restaurant_id)
SELECT id, @cBBur, @restBur FROM MenuItem WHERE restaurant_id = @restBur;

INSERT INTO OpeningHourSlot (restaurant_id, day_of_week, open_time, close_time) VALUES
    (@restBur, 0, '11:00', '23:00'), (@restBur, 1, '11:00', '23:00'), (@restBur, 2, '11:00', '23:00'),
    (@restBur, 3, '11:00', '23:00'), (@restBur, 4, '11:00', '23:00'), (@restBur, 5, '11:00', '23:00'), (@restBur, 6, '11:00', '23:00');

INSERT INTO DeliveryZone (restaurant_id, min_order_value, max_distance) VALUES (@restBur, 20.00, 10.0);
SET @z1Bur = SCOPE_IDENTITY();
INSERT INTO DeliveryFeeRule (delivery_zone_id, max_order_value, delivery_fee) VALUES (@z1Bur, 999.00, 2.50);

-- ============================================================
-- 4. Restaurant Sakura Sushi (Wien)
-- ============================================================
DECLARE @addrSak INT;
DECLARE @restSak INT;
DECLARE @z1Sak   INT;
DECLARE @cSSus   INT;

INSERT INTO Address (street, number, zip_code, city, country, longitude, latitude)
VALUES ('Mariahilfer Strasse', '88', '1060', 'Wien', 'AT', 16.3540, 48.1970);
SET @addrSak = SCOPE_IDENTITY();

INSERT INTO Restaurant (name, address_id, webhook_url, title_image_path, api_key)
VALUES ('Sakura Sushi', @addrSak, 'https://webhooks.sakura-sushi.at/orders', 'img/sakura.png', @hashSak);
SET @restSak = SCOPE_IDENTITY();

INSERT INTO MenuCategory (restaurant_id, name) VALUES (@restSak, 'Sushi');
SET @cSSus = SCOPE_IDENTITY();

INSERT INTO MenuItem (restaurant_id, name, description, price) VALUES
    (@restSak, 'Sake Nigiri', 'Lachs (2 Stk.)', 4.80),
    (@restSak, 'California Roll', 'Avocado & Krabbe (8 Stk.)', 8.90);

INSERT INTO MenuItemMenuCategory (menu_item_id, menu_category_id, restaurant_id)
SELECT id, @cSSus, @restSak FROM MenuItem WHERE restaurant_id = @restSak;

INSERT INTO OpeningHourSlot (restaurant_id, day_of_week, open_time, close_time) VALUES
    (@restSak, 0, '12:00', '22:00'), (@restSak, 1, '12:00', '22:00'), (@restSak, 2, '12:00', '22:00'),
    (@restSak, 3, '12:00', '22:00'), (@restSak, 4, '12:00', '22:00'), (@restSak, 5, '12:00', '22:00'), (@restSak, 6, '12:00', '22:00');

INSERT INTO DeliveryZone (restaurant_id, min_order_value, max_distance) VALUES (@restSak, 25.00, 12.0);
SET @z1Sak = SCOPE_IDENTITY();
INSERT INTO DeliveryFeeRule (delivery_zone_id, max_order_value, delivery_fee) VALUES (@z1Sak, 999.00, 3.90);

-- ============================================================
-- 5. Create 20 Unique Student Locations
-- ============================================================
DECLARE @Locs TABLE (Id INT IDENTITY(1,1), City NVARCHAR(50), Street NVARCHAR(50), Zip VARCHAR(10), Lat FLOAT, Lon FLOAT);
INSERT INTO @Locs (City, Street, Zip, Lat, Lon) VALUES
('Hagenberg', 'Softwarepark', '4232', 48.3692, 14.5125),
('Hagenberg', 'Hauptstrasse', '4232', 48.3650, 14.5180),
('Linz', 'Landstrasse', '4020', 48.3000, 14.2900),
('Linz', 'Hauptplatz', '4020', 48.3060, 14.2860),
('Linz', 'Urfahr', '4040', 48.3150, 14.2850),
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
('Linz', 'Bindermichl', '4020', 48.2750, 14.2950),
('Hagenberg', 'Mahrersdorf', '4232', 48.3750, 14.5250),
('Wien', 'Floridsdorf', '1210', 48.2550, 16.4000),
('Linz', 'Ebelsberg', '4030', 48.2450, 14.3300),
('Wien', 'Simmering', '1110', 48.1750, 16.4150);

DECLARE @i INT = 1;
WHILE @i <= 20
BEGIN
    INSERT INTO Address (street, number, zip_code, city, country, longitude, latitude, additional_info)
    SELECT Street, CAST(@i AS NVARCHAR(10)), Zip, City, 'AT', Lon, Lat, 'Room ' + CAST(400+@i AS NVARCHAR(10))
    FROM @Locs WHERE Id = @i;
    SET @i = @i + 1;
END

PRINT '20 Unique Student addresses inserted.';

-- ============================================================
-- 6. Create 30 Geographically Plausible Orders
-- ============================================================
DECLARE @orderIter INT = 1;
DECLARE @sAddrId INT;
DECLARE @rId INT;
DECLARE @sCity NVARCHAR(50);
DECLARE @oStatus NVARCHAR(30);

WHILE @orderIter <= 30
BEGIN
    SELECT TOP 1 @sAddrId = id, @sCity = city FROM Address WHERE id > 3 ORDER BY NEWID();

    IF @sCity = 'Wien'
        SET @rId = CASE WHEN RAND() > 0.5 THEN @restBur ELSE @restSak END;
    ELSE
        SET @rId = @restNim;

    SET @oStatus = CASE (CAST(RAND()*6 AS INT) % 6)
        WHEN 0 THEN 'RECEIVED' WHEN 1 THEN 'SENT_TO_RESTAURANT' WHEN 2 THEN 'IN_PREPARATION'
        WHEN 3 THEN 'OUT_FOR_DELIVERY' WHEN 4 THEN 'DELIVERED' ELSE 'CANCELLED' END;

    DECLARE @oCode VARCHAR(16) = SUBSTRING(REPLACE(NEWID(), '-', ''), 1, 8);
    DECLARE @fee DECIMAL(10,2) = 2.50;

    INSERT INTO CustomerOrder (restaurant_id, address_id, order_code, status, delivery_fee, total)
    VALUES (@rId, @sAddrId, @oCode, @oStatus, @fee, 0);

    DECLARE @oId INT = SCOPE_IDENTITY();
    DECLARE @subtotal DECIMAL(10,2) = 0;

    DECLARE @mId INT; DECLARE @mPrice DECIMAL(10,2);
    SELECT TOP 1 @mId = id, @mPrice = price FROM MenuItem WHERE restaurant_id = @rId ORDER BY NEWID();

    INSERT INTO OrderItem (order_id, menu_item_id, quantity, unit_price)
    VALUES (@oId, @mId, 1, @mPrice);

    SET @subtotal = @mPrice;

    UPDATE CustomerOrder SET total = @subtotal + @fee WHERE id = @oId;
    SET @orderIter = @orderIter + 1;
END

PRINT '30 Geographically varied orders inserted.';

PRINT 'Test data filled successfully.';

EndOfScript:
PRINT 'Script completed.';
GO