
USE ClipperCoffeeCorner;
GO

/* =======================================================================
   COFFEE SHOP ORDERING DATABASE
   FULL SCHEMA + SEED DATA + ALL COMBINATIONS
   ======================================================================= */

-----------------------
-- CLEANUP (safe to re-run)
-----------------------
IF OBJECT_ID('dbo.OrderItem', 'U') IS NOT NULL DROP TABLE dbo.OrderItem;
IF OBJECT_ID('dbo.[Order]', 'U')     IS NOT NULL DROP TABLE dbo.[Order];
IF OBJECT_ID('dbo.CombinationOption', 'U') IS NOT NULL DROP TABLE dbo.CombinationOption;
IF OBJECT_ID('dbo.Combination', 'U')       IS NOT NULL DROP TABLE dbo.Combination;
IF OBJECT_ID('dbo.MenuItemOptionGroup', 'U') IS NOT NULL DROP TABLE dbo.MenuItemOptionGroup;
IF OBJECT_ID('dbo.OptionValue', 'U')      IS NOT NULL DROP TABLE dbo.OptionValue;
IF OBJECT_ID('dbo.OptionGroup', 'U')      IS NOT NULL DROP TABLE dbo.OptionGroup;
IF OBJECT_ID('dbo.MenuItem', 'U')         IS NOT NULL DROP TABLE dbo.MenuItem;
IF OBJECT_ID('dbo.MenuCategory', 'U')     IS NOT NULL DROP TABLE dbo.MenuCategory;
IF OBJECT_ID('dbo.[Password]', 'U')       IS NOT NULL DROP TABLE dbo.[Password];
IF OBJECT_ID('dbo.[User]', 'U')           IS NOT NULL DROP TABLE dbo.[User];

-----------------------
-- USERS & PASSWORDS
-----------------------
CREATE TABLE [User] (
    UserId           INT IDENTITY(1,1)       NOT NULL PRIMARY KEY,
    UserName         NVARCHAR(50)            NOT NULL UNIQUE,
    UserRole         NVARCHAR(20)            NOT NULL DEFAULT 'Customer',
    Email            NVARCHAR(100)           NULL,
    PhoneNumber      NVARCHAR(20)            NULL,
    NotificationPref NVARCHAR(20)            NULL,   -- 'SMS','Email','None'
    CreatedAt        DATETIME2               NOT NULL DEFAULT SYSUTCDATETIME(),
    LastLoginAt      DATETIME2               NULL,
    SquareCustomerId NVARCHAR(200) 	     NULL
);

CREATE TABLE [Password] (
    PasswordId   INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    UserId       INT               NOT NULL,
    PasswordHash VARBINARY(256)    NOT NULL,
    PasswordSalt VARBINARY(128)    NULL,
    CreatedAt    DATETIME2         NOT NULL DEFAULT SYSUTCDATETIME(),
    IsActive     BIT               NOT NULL DEFAULT 1,
    CONSTRAINT FK_Password_User FOREIGN KEY (UserId)
        REFERENCES [User](UserId)
);

-----------------------
-- MENU STRUCTURE
-----------------------
CREATE TABLE MenuCategory (
    MenuCategoryId   INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    ParentCategoryId INT               NULL,
    Name             NVARCHAR(50)      NOT NULL,
    CONSTRAINT FK_MenuCategory_Parent FOREIGN KEY (ParentCategoryId)
        REFERENCES MenuCategory(MenuCategoryId)
);

CREATE TABLE MenuItem (
    MenuItemId     INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    MenuCategoryId INT               NOT NULL,
    Name           NVARCHAR(100)     NOT NULL,
    BasePrice      DECIMAL(10,2)     NOT NULL,
    IsActive       BIT               NOT NULL DEFAULT 1,
    Description    NVARCHAR(400)     NULL,
    CONSTRAINT FK_MenuItem_MenuCategory FOREIGN KEY (MenuCategoryId)
        REFERENCES MenuCategory(MenuCategoryId)
);

CREATE TABLE OptionGroup (
    OptionGroupId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Name          NVARCHAR(50)      NOT NULL,
    Description   NVARCHAR(200)     NULL
);

CREATE TABLE OptionValue (
    OptionValueId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    OptionGroupId INT               NOT NULL,
    Name          NVARCHAR(50)      NOT NULL,
    DisplayOrder  INT               NOT NULL DEFAULT 0,
    ExtraPrice    DECIMAL(10,2)     NOT NULL DEFAULT 0.00,
    CONSTRAINT FK_OptionValue_OptionGroup FOREIGN KEY (OptionGroupId)
        REFERENCES OptionGroup(OptionGroupId)
);

