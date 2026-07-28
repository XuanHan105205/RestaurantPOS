-- RESTAURANT POS - FULL DATABASE SCRIPT
-- Chay toan bo file nay mot lan trong SQL Server Management Studio.
-- Thu tu: tao database va bang -> tao view bao cao -> them du lieu kho mau.

-- =======================================================
-- Ká»ŠCH Báº¢N Táº O CÆ  Sá»ž Dá»® LIá»†U Dá»° ÃN RESTAURANT POS
-- =======================================================

-- 1. Táº¡o CÆ¡ Sá»Ÿ Dá»¯ Liá»‡u
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'RestaurantPOS')
BEGIN
    CREATE DATABASE RestaurantPOS;
END
GO

USE RestaurantPOS;
GO

-- XÃ³a view bÃ¡o cÃ¡o trÆ°á»›c vÃ¬ cÃ¡c view Ä‘ang tham chiáº¿u Ä‘áº¿n báº£ng dá»¯ liá»‡u.
-- Nhá» váº­y script cÃ³ thá»ƒ cháº¡y láº¡i nhiá»u láº§n mÃ  khÃ´ng bá»‹ lá»—i.
IF OBJECT_ID('vw_DailyPaymentBreakdown', 'V') IS NOT NULL DROP VIEW vw_DailyPaymentBreakdown;
IF OBJECT_ID('vw_DailyBestSellingDishes', 'V') IS NOT NULL DROP VIEW vw_DailyBestSellingDishes;
IF OBJECT_ID('vw_DailySalesSummary', 'V') IS NOT NULL DROP VIEW vw_DailySalesSummary;
GO

-- XÃ³a cÃ¡c báº£ng cÅ© theo thá»© tá»± tá»« báº£ng con Ä‘áº¿n báº£ng cha.
IF OBJECT_ID('payment_details', 'U') IS NOT NULL DROP TABLE payment_details;
IF OBJECT_ID('invoices', 'U') IS NOT NULL DROP TABLE invoices;
IF OBJECT_ID('stock_receipts', 'U') IS NOT NULL DROP TABLE stock_receipts;
IF OBJECT_ID('recipes', 'U') IS NOT NULL DROP TABLE recipes;
IF OBJECT_ID('ingredients', 'U') IS NOT NULL DROP TABLE ingredients;
IF OBJECT_ID('order_items', 'U') IS NOT NULL DROP TABLE order_items;
IF OBJECT_ID('orders', 'U') IS NOT NULL DROP TABLE orders;
IF OBJECT_ID('dishes', 'U') IS NOT NULL DROP TABLE dishes;
IF OBJECT_ID('categories', 'U') IS NOT NULL DROP TABLE categories;
IF OBJECT_ID('table_sessions', 'U') IS NOT NULL DROP TABLE table_sessions;
IF OBJECT_ID('dining_sessions', 'U') IS NOT NULL DROP TABLE dining_sessions;
IF OBJECT_ID('restaurant_tables', 'U') IS NOT NULL DROP TABLE restaurant_tables;
IF OBJECT_ID('customers', 'U') IS NOT NULL DROP TABLE customers;
IF OBJECT_ID('employees', 'U') IS NOT NULL DROP TABLE employees;
GO

-- 2. Táº¡o Báº£ng employees
CREATE TABLE employees (
    employee_id INT IDENTITY(1,1) PRIMARY KEY,
    full_name NVARCHAR(100) NOT NULL,
    username VARCHAR(50) NOT NULL UNIQUE,
    password_hash VARCHAR(256) NOT NULL,
    role VARCHAR(20) NOT NULL CHECK (role IN ('waiter', 'kitchen', 'cashier', 'manager')),
    phone VARCHAR(15),
    is_active BIT NOT NULL DEFAULT 1
);

-- 3. Táº¡o Báº£ng customers
CREATE TABLE customers (
    customer_id INT IDENTITY(1,1) PRIMARY KEY,
    full_name NVARCHAR(100) NOT NULL,
    phone VARCHAR(15) UNIQUE,
    membership_tier VARCHAR(20) NOT NULL DEFAULT 'regular' CHECK (membership_tier IN ('regular', 'vip', 'vip_gold')),
    loyalty_points INT NOT NULL DEFAULT 0
);

-- 4. Táº¡o Báº£ng restaurant_tables
CREATE TABLE restaurant_tables (
    table_id INT IDENTITY(1,1) PRIMARY KEY,
    table_name NVARCHAR(50) NOT NULL UNIQUE,
    capacity INT DEFAULT 4,
    status VARCHAR(20) NOT NULL DEFAULT 'available' CHECK (status IN ('available', 'occupied', 'needs_cleaning', 'reserved')),
    area NVARCHAR(50)
);

