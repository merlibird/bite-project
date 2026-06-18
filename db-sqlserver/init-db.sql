-- ============================================================
-- init-db.sql
-- Creates the database "$(DbName)" and all necessary tables for the Bite project.
-- The target database name is passed in via the sqlcmd variable DbName.
-- Called via sqlcmd:
--   sqlcmd -S db -U sa -P <password> -C -v DbName=BiteTestDb -i init-db.sql
-- ============================================================

-- 1. Check if the database '$(DbName)' exists
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'$(DbName)')
BEGIN
    CREATE DATABASE [$(DbName)];
    PRINT 'Database "$(DbName)" created.';
END
ELSE
    PRINT 'Database "$(DbName)" already exists.';
GO

USE [$(DbName)];
GO

-- 2. Delete existing tables (if any)
DROP TABLE IF EXISTS WebhookOutbox;
DROP TABLE IF EXISTS OrderStatusToken;
DROP TABLE IF EXISTS OrderItem;
DROP TABLE IF EXISTS CustomerOrder;
DROP TABLE IF EXISTS DeliveryFeeRule;
DROP TABLE IF EXISTS DeliveryZone;
DROP TABLE IF EXISTS MenuItemMenuCategory;
DROP TABLE IF EXISTS MenuItem;
DROP TABLE IF EXISTS MenuCategory;
DROP TABLE IF EXISTS OpeningHourSlot;
DROP TABLE IF EXISTS Restaurant;
DROP TABLE IF EXISTS Address;
PRINT 'Existing tables dropped (if any).';
GO

-- 3. Create tables

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

CREATE TABLE Restaurant (
    id               INT IDENTITY(1,1),
    name             NVARCHAR(100) NOT NULL,
    address_id       INT NOT NULL,
    webhook_url      NVARCHAR(255),
    title_image_path NVARCHAR(255),
    api_key          NVARCHAR(100) NOT NULL,
    created_at       DATETIME DEFAULT GETDATE(),
    updated_at       DATETIME DEFAULT GETDATE(),
    CONSTRAINT PK_Restaurant PRIMARY KEY (id),
    CONSTRAINT UQ_Restaurant_ApiKey UNIQUE (api_key),
    CONSTRAINT FK_Restaurant_Address FOREIGN KEY (address_id) REFERENCES Address(id)
);
PRINT 'Table "Restaurant" created.';

CREATE TABLE MenuCategory (
    id            INT IDENTITY(1,1),
    restaurant_id INT          NOT NULL,
    name          NVARCHAR(50) NOT NULL,
    is_active     BIT          NOT NULL DEFAULT 1,
    CONSTRAINT PK_MenuCategory PRIMARY KEY (id),
    CONSTRAINT UQ_MenuCategory_Restaurant_Name UNIQUE (restaurant_id, name),
    CONSTRAINT UQ_MenuCategory_Id_Restaurant UNIQUE (id, restaurant_id),
    CONSTRAINT FK_MenuCategory_Restaurant FOREIGN KEY (restaurant_id) REFERENCES Restaurant(id) ON DELETE CASCADE
);
PRINT 'Table "MenuCategory" created.';

CREATE TABLE MenuItem (
    id          INT IDENTITY(1,1),
    restaurant_id INT          NOT NULL,
    name        NVARCHAR(100)  NOT NULL,
    description NVARCHAR(255),
    price       DECIMAL(10, 2) NOT NULL,
    is_active   BIT            NOT NULL DEFAULT 1,
    created_at  DATETIME       DEFAULT GETDATE(),
    updated_at  DATETIME       DEFAULT GETDATE(),
    CONSTRAINT PK_MenuItem PRIMARY KEY (id),
    CONSTRAINT UQ_MenuItem_Id_Restaurant UNIQUE (id, restaurant_id),
    CONSTRAINT FK_MenuItem_Restaurant FOREIGN KEY (restaurant_id) REFERENCES Restaurant(id)
);
PRINT 'Table "MenuItem" created.';

CREATE TABLE MenuItemMenuCategory (
    menu_item_id     INT NOT NULL,
    menu_category_id INT NOT NULL,
    restaurant_id    INT NOT NULL,
    CONSTRAINT PK_MenuItemMenuCategory PRIMARY KEY (menu_item_id, menu_category_id),
    CONSTRAINT FK_MenuItemMenuCategory_MenuItem FOREIGN KEY (menu_item_id, restaurant_id) REFERENCES MenuItem(id, restaurant_id) ON DELETE CASCADE,
    CONSTRAINT FK_MenuItemMenuCategory_MenuCategory FOREIGN KEY (menu_category_id, restaurant_id) REFERENCES MenuCategory(id, restaurant_id) ON DELETE CASCADE
);
PRINT 'Table "MenuItemMenuCategory" created.';

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