CREATE TABLE MenuItemOptionGroup (
    MenuItemId    INT NOT NULL,
    OptionGroupId INT NOT NULL,
    IsRequired    BIT NOT NULL DEFAULT 0,
    MinChoices    INT NOT NULL DEFAULT 0,
    MaxChoices    INT NOT NULL DEFAULT 1,
    CONSTRAINT PK_MenuItemOptionGroup PRIMARY KEY (MenuItemId, OptionGroupId),
    CONSTRAINT FK_MIOG_MenuItem FOREIGN KEY (MenuItemId)
        REFERENCES MenuItem(MenuItemId),
    CONSTRAINT FK_MIOG_OptionGroup FOREIGN KEY (OptionGroupId)
        REFERENCES OptionGroup(OptionGroupId)
);

-----------------------
-- COMBINATIONS
-----------------------
CREATE TABLE Combination (
    CombinationId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    MenuItemId    INT               NOT NULL,
    Code          NVARCHAR(50)      NULL,  -- SKU / internal code
    Price         DECIMAL(10,2)     NOT NULL,
    IsActive      BIT               NOT NULL DEFAULT 1,
    CONSTRAINT FK_Combination_MenuItem FOREIGN KEY (MenuItemId)
        REFERENCES MenuItem(MenuItemId)
);

CREATE TABLE CombinationOption (
    CombinationId INT NOT NULL,
    OptionValueId INT NOT NULL,
    CONSTRAINT PK_CombinationOption PRIMARY KEY (CombinationId, OptionValueId),
    CONSTRAINT FK_CombinationOption_Combination FOREIGN KEY (CombinationId)
        REFERENCES Combination(CombinationId),
    CONSTRAINT FK_CombinationOption_OptionValue FOREIGN KEY (OptionValueId)
        REFERENCES OptionValue(OptionValueId)
);

-----------------------
-- ORDERS
-----------------------
CREATE TABLE [Order] (
    OrderId        INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    UserId         INT               NULL,  -- NULL for guest
    IdempotencyKey UNIQUEIDENTIFIER  NOT NULL DEFAULT NEWID(),
    Status         NVARCHAR(20)      NOT NULL DEFAULT 'Pending',
    PlacedAt       DATETIME2         NOT NULL DEFAULT SYSUTCDATETIME(),
    CompletedAt    DATETIME2         NULL,
    TotalAmount    DECIMAL(10,2)     NOT NULL DEFAULT 0.00,
    CONSTRAINT UQ_Order_IdempotencyKey UNIQUE (IdempotencyKey),
    CONSTRAINT FK_Order_User FOREIGN KEY (UserId)
        REFERENCES [User](UserId)
);

CREATE TABLE OrderItem (
    OrderItemId   INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    OrderId       INT               NOT NULL,
    CombinationId INT               NOT NULL,
    Quantity      INT               NOT NULL DEFAULT 1,
    UnitPrice     DECIMAL(10,2)     NOT NULL,
    LineTotal     AS (Quantity * UnitPrice) PERSISTED,
    CONSTRAINT FK_OrderItem_Order FOREIGN KEY (OrderId)
        REFERENCES [Order](OrderId),
    CONSTRAINT FK_OrderItem_Combination FOREIGN KEY (CombinationId)
        REFERENCES Combination(CombinationId)
);

-----------------------------------------------------------------------
-- SEED DATA (from your menu image)
-----------------------------------------------------------------------

-----------------------
-- MENU CATEGORIES
-----------------------
DECLARE @DrinksId INT,
        @HotId INT, @ColdId INT,
        @BlendedCatId INT, @JuiceCatId INT, @WaterCatId INT,
        @HotCoffeeId INT, @HotTeaId INT,
        @ColdCoffeeId INT, @ColdTeaId INT;

INSERT INTO MenuCategory (ParentCategoryId, Name) VALUES (NULL, 'Drinks');
SET @DrinksId = SCOPE_IDENTITY();

INSERT INTO MenuCategory (ParentCategoryId, Name) VALUES (@DrinksId, 'Hot');
SET @HotId = SCOPE_IDENTITY();

INSERT INTO MenuCategory (ParentCategoryId, Name) VALUES (@DrinksId, 'Cold');
SET @ColdId = SCOPE_IDENTITY();

INSERT INTO MenuCategory (ParentCategoryId, Name) VALUES (@DrinksId, 'Blended');
SET @BlendedCatId = SCOPE_IDENTITY();