-- 5. Táº¡o Báº£ng dining_sessions
CREATE TABLE dining_sessions (
    session_id INT IDENTITY(1,1) PRIMARY KEY,
    opened_at DATETIME NOT NULL DEFAULT GETDATE(),
    closed_at DATETIME,
    opened_by_employee_id INT NOT NULL FOREIGN KEY REFERENCES employees(employee_id),
    customer_id INT FOREIGN KEY REFERENCES customers(customer_id),
    status VARCHAR(20) NOT NULL DEFAULT 'open' CHECK (status IN ('open', 'closed'))
);

-- 6. Táº¡o Báº£ng table_sessions
CREATE TABLE table_sessions (
    table_id INT NOT NULL FOREIGN KEY REFERENCES restaurant_tables(table_id),
    session_id INT NOT NULL FOREIGN KEY REFERENCES dining_sessions(session_id),
    PRIMARY KEY (table_id, session_id)
);

-- 7. Táº¡o Báº£ng categories
CREATE TABLE categories (
    category_id INT IDENTITY(1,1) PRIMARY KEY,
    category_name NVARCHAR(100) NOT NULL UNIQUE
);

-- 8. Táº¡o Báº£ng dishes
CREATE TABLE dishes (
    dish_id INT IDENTITY(1,1) PRIMARY KEY,
    dish_name NVARCHAR(150) NOT NULL,
    price DECIMAL(10,2) NOT NULL,
    category_id INT FOREIGN KEY REFERENCES categories(category_id),
    availability_status VARCHAR(20) NOT NULL DEFAULT 'active' CHECK (availability_status IN ('active', 'discontinued')),
    image_url NVARCHAR(255)
);

-- 9. Táº¡o Báº£ng orders
CREATE TABLE orders (
    order_id INT IDENTITY(1,1) PRIMARY KEY,
    session_id INT NOT NULL FOREIGN KEY REFERENCES dining_sessions(session_id),
    created_by_employee_id INT NOT NULL FOREIGN KEY REFERENCES employees(employee_id),
    ordered_at DATETIME NOT NULL DEFAULT GETDATE()
);

-- 10. Táº¡o Báº£ng order_items
CREATE TABLE order_items (
    order_item_id INT IDENTITY(1,1) PRIMARY KEY,
    order_id INT NOT NULL FOREIGN KEY REFERENCES orders(order_id),
    dish_id INT NOT NULL FOREIGN KEY REFERENCES dishes(dish_id),
    quantity INT NOT NULL CHECK (quantity > 0),
    unit_price DECIMAL(10,2) NOT NULL,
    status VARCHAR(20) NOT NULL DEFAULT 'pending' CHECK (status IN ('pending', 'cooking', 'ready', 'served', 'cancelled')),
    note NVARCHAR(255),
    status_updated_at DATETIME
);

-- 11. Táº¡o Báº£ng ingredients
CREATE TABLE ingredients (
    ingredient_id INT IDENTITY(1,1) PRIMARY KEY,
    ingredient_name NVARCHAR(100) NOT NULL UNIQUE,
    unit NVARCHAR(20) NOT NULL,
    stock_quantity DECIMAL(10,2) NOT NULL DEFAULT 0,
    min_stock_alert DECIMAL(10,2)
);

-- 12. Táº¡o Báº£ng recipes
CREATE TABLE recipes (
    dish_id INT NOT NULL FOREIGN KEY REFERENCES dishes(dish_id),
    ingredient_id INT NOT NULL FOREIGN KEY REFERENCES ingredients(ingredient_id),
    quantity_per_serving DECIMAL(10,2) NOT NULL,
    PRIMARY KEY (dish_id, ingredient_id)
);

-- 13. Táº¡o Báº£ng stock_receipts
CREATE TABLE stock_receipts (
    receipt_id INT IDENTITY(1,1) PRIMARY KEY,
    ingredient_id INT NOT NULL FOREIGN KEY REFERENCES ingredients(ingredient_id),
    quantity DECIMAL(10,2) NOT NULL CHECK (quantity > 0),
    unit_cost DECIMAL(10,2),
    received_at DATETIME NOT NULL DEFAULT GETDATE(),
    received_by_employee_id INT FOREIGN KEY REFERENCES employees(employee_id),
    supplier NVARCHAR(150)
);

