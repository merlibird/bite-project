-- ============================================================
-- init-db.sql
-- Creates the database "BiteTestDb" and all necessary tables for the Bite project.
-- Called via sqlcmd:
--   sqlcmd -S db -U sa -P <password> -C -i init-db.sql
-- ============================================================

-- 1. Check if the database 'BiteTestDb' exists
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'BiteTestDb')
BEGIN
    CREATE DATABASE BiteTestDb;
    PRINT 'Database "BiteTestDb" created.';
END
ELSE
    PRINT 'Database "BiteTestDb" already exists.';
GO

USE BiteTestDb;
GO

-- 2. Delete existing tables (if any)
DROP TABLE IF EXISTS DeliveryFeeRule;
DROP TABLE IF EXISTS DeliveryZone;
DROP TABLE IF EXISTS MenuItem;
DROP TABLE IF EXISTS MenuCategory;
DROP TABLE IF EXISTS OpeningHourSlot;
DROP TABLE IF EXISTS Restaurant;
DROP TABLE IF EXISTS Address;
DROP TABLE IF EXISTS Menu;
PRINT 'Existing tables dropped (if any).';
GO

-- 3. Create tables
CREATE TABLE Menu (
    id INT IDENTITY(1,1),
    CONSTRAINT PK_Menu PRIMARY KEY (id)
);
PRINT 'Table "Menu" created.';

CREATE TABLE Address (
    id              INT IDENTITY(1,1),
    street          NVARCHAR(255) NOT NULL,
    number          NVARCHAR(50)  NOT NULL,
    zip_code        VARCHAR(20)   NOT NULL,
    city            NVARCHAR(100) NOT NULL,
    country         NVARCHAR(100) NOT NULL,
    longitude       FLOAT         NOT NULL CHECK(longitude BETWEEN -180 AND 180),
    latitude        FLOAT         NOT NULL CHECK(latitude BETWEEN -90 AND 90),
    additional_info NVARCHAR(255),
    CONSTRAINT PK_Address PRIMARY KEY (id)
);
PRINT 'Table "Address" created.';

CREATE TABLE MenuCategory (
    id   INT IDENTITY(1,1),
    name NVARCHAR(50)       NOT NULL UNIQUE,
    CONSTRAINT PK_MenuCategory PRIMARY KEY (id)
);
PRINT 'Table "MenuCategory" created.';

CREATE TABLE Restaurant (
    id               INT IDENTITY(1,1),
    name             NVARCHAR(100) NOT NULL,
    menu_id          INT NOT NULL,
    address_id       INT NOT NULL,
    webhook_url      NVARCHAR(255),
    title_image_path NVARCHAR(255),
    created_at       DATETIME DEFAULT GETDATE(),
    updated_at       DATETIME DEFAULT GETDATE(),
    CONSTRAINT PK_Restaurant PRIMARY KEY (id),
    CONSTRAINT FK_Restaurant_Menu FOREIGN KEY (menu_id) REFERENCES Menu(id),
    CONSTRAINT FK_Restaurant_Address FOREIGN KEY (address_id) REFERENCES Address(id)
);
PRINT 'Table "Restaurant" created.';

CREATE TABLE MenuItem (
    id          INT IDENTITY(1,1),
    menu_id     INT            NOT NULL,
    category_id INT            NOT NULL,
    name        NVARCHAR(100)  NOT NULL,
    description NVARCHAR(255),
    price       DECIMAL(10, 2) NOT NULL,
    is_active   BIT            NOT NULL DEFAULT 1,
    created_at  DATETIME       DEFAULT GETDATE(),
    updated_at  DATETIME       DEFAULT GETDATE(),
    CONSTRAINT PK_MenuItem PRIMARY KEY (id),
    CONSTRAINT FK_MenuItem_Menu FOREIGN KEY (menu_id) REFERENCES Menu(id),
    CONSTRAINT FK_MenuItem_Category FOREIGN KEY (category_id) REFERENCES MenuCategory(id)
);
PRINT 'Table "MenuItem" created.';

CREATE TABLE OpeningHourSlot (
    id            INT IDENTITY(1,1),
    restaurant_id INT         NOT NULL,
    day_of_week   INT         NOT NULL CHECK(day_of_week BETWEEN 0 AND 6),
    open_time     TIME        NOT NULL,
    close_time    TIME        NOT NULL,
    CONSTRAINT PK_OpeningHourSlot PRIMARY KEY (id),
    CONSTRAINT FK_OpeningHourSlot_Restaurant FOREIGN KEY (restaurant_id) REFERENCES Restaurant(id) ON DELETE CASCADE
);
PRINT 'Table "OpeningHourSlot" created.';

CREATE TABLE DeliveryZone (
    id              INT IDENTITY(1,1),
    restaurant_id   INT            NOT NULL,
    min_order_value DECIMAL(10, 2) NOT NULL CHECK(min_order_value >= 0),
    max_distance    FLOAT          NOT NULL CHECK(max_distance >= 0),
    CONSTRAINT PK_DeliveryZone PRIMARY KEY (id),
    CONSTRAINT FK_DeliveryZone_Restaurant FOREIGN KEY (restaurant_id) REFERENCES Restaurant(id) ON DELETE CASCADE
);
PRINT 'Table "DeliveryZone" created.';

CREATE TABLE DeliveryFeeRule (
    id               INT IDENTITY(1,1),
    delivery_zone_id INT            NOT NULL,
    max_order_value  DECIMAL(10, 2) NOT NULL CHECK(max_order_value >= 0),
    delivery_fee     DECIMAL(10, 2) NOT NULL CHECK(delivery_fee >= 0),
    CONSTRAINT PK_DeliveryFeeRule PRIMARY KEY (id),
    CONSTRAINT FK_DeliveryFeeRule_DeliveryZone FOREIGN KEY (delivery_zone_id) REFERENCES DeliveryZone(id) ON DELETE CASCADE
);
PRINT 'Table "DeliveryFeeRule" created.';

PRINT 'All tables created successfully.';
GO

-- 4. Create triggers

CREATE TRIGGER TR_Restaurant_UpdatedAt
ON Restaurant
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Restaurant
    SET updated_at = GETDATE()
    WHERE id IN (SELECT id FROM inserted);
END;
PRINT 'Trigger "TR_Restaurant_UpdatedAt" created.';
GO

CREATE TRIGGER TR_MenuItem_UpdatedAt
ON MenuItem
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE MenuItem
    SET updated_at = GETDATE()
    WHERE id IN (SELECT id FROM inserted);
END;
PRINT 'Trigger "TR_MenuItem_UpdatedAt" created.';
GO