INSERT INTO MenuCategory (ParentCategoryId, Name) VALUES (@DrinksId, 'Juice');
SET @JuiceCatId = SCOPE_IDENTITY();

INSERT INTO MenuCategory (ParentCategoryId, Name) VALUES (@DrinksId, 'Water');
SET @WaterCatId = SCOPE_IDENTITY();

-- Subcategories under Hot / Cold
INSERT INTO MenuCategory (ParentCategoryId, Name) VALUES (@HotId, 'Coffee');
SET @HotCoffeeId = SCOPE_IDENTITY();

INSERT INTO MenuCategory (ParentCategoryId, Name) VALUES (@HotId, 'Tea');
SET @HotTeaId = SCOPE_IDENTITY();

INSERT INTO MenuCategory (ParentCategoryId, Name) VALUES (@ColdId, 'Coffee');
SET @ColdCoffeeId = SCOPE_IDENTITY();

INSERT INTO MenuCategory (ParentCategoryId, Name) VALUES (@ColdId, 'Tea');
SET @ColdTeaId = SCOPE_IDENTITY();

-----------------------
-- MENU ITEMS
-----------------------
DECLARE @HotLatteId INT, @AmericanoId INT, @EspressoId INT, @MochaId INT,
        @DripCoffeeId INT, @HotTeaLatteId INT,
        @ColdLatteId INT, @ColdTeaLatteId INT,
        @BlendedDrinkId INT, @JuiceItemId INT, @WaterItemId INT;

-- Hot Coffee items
INSERT INTO MenuItem (MenuCategoryId, Name, BasePrice, IsActive, Description)
VALUES (@HotCoffeeId, 'Hot Latte', 4.00, 1, 'Hot espresso with milk');
SET @HotLatteId = SCOPE_IDENTITY();

INSERT INTO MenuItem (MenuCategoryId, Name, BasePrice, IsActive, Description)
VALUES (@HotCoffeeId, 'Americano', 3.00, 1, 'Espresso with hot water');
SET @AmericanoId = SCOPE_IDENTITY();

INSERT INTO MenuItem (MenuCategoryId, Name, BasePrice, IsActive, Description)
VALUES (@HotCoffeeId, 'Espresso', 2.50, 1, 'Espresso shots');
SET @EspressoId = SCOPE_IDENTITY();

INSERT INTO MenuItem (MenuCategoryId, Name, BasePrice, IsActive, Description)
VALUES (@HotCoffeeId, 'Mocha', 4.50, 1, 'Espresso with chocolate and milk');
SET @MochaId = SCOPE_IDENTITY();

INSERT INTO MenuItem (MenuCategoryId, Name, BasePrice, IsActive, Description)
VALUES (@HotCoffeeId, 'Drip Coffee', 2.25, 1, 'Brewed coffee');
SET @DripCoffeeId = SCOPE_IDENTITY();

-- Hot Tea (Lattes: Chai / Matcha)
INSERT INTO MenuItem (MenuCategoryId, Name, BasePrice, IsActive, Description)
VALUES (@HotTeaId, 'Hot Tea Latte', 4.00, 1, 'Tea latte (Chai or Matcha)');
SET @HotTeaLatteId = SCOPE_IDENTITY();

-- Cold Coffee
INSERT INTO MenuItem (MenuCategoryId, Name, BasePrice, IsActive, Description)
VALUES (@ColdCoffeeId, 'Iced Latte', 4.25, 1, 'Iced latte with milk choice');
SET @ColdLatteId = SCOPE_IDENTITY();

-- Cold Tea
INSERT INTO MenuItem (MenuCategoryId, Name, BasePrice, IsActive, Description)
VALUES (@ColdTeaId, 'Iced Tea Latte', 4.25, 1, 'Iced Chai or Matcha latte');
SET @ColdTeaLatteId = SCOPE_IDENTITY();

-- Blended / Juice / Water
INSERT INTO MenuItem (MenuCategoryId, Name, BasePrice, IsActive, Description)
VALUES (@BlendedCatId, 'Blended Drink', 5.00, 1, 'Blended coffee or creme drink');
SET @BlendedDrinkId = SCOPE_IDENTITY();

INSERT INTO MenuItem (MenuCategoryId, Name, BasePrice, IsActive, Description)
VALUES (@JuiceCatId, 'Juice', 3.00, 1, 'Assorted juices');
SET @JuiceItemId = SCOPE_IDENTITY();

INSERT INTO MenuItem (MenuCategoryId, Name, BasePrice, IsActive, Description)
VALUES (@WaterCatId, 'Water', 0.00, 1, 'Water (limit 4 per order in app logic)');
SET @WaterItemId = SCOPE_IDENTITY();