-- 14. Táº¡o Báº£ng invoices
CREATE TABLE invoices (
    invoice_id INT IDENTITY(1,1) PRIMARY KEY,
    session_id INT NOT NULL FOREIGN KEY REFERENCES dining_sessions(session_id),
    subtotal DECIMAL(10,2) NOT NULL,
    discount DECIMAL(10,2) NOT NULL DEFAULT 0,
    total_amount DECIMAL(10,2) NOT NULL,
    paid_at DATETIME NOT NULL DEFAULT GETDATE(),
    cashier_employee_id INT FOREIGN KEY REFERENCES employees(employee_id)
);

-- 15. Táº¡o Báº£ng payment_details
CREATE TABLE payment_details (
    payment_id INT IDENTITY(1,1) PRIMARY KEY,
    invoice_id INT NOT NULL FOREIGN KEY REFERENCES invoices(invoice_id),
    method VARCHAR(20) NOT NULL CHECK (method IN ('cash', 'bank_transfer', 'card')),
    amount DECIMAL(10,2) NOT NULL
);
GO

-- =======================================================
-- CHÃˆN Dá»® LIá»†U Äá»‚ KIá»‚M THá»¬ BAN Äáº¦U (SEED DATA)
-- =======================================================

-- NhÃ¢n viÃªn máº«u
INSERT INTO employees (full_name, username, password_hash, role, phone, is_active) VALUES
(N'Manager', 'manager', '123456', 'manager', '0912345678', 1),
(N'Waiter', 'waiter', '123456', 'waiter', '0922345678', 1),
(N'Kitchen', 'kitchen', '123456', 'kitchen', '0932345678', 1),
(N'Cashier', 'cashier', '123456', 'cashier', '0942345678', 1);

-- Danh má»¥c mÃ³n Äƒn
INSERT INTO categories (category_name) VALUES
(N'Khai vá»‹'),
(N'MÃ³n chÃ­nh'),
(N'Láº©u'),
(N'Äá»“ uá»‘ng'),
(N'TrÃ¡ng miá»‡ng');

-- CÃ¡c mÃ³n Äƒn máº«u
INSERT INTO dishes (dish_name, price, category_id, availability_status) VALUES
(N'Gá»i ngÃ³ sen tÃ´m thá»‹t', 95000, 1, 'active'),
(N'CÆ¡m chiÃªn háº£i sáº£n', 120000, 2, 'active'),
(N'BÃ² lÃºc láº¯c khoai tÃ¢y chiÃªn', 150000, 2, 'active'),
(N'Láº©u thÃ¡i chua cay', 250000, 3, 'active'),
(N'TrÃ  Ä‘Ã o cam sáº£', 35000, 4, 'active'),
(N'NÆ°á»›c ngá»t lon', 20000, 4, 'active'),
(N'Rau cÃ¢u trÃ¡i dá»«a', 40000, 5, 'active');

-- BÃ n Äƒn máº«u
INSERT INTO restaurant_tables (table_name, capacity, status, area) VALUES
(N'BÃ n 1 (Trá»‡t)', 4, 'available', N'Táº§ng trá»‡t'),
(N'BÃ n 2 (Trá»‡t)', 4, 'available', N'Táº§ng trá»‡t'),
(N'BÃ n 3 (Trá»‡t)', 6, 'available', N'Táº§ng trá»‡t'),
(N'BÃ n 4 (Láº§u 1)', 2, 'available', N'Táº§ng 1'),
(N'BÃ n 5 (Láº§u 1)', 8, 'available', N'Táº§ng 1');

-- KhÃ¡ch hÃ ng máº«u
INSERT INTO customers (full_name, phone, membership_tier, loyalty_points) VALUES
(N'KhÃ¡ch vÃ£ng lai', '0000000000', 'regular', 0),
(N'Nguyá»…n Thá»‹ Hoa', '0909090909', 'vip', 150);
GO


-- =======================================================
-- PHAN TIEP THEO
-- =======================================================

USE RestaurantPOS;
GO

-- 1. vw_DailySalesSummary
IF OBJECT_ID('vw_DailySalesSummary', 'V') IS NOT NULL DROP VIEW vw_DailySalesSummary;
GO
CREATE VIEW vw_DailySalesSummary AS
SELECT 
    CAST(paid_at AS DATE) AS SaleDate,
    COUNT(invoice_id) AS TotalInvoices,
    SUM(subtotal) AS TotalSubtotal,
    SUM(discount) AS TotalDiscount,
    SUM(total_amount) AS TotalRevenue
