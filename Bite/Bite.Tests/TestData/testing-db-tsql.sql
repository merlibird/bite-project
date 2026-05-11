-- Address
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Address]') AND type = N'U')
CREATE TABLE Address (
    id INT PRIMARY KEY IDENTITY(1,1),
    street NVARCHAR(255) NOT NULL,
    number NVARCHAR(50) NOT NULL,
    zip_code NVARCHAR(20) NOT NULL,
    city NVARCHAR(100) NOT NULL,
    country NVARCHAR(100) NOT NULL,
    additional_info NVARCHAR(MAX),
    longitude FLOAT NOT NULL CHECK(longitude BETWEEN -180 AND 180),
    latitude FLOAT NOT NULL CHECK(latitude BETWEEN -90 AND 90)
);

-- Menu
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Menu]') AND type = N'U')
CREATE TABLE Menu (
    id INT PRIMARY KEY IDENTITY(1,1)
);

-- Restaurant
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Restaurant]') AND type = N'U')
CREATE TABLE Restaurant (
    id INT PRIMARY KEY IDENTITY(1,1),
    name NVARCHAR(255) NOT NULL,
    menu_id INT NOT NULL,
    address_id INT NOT NULL,
    webhook_url NVARCHAR(MAX) NOT NULL,
    title_image_path NVARCHAR(MAX) NOT NULL,
    created_at DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Restaurant_Menu FOREIGN KEY (menu_id) REFERENCES Menu(id),
    CONSTRAINT FK_Restaurant_Address FOREIGN KEY (address_id) REFERENCES Address(id)
);

-- OpeningHourSlot
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OpeningHourSlot]') AND type = N'U')
CREATE TABLE OpeningHourSlot (
    id INT PRIMARY KEY IDENTITY(1,1),
    restaurant_id INT NOT NULL,
    day_of_week INT NOT NULL CHECK(day_of_week BETWEEN 0 AND 6),
    open_time TIME NOT NULL,
    close_time TIME NOT NULL,
    CONSTRAINT FK_OpeningHourSlot_Restaurant FOREIGN KEY (restaurant_id) REFERENCES Restaurant(id) ON DELETE CASCADE
);

-- MenuCategory
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MenuCategory]') AND type = N'U')
CREATE TABLE MenuCategory (
    id INT PRIMARY KEY IDENTITY(1,1),
    name NVARCHAR(255) NOT NULL UNIQUE
);

-- MenuItem
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MenuItem]') AND type = N'U')
CREATE TABLE MenuItem (
    id INT PRIMARY KEY IDENTITY(1,1),
    menu_id INT NOT NULL,
    category_id INT NOT NULL,
    name NVARCHAR(255) NOT NULL,
    description NVARCHAR(MAX),
    price DECIMAL(10,2) NOT NULL CHECK(price >= 0),
    created_at DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_MenuItem_Category FOREIGN KEY (category_id) REFERENCES MenuCategory(id),
    CONSTRAINT FK_MenuItem_Menu FOREIGN KEY (menu_id) REFERENCES Menu(id) ON DELETE CASCADE
);

-- DeliveryZone
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DeliveryZone]') AND type = N'U')
CREATE TABLE DeliveryZone (
    id INT PRIMARY KEY IDENTITY(1,1),
    restaurant_id INT NOT NULL,
    min_order_value DECIMAL(10,2) NOT NULL CHECK(min_order_value >= 0),
    max_distance FLOAT NOT NULL CHECK(max_distance >= 0),
    CONSTRAINT FK_DeliveryZone_Restaurant FOREIGN KEY (restaurant_id) REFERENCES Restaurant(id) ON DELETE CASCADE
);

-- DeliveryFeeRule
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DeliveryFeeRule]') AND type = N'U')
CREATE TABLE DeliveryFeeRule (
    id INT PRIMARY KEY IDENTITY(1,1),
    delivery_zone_id INT NOT NULL,
    max_order_value DECIMAL(10,2) NOT NULL CHECK(max_order_value >= 0),
    delivery_fee DECIMAL(10,2) NOT NULL CHECK(delivery_fee >= 0),
    CONSTRAINT FK_DeliveryFeeRule_DeliveryZone FOREIGN KEY (delivery_zone_id) REFERENCES DeliveryZone(id) ON DELETE CASCADE
);