-----------------------
-- OPTION GROUPS
-----------------------
DECLARE @MilkGroupId INT, @MochaFlavorGroupId INT, @TeaTypeGroupId INT,
        @ShotCountGroupId INT, @SizeGroupId INT, @IceGroupId INT;

INSERT INTO OptionGroup (Name, Description)
VALUES ('Milk', 'Dairy and non-dairy milk choices');
SET @MilkGroupId = SCOPE_IDENTITY();

INSERT INTO OptionGroup (Name, Description)
VALUES ('Mocha Flavor', 'Mocha flavor options');
SET @MochaFlavorGroupId = SCOPE_IDENTITY();

INSERT INTO OptionGroup (Name, Description)
VALUES ('Tea Type', 'Tea latte type');
SET @TeaTypeGroupId = SCOPE_IDENTITY();

INSERT INTO OptionGroup (Name, Description)
VALUES ('Shot Count', 'Number of espresso shots (limit 5–6)');
SET @ShotCountGroupId = SCOPE_IDENTITY();

INSERT INTO OptionGroup (Name, Description)
VALUES ('Size', 'Drink size');
SET @SizeGroupId = SCOPE_IDENTITY();

INSERT INTO OptionGroup (Name, Description)
VALUES ('Ice', 'Ice preference');
SET @IceGroupId = SCOPE_IDENTITY();

-----------------------
-- OPTION VALUES (from your menu)
-----------------------

-- Milk options: Dairy + Non-dairy
INSERT INTO OptionValue (OptionGroupId, Name, DisplayOrder, ExtraPrice)
VALUES
(@MilkGroupId, 'Breve (half and half)', 1, 0.50),
(@MilkGroupId, 'Heavy cream',           2, 0.75),
(@MilkGroupId, 'Nonfat',                3, 0.00),
(@MilkGroupId, 'Whole milk',            4, 0.00),
(@MilkGroupId, 'Almond',                5, 0.50),
(@MilkGroupId, 'Coconut',               6, 0.50),
(@MilkGroupId, 'Oat',                   7, 0.50),
(@MilkGroupId, 'Soy',                   8, 0.50);

-- Mocha: Plain / White
INSERT INTO OptionValue (OptionGroupId, Name, DisplayOrder, ExtraPrice)
VALUES
(@MochaFlavorGroupId, 'Plain', 1, 0.00),
(@MochaFlavorGroupId, 'White', 2, 0.00);

-- Tea lattes: Chai / Matcha
INSERT INTO OptionValue (OptionGroupId, Name, DisplayOrder, ExtraPrice)
VALUES
(@TeaTypeGroupId, 'Chai',   1, 0.00),
(@TeaTypeGroupId, 'Matcha', 2, 0.25);

-- Shot count 1–6
INSERT INTO OptionValue (OptionGroupId, Name, DisplayOrder, ExtraPrice)
VALUES
(@ShotCountGroupId, '1 shot', 1, 0.00),
(@ShotCountGroupId, '2 shots', 2, 0.50),
(@ShotCountGroupId, '3 shots', 3, 1.00),
(@ShotCountGroupId, '4 shots', 4, 1.50),
(@ShotCountGroupId, '5 shots', 5, 2.00),
(@ShotCountGroupId, '6 shots', 6, 2.50);

-- Generic sizes
INSERT INTO OptionValue (OptionGroupId, Name, DisplayOrder, ExtraPrice)
VALUES
(@SizeGroupId, 'Small',  1, 0.00),
(@SizeGroupId, 'Medium', 2, 0.50),
(@SizeGroupId, 'Large',  3, 1.00);

-- Ice options
INSERT INTO OptionValue (OptionGroupId, Name, DisplayOrder, ExtraPrice)
VALUES
(@IceGroupId, 'Ice',    1, 0.00),
(@IceGroupId, 'No Ice', 2, 0.00);

-----------------------
-- MENU ITEM ↔ OPTION GROUPS
-----------------------

-- Hot Latte: milk + size
INSERT INTO MenuItemOptionGroup (MenuItemId, OptionGroupId, IsRequired, MinChoices, MaxChoices) VALUES
(@HotLatteId,  @MilkGroupId, 1, 1, 1),
(@HotLatteId,  @SizeGroupId, 1, 1, 1);

-- Iced Latte: milk + size + ice
INSERT INTO MenuItemOptionGroup (MenuItemId, OptionGroupId, IsRequired, MinChoices, MaxChoices) VALUES
(@ColdLatteId, @MilkGroupId, 1, 1, 1),
(@ColdLatteId, @SizeGroupId, 1, 1, 1),
(@ColdLatteId, @IceGroupId,  1, 1, 1);