FROM invoices
GROUP BY CAST(paid_at AS DATE);
GO

-- 2. vw_DailyBestSellingDishes
IF OBJECT_ID('vw_DailyBestSellingDishes', 'V') IS NOT NULL DROP VIEW vw_DailyBestSellingDishes;
GO
CREATE VIEW vw_DailyBestSellingDishes AS
SELECT 
    CAST(i.paid_at AS DATE) AS SaleDate,
    d.dish_id AS DishId,
    d.dish_name AS DishName,
    SUM(oi.quantity) AS TotalQuantity,
    SUM(oi.quantity * oi.unit_price) AS TotalRevenue
FROM invoices i
JOIN dining_sessions ds ON i.session_id = ds.session_id
JOIN orders o ON ds.session_id = o.session_id
JOIN order_items oi ON o.order_id = oi.order_id
JOIN dishes d ON oi.dish_id = d.dish_id
WHERE oi.status IN ('ready', 'served')
GROUP BY CAST(i.paid_at AS DATE), d.dish_id, d.dish_name;
GO

-- 3. vw_DailyPaymentBreakdown
IF OBJECT_ID('vw_DailyPaymentBreakdown', 'V') IS NOT NULL DROP VIEW vw_DailyPaymentBreakdown;
GO
CREATE VIEW vw_DailyPaymentBreakdown AS
SELECT 
    CAST(i.paid_at AS DATE) AS SaleDate,
    pd.method AS PaymentMethod,
    SUM(pd.amount) AS TotalAmount,
    COUNT(pd.payment_id) AS TransactionCount
FROM payment_details pd
JOIN invoices i ON pd.invoice_id = i.invoice_id
GROUP BY CAST(i.paid_at AS DATE), pd.method;
GO


-- =======================================================
-- PHAN TIEP THEO
-- =======================================================

-- =======================================================
-- CHÃˆN Dá»® LIá»†U MáºªU CHO Há»† THá»NG KHO & NGUYÃŠN LIá»†U
-- Cháº¡y script nÃ y sau khi Ä‘Ã£ cháº¡y database_schema.sql
-- =======================================================

USE RestaurantPOS;
GO

-- 1. ThÃªm nguyÃªn liá»‡u máº«u
INSERT INTO ingredients (ingredient_name, unit, stock_quantity, min_stock_alert) VALUES
(N'Thá»‹t bÃ² Má»¹', N'kg', 0, 5),
(N'TÃ´m sÃº', N'kg', 0, 3),
(N'Khoai tÃ¢y', N'kg', 0, 10),
(N'NgÃ³ sen', N'kg', 0, 2),
(N'Rau xÃ  lÃ¡ch', N'kg', 0, 5),
(N'Gáº¡o', N'kg', 0, 20),
(N'NÆ°á»›c máº¯m', N'chai', 0, 5),
(N'Dáº§u Äƒn', N'lÃ­t', 0, 10),
(N'HÃ nh tÃ­m', N'kg', 0, 3),
(N'Tá»i', N'kg', 0, 2),
(N'á»št tÆ°Æ¡i', N'kg', 0, 1),
(N'Chanh', N'kg', 0, 2),
(N'Náº¥m kim chÃ¢m', N'gÃ³i', 0, 10),
(N'BÃºn tÆ°Æ¡i', N'kg', 0, 5),
(N'NÆ°á»›c cá»‘t dá»«a', N'há»™p', 0, 5),
(N'TrÃ  Ã” Long', N'gÃ³i', 0, 10),
(N'ÄÃ o ngÃ¢m', N'há»™p', 0, 5),
(N'Cam tÆ°Æ¡i', N'kg', 0, 5),
(N'Sáº£', N'bÃ³', 0, 5),
(N'Bá»™t rau cÃ¢u', N'gÃ³i', 0, 10);
GO

