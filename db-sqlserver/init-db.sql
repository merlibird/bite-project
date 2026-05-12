-- ============================================================
-- init-db.sql
-- Erstellt die Datenbank BiteTestDb und alle Tabellen.
-- Entspricht DatabaseSetup.cs (CreateDatabase + ResetTables + CreateTables)
-- Aufruf via sqlcmd:
--   sqlcmd -S db -U sa -P <password> -C -i init-db.sql
-- ============================================================

-- 1. Datenbank anlegen (falls nicht vorhanden)
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'BiteTestDb')
BEGIN
    CREATE DATABASE BiteTestDb;
    PRINT 'Datenbank BiteTestDb erstellt.';
END
ELSE
    PRINT 'Datenbank BiteTestDb existiert bereits.';
GO

USE BiteTestDb;
GO

-- 2. Tabellen löschen (Reihenfolge: abhängige zuerst wegen Foreign Keys)
IF OBJECT_ID('DeliveryFeeRule',  'U') IS NOT NULL DROP TABLE DeliveryFeeRule;
IF OBJECT_ID('DeliveryZone',     'U') IS NOT NULL DROP TABLE DeliveryZone;
IF OBJECT_ID('MenuItem',         'U') IS NOT NULL DROP TABLE MenuItem;
IF OBJECT_ID('MenuCategory',     'U') IS NOT NULL DROP TABLE MenuCategory;
IF OBJECT_ID('OpeningHourSlot',  'U') IS NOT NULL DROP TABLE OpeningHourSlot;
IF OBJECT_ID('Restaurant',       'U') IS NOT NULL DROP TABLE Restaurant;
IF OBJECT_ID('Address',          'U') IS NOT NULL DROP TABLE Address;
IF OBJECT_ID('Menu',             'U') IS NOT NULL DROP TABLE Menu;
PRINT 'Alle vorhandenen Tabellen gelöscht.';
GO

-- 3. Tabellen erstellen
-- (Inhalt aus testing-db-tsql.sql – hier als Platzhalter; eigene CREATE TABLE Statements einfügen)

CREATE TABLE Menu (
    id INT IDENTITY(1,1) PRIMARY KEY
);

CREATE TABLE Address (
    id         INT IDENTITY(1,1) PRIMARY KEY,
    street     VARCHAR(100) NOT NULL,
    number     VARCHAR(10)  NOT NULL,
    zip_code   VARCHAR(10)  NOT NULL,
    city       VARCHAR(100) NOT NULL,
    country    VARCHAR(50)  NOT NULL,
    longitude  FLOAT        NOT NULL,
    latitude   FLOAT        NOT NULL
);

CREATE TABLE MenuCategory (
    id   INT IDENTITY(1,1) PRIMARY KEY,
    name VARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE Restaurant (
    id               INT IDENTITY(1,1) PRIMARY KEY,
    name             VARCHAR(100) NOT NULL,
    menu_id          INT NOT NULL REFERENCES Menu(id),
    address_id       INT NOT NULL REFERENCES Address(id),
    webhook_url      VARCHAR(255),
    title_image_path VARCHAR(255)
);

CREATE TABLE MenuItem (
    id          INT IDENTITY(1,1) PRIMARY KEY,
    menu_id     INT            NOT NULL REFERENCES Menu(id),
    category_id INT            NOT NULL REFERENCES MenuCategory(id),
    name        VARCHAR(100)   NOT NULL,
    description VARCHAR(255),
    price       DECIMAL(10, 2) NOT NULL
);

CREATE TABLE OpeningHourSlot (
    id            INT IDENTITY(1,1) PRIMARY KEY,
    restaurant_id INT         NOT NULL REFERENCES Restaurant(id),
    day_of_week   INT         NOT NULL, -- 0=So, 1=Mo, ..., 6=Sa
    open_time     TIME        NOT NULL,
    close_time    TIME        NOT NULL
);

CREATE TABLE DeliveryZone (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    restaurant_id   INT            NOT NULL REFERENCES Restaurant(id),
    min_order_value DECIMAL(10, 2) NOT NULL,
    max_distance    FLOAT          NOT NULL
);

CREATE TABLE DeliveryFeeRule (
    id               INT IDENTITY(1,1) PRIMARY KEY,
    delivery_zone_id INT            NOT NULL REFERENCES DeliveryZone(id),
    max_order_value  DECIMAL(10, 2) NOT NULL,
    delivery_fee     DECIMAL(10, 2) NOT NULL
);

PRINT 'Alle Tabellen erfolgreich erstellt.';
GO