-- Mocha: milk + size + mocha flavor
INSERT INTO MenuItemOptionGroup (MenuItemId, OptionGroupId, IsRequired, MinChoices, MaxChoices) VALUES
(@MochaId,     @MilkGroupId,        1, 1, 1),
(@MochaId,     @SizeGroupId,        1, 1, 1),
(@MochaId,     @MochaFlavorGroupId, 1, 1, 1);

-- Drip Coffee: size
INSERT INTO MenuItemOptionGroup (MenuItemId, OptionGroupId, IsRequired, MinChoices, MaxChoices) VALUES
(@DripCoffeeId, @SizeGroupId, 1, 1, 1);

-- Americano: size
INSERT INTO MenuItemOptionGroup (MenuItemId, OptionGroupId, IsRequired, MinChoices, MaxChoices) VALUES
(@AmericanoId, @SizeGroupId, 1, 1, 1);

-- Espresso: shot count
INSERT INTO MenuItemOptionGroup (MenuItemId, OptionGroupId, IsRequired, MinChoices, MaxChoices) VALUES
(@EspressoId, @ShotCountGroupId, 1, 1, 1);

-- Hot Tea Latte: tea type + size
INSERT INTO MenuItemOptionGroup (MenuItemId, OptionGroupId, IsRequired, MinChoices, MaxChoices) VALUES
(@HotTeaLatteId,  @TeaTypeGroupId, 1, 1, 1),
(@HotTeaLatteId,  @SizeGroupId,    1, 1, 1);

-- Iced Tea Latte: tea type + size + ice
INSERT INTO MenuItemOptionGroup (MenuItemId, OptionGroupId, IsRequired, MinChoices, MaxChoices) VALUES
(@ColdTeaLatteId, @TeaTypeGroupId, 1, 1, 1),
(@ColdTeaLatteId, @SizeGroupId,    1, 1, 1),
(@ColdTeaLatteId, @IceGroupId,     1, 1, 1);

-- Blended drink: size
INSERT INTO MenuItemOptionGroup (MenuItemId, OptionGroupId, IsRequired, MinChoices, MaxChoices) VALUES
(@BlendedDrinkId, @SizeGroupId, 1, 1, 1);

-- Juice: size
INSERT INTO MenuItemOptionGroup (MenuItemId, OptionGroupId, IsRequired, MinChoices, MaxChoices) VALUES
(@JuiceItemId, @SizeGroupId, 1, 1, 1);

-- Water: size + ice
INSERT INTO MenuItemOptionGroup (MenuItemId, OptionGroupId, IsRequired, MinChoices, MaxChoices) VALUES
(@WaterItemId, @SizeGroupId, 1, 1, 1),
(@WaterItemId, @IceGroupId,  1, 1, 1);


-----------------------------------------------------------------------
-- GENERATE ALL COMBINATIONS  (FIXED VERSION)
-----------------------------------------------------------------------

-----------------------
-- 1) HOT LATTE (milk × size)
-----------------------
INSERT INTO Combination (MenuItemId, Code, Price, IsActive)
SELECT
    @HotLatteId,
    'LAT_HOT_' + REPLACE(m.Name,' ','') + '_' + REPLACE(sz.Name,' ',''),
    mi.BasePrice + m.ExtraPrice + sz.ExtraPrice,
    1
FROM MenuItem mi
CROSS JOIN OptionValue m
CROSS JOIN OptionValue sz
WHERE mi.MenuItemId = @HotLatteId
  AND m.OptionGroupId  = @MilkGroupId
  AND sz.OptionGroupId = @SizeGroupId;

-- link milk
INSERT INTO CombinationOption (CombinationId, OptionValueId)
SELECT c.CombinationId, m.OptionValueId
FROM Combination c
JOIN OptionValue m
  ON m.OptionGroupId = @MilkGroupId
WHERE c.MenuItemId = @HotLatteId
  AND c.Code LIKE 'LAT_HOT_' + REPLACE(m.Name,' ','') + '[_]%';

-- link size
INSERT INTO CombinationOption (CombinationId, OptionValueId)
SELECT c.CombinationId, sz.OptionValueId
FROM Combination c
JOIN OptionValue sz
  ON sz.OptionGroupId = @SizeGroupId
WHERE c.MenuItemId = @HotLatteId
  AND c.Code LIKE 'LAT_HOT_%[_]' + REPLACE(sz.Name,' ','');