-- 2. Nháº­p kho nguyÃªn liá»‡u (Táº¡o phiáº¿u nháº­p hÃ ng máº«u)
-- Nháº­p kho Thá»‹t bÃ² Má»¹
INSERT INTO stock_receipts (ingredient_id, quantity, unit_cost, received_by_employee_id, supplier) VALUES
(1, 15, 220000, 1, N'Thá»±c pháº©m sáº¡ch Metro'),
(2, 10, 180000, 1, N'Háº£i sáº£n tÆ°Æ¡i sá»‘ng Biá»ƒn ÄÃ´ng'),
(3, 20, 15000, 1, N'Chá»£ Ä‘áº§u má»‘i BÃ¬nh Äiá»n'),
(4, 5, 35000, 1, N'Chá»£ Ä‘áº§u má»‘i BÃ¬nh Äiá»n'),
(5, 8, 25000, 1, N'NÃ´ng tráº¡i rau sáº¡ch ÄÃ  Láº¡t'),
(6, 50, 18000, 1, N'Gáº¡o ST25 SÃ³c TrÄƒng'),
(7, 10, 22000, 1, N'SiÃªu thá»‹ BÃ¡ch HÃ³a Xanh'),
(8, 15, 35000, 1, N'Dáº§u TÆ°á»ng An'),
(9, 5, 30000, 1, N'Chá»£ Ä‘áº§u má»‘i BÃ¬nh Äiá»n'),
(10, 3, 45000, 1, N'Chá»£ Ä‘áº§u má»‘i BÃ¬nh Äiá»n'),
(11, 2, 40000, 1, N'Chá»£ Ä‘áº§u má»‘i BÃ¬nh Äiá»n'),
(12, 3, 20000, 1, N'Chá»£ Ä‘áº§u má»‘i BÃ¬nh Äiá»n'),
(13, 20, 12000, 1, N'SiÃªu thá»‹ BÃ¡ch HÃ³a Xanh'),
(14, 10, 15000, 1, N'Chá»£ Ä‘áº§u má»‘i BÃ¬nh Äiá»n'),
(15, 10, 18000, 1, N'SiÃªu thá»‹ BÃ¡ch HÃ³a Xanh'),
(16, 20, 25000, 1, N'TrÃ  ThÃ¡i NguyÃªn'),
(17, 10, 35000, 1, N'SiÃªu thá»‹ BÃ¡ch HÃ³a Xanh'),
(18, 8, 25000, 1, N'Chá»£ Ä‘áº§u má»‘i BÃ¬nh Äiá»n'),
(19, 10, 8000, 1, N'Chá»£ Ä‘áº§u má»‘i BÃ¬nh Äiá»n'),
(20, 15, 15000, 1, N'SiÃªu thá»‹ BÃ¡ch HÃ³a Xanh');
GO

-- Cáº­p nháº­t tá»“n kho theo phiáº¿u nháº­p (mÃ´ phá»ng há»‡ thá»‘ng tá»± Ä‘á»™ng cá»™ng dá»“n)
UPDATE ingredients SET stock_quantity = 15 WHERE ingredient_id = 1;   -- Thá»‹t bÃ² Má»¹
UPDATE ingredients SET stock_quantity = 10 WHERE ingredient_id = 2;   -- TÃ´m sÃº
UPDATE ingredients SET stock_quantity = 20 WHERE ingredient_id = 3;   -- Khoai tÃ¢y
UPDATE ingredients SET stock_quantity = 5  WHERE ingredient_id = 4;   -- NgÃ³ sen
UPDATE ingredients SET stock_quantity = 8  WHERE ingredient_id = 5;   -- Rau xÃ  lÃ¡ch
UPDATE ingredients SET stock_quantity = 50 WHERE ingredient_id = 6;   -- Gáº¡o
UPDATE ingredients SET stock_quantity = 10 WHERE ingredient_id = 7;   -- NÆ°á»›c máº¯m
UPDATE ingredients SET stock_quantity = 15 WHERE ingredient_id = 8;   -- Dáº§u Äƒn
UPDATE ingredients SET stock_quantity = 5  WHERE ingredient_id = 9;   -- HÃ nh tÃ­m
UPDATE ingredients SET stock_quantity = 3  WHERE ingredient_id = 10;  -- Tá»i
UPDATE ingredients SET stock_quantity = 2  WHERE ingredient_id = 11;  -- á»št tÆ°Æ¡i
UPDATE ingredients SET stock_quantity = 3  WHERE ingredient_id = 12;  -- Chanh
UPDATE ingredients SET stock_quantity = 20 WHERE ingredient_id = 13;  -- Náº¥m kim chÃ¢m
UPDATE ingredients SET stock_quantity = 10 WHERE ingredient_id = 14;  -- BÃºn tÆ°Æ¡i
UPDATE ingredients SET stock_quantity = 10 WHERE ingredient_id = 15;  -- NÆ°á»›c cá»‘t dá»«a
UPDATE ingredients SET stock_quantity = 20 WHERE ingredient_id = 16;  -- TrÃ  Ã” Long
UPDATE ingredients SET stock_quantity = 10 WHERE ingredient_id = 17;  -- ÄÃ o ngÃ¢m
UPDATE ingredients SET stock_quantity = 8  WHERE ingredient_id = 18;  -- Cam tÆ°Æ¡i
UPDATE ingredients SET stock_quantity = 10 WHERE ingredient_id = 19;  -- Sáº£
UPDATE ingredients SET stock_quantity = 15 WHERE ingredient_id = 20;  -- Bá»™t rau cÃ¢u
GO