CREATE TABLE CustomerOrder (
    id            INT IDENTITY(1,1),
    restaurant_id INT           NOT NULL,
    address_id    INT           NOT NULL,
    order_code    VARCHAR(16)   NOT NULL,
    status        NVARCHAR(30)  NOT NULL,
    delivery_fee  DECIMAL(10,2) NOT NULL CHECK(delivery_fee >= 0),
    total         DECIMAL(10,2) NOT NULL CHECK(total >= 0),
    created_at    DATETIME      DEFAULT GETDATE(),
    updated_at    DATETIME      DEFAULT GETDATE(),
    CONSTRAINT PK_CustomerOrder PRIMARY KEY (id),
    CONSTRAINT UQ_CustomerOrder_OrderCode UNIQUE (order_code),
    CONSTRAINT CK_CustomerOrder_Status CHECK (status IN (
        'RECEIVED', 'SENT_TO_RESTAURANT', 'IN_PREPARATION', 'OUT_FOR_DELIVERY', 'DELIVERED', 'CANCELLED'
    )),
    CONSTRAINT FK_CustomerOrder_Restaurant FOREIGN KEY (restaurant_id) REFERENCES Restaurant(id),
    CONSTRAINT FK_CustomerOrder_Address FOREIGN KEY (address_id) REFERENCES Address(id)
);
PRINT 'Table "CustomerOrder" created.';

CREATE TABLE OrderItem (
    id           INT IDENTITY(1,1),
    order_id     INT           NOT NULL,
    menu_item_id INT           NOT NULL,
    quantity     INT           NOT NULL CHECK(quantity > 0),
    unit_price   DECIMAL(10,2) NOT NULL CHECK(unit_price >= 0),
    CONSTRAINT PK_OrderItem PRIMARY KEY (id),
    CONSTRAINT FK_OrderItem_CustomerOrder FOREIGN KEY (order_id) REFERENCES CustomerOrder(id) ON DELETE CASCADE,
    CONSTRAINT FK_OrderItem_MenuItem FOREIGN KEY (menu_item_id) REFERENCES MenuItem(id)
);
PRINT 'Table "OrderItem" created.';

CREATE TABLE OrderStatusToken (
    id            INT IDENTITY(1,1),
    order_id      INT           NOT NULL,
    token         VARCHAR(64)   NOT NULL,
    target_status NVARCHAR(30)  NOT NULL,
    used          BIT           NOT NULL DEFAULT 0,
    expires_at    DATETIME      NOT NULL,
    created_at    DATETIME      DEFAULT GETDATE(),
    CONSTRAINT PK_OrderStatusToken PRIMARY KEY (id),
    CONSTRAINT UQ_OrderStatusToken_Token UNIQUE (token),
    CONSTRAINT CK_OrderStatusToken_TargetStatus CHECK (target_status IN (
        'RECEIVED', 'SENT_TO_RESTAURANT', 'IN_PREPARATION', 'OUT_FOR_DELIVERY', 'DELIVERED', 'CANCELLED'
    )),
    CONSTRAINT FK_OrderStatusToken_CustomerOrder FOREIGN KEY (order_id) REFERENCES CustomerOrder(id) ON DELETE CASCADE
);
CREATE INDEX IX_OrderStatusToken_OrderId ON OrderStatusToken(order_id);
CREATE INDEX IX_OrderStatusToken_ExpiresAt ON OrderStatusToken(expires_at);
PRINT 'Table "OrderStatusToken" created.';

CREATE TABLE WebhookOutbox (
    id              INT IDENTITY(1,1),
    order_id        INT            NOT NULL,
    url             NVARCHAR(2048) NOT NULL,
    payload         NVARCHAR(MAX)  NOT NULL,
    status          NVARCHAR(20)   NOT NULL DEFAULT 'PENDING',
    attempts        INT            NOT NULL DEFAULT 0,
    next_attempt_at DATETIME       NOT NULL DEFAULT GETUTCDATE(),
    created_at      DATETIME       DEFAULT GETDATE(),
    updated_at      DATETIME       DEFAULT GETDATE(),
    CONSTRAINT PK_WebhookOutbox PRIMARY KEY (id),
    CONSTRAINT CK_WebhookOutbox_Status CHECK (status IN ('PENDING', 'SENT', 'FAILED')),
    CONSTRAINT FK_WebhookOutbox_CustomerOrder FOREIGN KEY (order_id) REFERENCES CustomerOrder(id) ON DELETE CASCADE
);
PRINT 'Table "WebhookOutbox" created.';

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

CREATE TRIGGER TR_CustomerOrder_UpdatedAt
ON CustomerOrder
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE CustomerOrder
    SET updated_at = GETDATE()
    WHERE id IN (SELECT id FROM inserted);
END;
PRINT 'Trigger "TR_CustomerOrder_UpdatedAt" created.';
GO

CREATE TRIGGER TR_WebhookOutbox_UpdatedAt
ON WebhookOutbox
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE WebhookOutbox
    SET updated_at = GETDATE()
    WHERE id IN (SELECT id FROM inserted);
END;
PRINT 'Trigger "TR_WebhookOutbox_UpdatedAt" created.';
GO