-----------------------
-- 2) ICED LATTE (milk × size × ice)
-----------------------
INSERT INTO Combination (MenuItemId, Code, Price, IsActive)
SELECT
    @ColdLatteId,
    'LAT_ICE_' + REPLACE(m.Name,' ','') + '_' + REPLACE(sz.Name,' ','')
               + '_' + REPLACE(ice.Name,' ',''),
    mi.BasePrice + m.ExtraPrice + sz.ExtraPrice + ice.ExtraPrice,
    1
FROM MenuItem mi
CROSS JOIN OptionValue m
CROSS JOIN OptionValue sz
CROSS JOIN OptionValue ice
WHERE mi.MenuItemId = @ColdLatteId
  AND m.OptionGroupId   = @MilkGroupId
  AND sz.OptionGroupId  = @SizeGroupId
  AND ice.OptionGroupId = @IceGroupId;

-- link milk
INSERT INTO CombinationOption (CombinationId, OptionValueId)
SELECT c.CombinationId, m.OptionValueId
FROM Combination c
JOIN OptionValue m
  ON m.OptionGroupId = @MilkGroupId
WHERE c.MenuItemId = @ColdLatteId
  AND c.Code LIKE 'LAT_ICE_' + REPLACE(m.Name,' ','') + '[_]%';

-- link size
INSERT INTO CombinationOption (CombinationId, OptionValueId)
SELECT c.CombinationId, sz.OptionValueId
FROM Combination c
JOIN OptionValue sz
  ON sz.OptionGroupId = @SizeGroupId
WHERE c.MenuItemId = @ColdLatteId
  AND c.Code LIKE 'LAT_ICE_%[_]' + REPLACE(sz.Name,' ','') + '[_]%';

-- link ice
INSERT INTO CombinationOption (CombinationId, OptionValueId)
SELECT c.CombinationId, ice.OptionValueId
FROM Combination c
JOIN OptionValue ice
  ON ice.OptionGroupId = @IceGroupId
WHERE c.MenuItemId = @ColdLatteId
  AND c.Code LIKE 'LAT_ICE_%[_]' + REPLACE(ice.Name,' ','');

-----------------------
-- 3) MOCHA (milk × size × flavor)
-----------------------
INSERT INTO Combination (MenuItemId, Code, Price, IsActive)
SELECT
    @MochaId,
    'MOCHA_' + REPLACE(m.Name,' ','') + '_' + REPLACE(sz.Name,' ','')
              + '_' + REPLACE(f.Name,' ',''),
    mi.BasePrice + m.ExtraPrice + sz.ExtraPrice + f.ExtraPrice,
    1
FROM MenuItem mi
CROSS JOIN OptionValue m
CROSS JOIN OptionValue sz
CROSS JOIN OptionValue f
WHERE mi.MenuItemId = @MochaId
  AND m.OptionGroupId  = @MilkGroupId
  AND sz.OptionGroupId = @SizeGroupId
  AND f.OptionGroupId  = @MochaFlavorGroupId;

-- link milk
INSERT INTO CombinationOption (CombinationId, OptionValueId)
SELECT c.CombinationId, m.OptionValueId
FROM Combination c
JOIN OptionValue m
  ON m.OptionGroupId = @MilkGroupId
WHERE c.MenuItemId = @MochaId
  AND c.Code LIKE 'MOCHA_' + REPLACE(m.Name,' ','') + '[_]%';

-- link size
INSERT INTO CombinationOption (CombinationId, OptionValueId)
SELECT c.CombinationId, sz.OptionValueId
FROM Combination c
JOIN OptionValue sz
  ON sz.OptionGroupId = @SizeGroupId
WHERE c.MenuItemId = @MochaId
  AND c.Code LIKE 'MOCHA_%[_]' + REPLACE(sz.Name,' ','') + '[_]%';

-- link flavor
INSERT INTO CombinationOption (CombinationId, OptionValueId)
SELECT c.CombinationId, f.OptionValueId
FROM Combination c
JOIN OptionValue f
  ON f.OptionGroupId = @MochaFlavorGroupId
WHERE c.MenuItemId = @MochaId
  AND c.Code LIKE 'MOCHA_%[_]' + REPLACE(f.Name,' ','');

-----------------------
-- 4) HOT TEA LATTE (tea type × size)
-----------------------
INSERT INTO Combination (MenuItemId, Code, Price, IsActive)
SELECT
    @HotTeaLatteId,
    'TEA_HOT_' + REPLACE(t.Name,' ','') + '_' + REPLACE(sz.Name,' ',''),
    mi.BasePrice + t.ExtraPrice + sz.ExtraPrice,
    1