-- 3. Táº¡o cÃ´ng thá»©c Ä‘á»‹nh lÆ°á»£ng máº«u cho cÃ¡c mÃ³n Äƒn
-- MÃ³n 1: Gá»i ngÃ³ sen tÃ´m thá»‹t (dish_id = 1)
INSERT INTO recipes (dish_id, ingredient_id, quantity_per_serving) VALUES
(1, 4, 0.10),   -- NgÃ³ sen 100g
(1, 2, 0.08),   -- TÃ´m sÃº 80g
(1, 12, 0.05),  -- Chanh 50g
(1, 11, 0.02),  -- á»št tÆ°Æ¡i 20g
(1, 7, 0.02);   -- NÆ°á»›c máº¯m 20ml

-- MÃ³n 2: CÆ¡m chiÃªn háº£i sáº£n (dish_id = 2)
INSERT INTO recipes (dish_id, ingredient_id, quantity_per_serving) VALUES
(2, 6, 0.20),   -- Gáº¡o 200g
(2, 2, 0.10),   -- TÃ´m sÃº 100g
(2, 8, 0.03),   -- Dáº§u Äƒn 30ml
(2, 9, 0.02),   -- HÃ nh tÃ­m 20g
(2, 10, 0.01),  -- Tá»i 10g
(2, 7, 0.02);   -- NÆ°á»›c máº¯m 20ml

-- MÃ³n 3: BÃ² lÃºc láº¯c khoai tÃ¢y chiÃªn (dish_id = 3)
INSERT INTO recipes (dish_id, ingredient_id, quantity_per_serving) VALUES
(3, 1, 0.20),   -- Thá»‹t bÃ² Má»¹ 200g
(3, 3, 0.15),   -- Khoai tÃ¢y 150g
(3, 8, 0.05),   -- Dáº§u Äƒn 50ml
(3, 10, 0.02),  -- Tá»i 20g
(3, 5, 0.05);   -- Rau xÃ  lÃ¡ch 50g

-- MÃ³n 4: Láº©u thÃ¡i chua cay (dish_id = 4)
INSERT INTO recipes (dish_id, ingredient_id, quantity_per_serving) VALUES
(4, 2, 0.15),   -- TÃ´m sÃº 150g
(4, 13, 1.00),  -- Náº¥m kim chÃ¢m 1 gÃ³i
(4, 14, 0.20),  -- BÃºn tÆ°Æ¡i 200g
(4, 11, 0.03),  -- á»št tÆ°Æ¡i 30g
(4, 19, 0.50),  -- Sáº£ 0.5 bÃ³
(4, 12, 0.05),  -- Chanh 50g
(4, 7, 0.03);   -- NÆ°á»›c máº¯m 30ml

-- MÃ³n 5: TrÃ  Ä‘Ã o cam sáº£ (dish_id = 5)
INSERT INTO recipes (dish_id, ingredient_id, quantity_per_serving) VALUES
(5, 16, 0.50),  -- TrÃ  Ã” Long 0.5 gÃ³i
(5, 17, 0.30),  -- ÄÃ o ngÃ¢m 0.3 há»™p
(5, 18, 0.10),  -- Cam tÆ°Æ¡i 100g
(5, 19, 0.20);  -- Sáº£ 0.2 bÃ³

-- MÃ³n 7: Rau cÃ¢u trÃ¡i dá»«a (dish_id = 7)
INSERT INTO recipes (dish_id, ingredient_id, quantity_per_serving) VALUES
(7, 20, 0.50),  -- Bá»™t rau cÃ¢u 0.5 gÃ³i
(7, 15, 0.30);  -- NÆ°á»›c cá»‘t dá»«a 0.3 há»™p
GO

PRINT N'âœ… ÄÃ£ chÃ¨n dá»¯ liá»‡u máº«u thÃ nh cÃ´ng cho NguyÃªn liá»‡u, Phiáº¿u nháº­p kho vÃ  CÃ´ng thá»©c Ä‘á»‹nh lÆ°á»£ng!';
GO