FROM MenuItem mi
CROSS JOIN OptionValue t
CROSS JOIN OptionValue sz
WHERE mi.MenuItemId = @HotTeaLatteId
  AND t.OptionGroupId  = @TeaTypeGroupId
  AND sz.OptionGroupId = @SizeGroupId;

-- link tea type
INSERT INTO CombinationOption (CombinationId, OptionValueId)
SELECT c.CombinationId, t.OptionValueId
FROM Combination c
JOIN OptionValue t
  ON t.OptionGroupId = @TeaTypeGroupId
WHERE c.MenuItemId = @HotTeaLatteId
  AND c.Code LIKE 'TEA_HOT_' + REPLACE(t.Name,' ','') + '[_]%';

-- link size
INSERT INTO CombinationOption (CombinationId, OptionValueId)
SELECT c.CombinationId, sz.OptionValueId
FROM Combination c
JOIN OptionValue sz
  ON sz.OptionGroupId = @SizeGroupId
WHERE c.MenuItemId = @HotTeaLatteId
  AND c.Code LIKE 'TEA_HOT_%[_]' + REPLACE(sz.Name,' ','');

-----------------------
-- 5) ICED TEA LATTE (tea type × size × ice)
-----------------------
INSERT INTO Combination (MenuItemId, Code, Price, IsActive)
SELECT
    @ColdTeaLatteId,
    'TEA_ICE_' + REPLACE(t.Name,' ','') + '_' + REPLACE(sz.Name,' ','')
               + '_' + REPLACE(ice.Name,' ',''),
    mi.BasePrice + t.ExtraPrice + sz.ExtraPrice + ice.ExtraPrice,
    1
FROM MenuItem mi
CROSS JOIN OptionValue t
CROSS JOIN OptionValue sz
CROSS JOIN OptionValue ice
WHERE mi.MenuItemId = @ColdTeaLatteId
  AND t.OptionGroupId   = @TeaTypeGroupId
  AND sz.OptionGroupId  = @SizeGroupId
  AND ice.OptionGroupId = @IceGroupId;

-- link tea type
INSERT INTO CombinationOption (CombinationId, OptionValueId)
SELECT c.CombinationId, t.OptionValueId
FROM Combination c
JOIN OptionValue t
  ON t.OptionGroupId = @TeaTypeGroupId
WHERE c.MenuItemId = @ColdTeaLatteId
  AND c.Code LIKE 'TEA_ICE_' + REPLACE(t.Name,' ','') + '[_]%';

-- link size
INSERT INTO CombinationOption (CombinationId, OptionValueId)
SELECT c.CombinationId, sz.OptionValueId
FROM Combination c
JOIN OptionValue sz
  ON sz.OptionGroupId = @SizeGroupId
WHERE c.MenuItemId = @ColdTeaLatteId
  AND c.Code LIKE 'TEA_ICE_%[_]' + REPLACE(sz.Name,' ','') + '[_]%';

-- link ice
INSERT INTO CombinationOption (CombinationId, OptionValueId)
SELECT c.CombinationId, ice.OptionValueId
FROM Combination c
JOIN OptionValue ice
  ON ice.OptionGroupId = @IceGroupId
WHERE c.MenuItemId = @ColdTeaLatteId
  AND c.Code LIKE 'TEA_ICE_%[_]' + REPLACE(ice.Name,' ','');

-----------------------
-- 6) ESPRESSO (shot count)
-----------------------
INSERT INTO Combination (MenuItemId, Code, Price, IsActive)
SELECT
    @EspressoId,
    'ESP_' + REPLACE(s.Name,' ',''),
    mi.BasePrice + s.ExtraPrice,
    1
FROM MenuItem mi
JOIN OptionValue s
  ON s.OptionGroupId = @ShotCountGroupId
WHERE mi.MenuItemId = @EspressoId;

INSERT INTO CombinationOption (CombinationId, OptionValueId)
SELECT c.CombinationId, s.OptionValueId
FROM Combination c
JOIN OptionValue s
  ON s.OptionGroupId = @ShotCountGroupId
WHERE c.MenuItemId = @EspressoId
  AND c.Code = 'ESP_' + REPLACE(s.Name,' ','');

-----------------------
-- 7) AMERICANO (size)
-----------------------
INSERT INTO Combination (MenuItemId, Code, Price, IsActive)
SELECT
    @AmericanoId,
    'AMER_' + REPLACE(sz.Name,' ',''),
    mi.BasePrice + sz.ExtraPrice,
    1
FROM MenuItem mi
JOIN OptionValue sz
  ON sz.OptionGroupId = @SizeGroupId
WHERE mi.MenuItemId = @AmericanoId;

INSERT INTO CombinationOption (CombinationId, OptionValueId)
SELECT c.CombinationId, sz.OptionValueId
FROM Combination c
JOIN OptionValue sz
  ON sz.OptionGroupId = @SizeGroupId
WHERE c.MenuItemId = @AmericanoId
  AND c.Code = 'AMER_' + REPLACE(sz.Name,' ','');

-----------------------
-- 8) DRIP COFFEE (size)
-----------------------
INSERT INTO Combination (MenuItemId, Code, Price, IsActive)
SELECT
    @DripCoffeeId,
    'DRIP_' + REPLACE(sz.Name,' ',''),
    mi.BasePrice + sz.ExtraPrice,
    1
FROM MenuItem mi
JOIN OptionValue sz
  ON sz.OptionGroupId = @SizeGroupId
WHERE mi.MenuItemId = @DripCoffeeId;

INSERT INTO CombinationOption (CombinationId, OptionValueId)
SELECT c.CombinationId, sz.OptionValueId
FROM Combination c
JOIN OptionValue sz
  ON sz.OptionGroupId = @SizeGroupId
WHERE c.MenuItemId = @DripCoffeeId
  AND c.Code = 'DRIP_' + REPLACE(sz.Name,' ','');

-----------------------
-- 9) BLENDED DRINK (size)
-----------------------
INSERT INTO Combination (MenuItemId, Code, Price, IsActive)
SELECT
    @BlendedDrinkId,
    'BLEND_' + REPLACE(sz.Name,' ',''),
    mi.BasePrice + sz.ExtraPrice,
    1
FROM MenuItem mi
JOIN OptionValue sz
  ON sz.OptionGroupId = @SizeGroupId
WHERE mi.MenuItemId = @BlendedDrinkId;

INSERT INTO CombinationOption (CombinationId, OptionValueId)
SELECT c.CombinationId, sz.OptionValueId
FROM Combination c
JOIN OptionValue sz
  ON sz.OptionGroupId = @SizeGroupId
WHERE c.MenuItemId = @BlendedDrinkId
  AND c.Code = 'BLEND_' + REPLACE(sz.Name,' ','');

-----------------------
-- 10) JUICE (size)
-----------------------
INSERT INTO Combination (MenuItemId, Code, Price, IsActive)
SELECT
    @JuiceItemId,
    'JUICE_' + REPLACE(sz.Name,' ',''),
    mi.BasePrice + sz.ExtraPrice,
    1
FROM MenuItem mi
JOIN OptionValue sz
  ON sz.OptionGroupId = @SizeGroupId
WHERE mi.MenuItemId = @JuiceItemId;

INSERT INTO CombinationOption (CombinationId, OptionValueId)
SELECT c.CombinationId, sz.OptionValueId
FROM Combination c
JOIN OptionValue sz
  ON sz.OptionGroupId = @SizeGroupId
WHERE c.MenuItemId = @JuiceItemId
  AND c.Code = 'JUICE_' + REPLACE(sz.Name,' ','');

-----------------------
-- 11) WATER (size × ice)
-----------------------
INSERT INTO Combination (MenuItemId, Code, Price, IsActive)
SELECT
    @WaterItemId,
    'WTR_' + REPLACE(sz.Name,' ','') + '_' + REPLACE(ice.Name,' ',''),
    mi.BasePrice + sz.ExtraPrice + ice.ExtraPrice,
    1
FROM MenuItem mi
CROSS JOIN OptionValue sz
CROSS JOIN OptionValue ice
WHERE mi.MenuItemId = @WaterItemId
  AND sz.OptionGroupId  = @SizeGroupId
  AND ice.OptionGroupId = @IceGroupId;

-- link size
INSERT INTO CombinationOption (CombinationId, OptionValueId)
SELECT c.CombinationId, sz.OptionValueId
FROM Combination c
JOIN OptionValue sz
  ON sz.OptionGroupId = @SizeGroupId
WHERE c.MenuItemId = @WaterItemId
  AND c.Code LIKE 'WTR_%[_]' + REPLACE(sz.Name,' ','') + '%';

-- link ice
INSERT INTO CombinationOption (CombinationId, OptionValueId)
SELECT c.CombinationId, ice.OptionValueId
FROM Combination c
JOIN OptionValue ice
  ON ice.OptionGroupId = @IceGroupId
WHERE c.MenuItemId = @WaterItemId
  AND c.Code LIKE 'WTR_%[_]' + REPLACE(ice.Name,' ','');

-- =====================================================================
-- END OF SCRIPT
-- =====================================================================
