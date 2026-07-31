-- RESTAURANT POS - SCRIPT DATABASE ĐẦY ĐỦ
-- Chạy toàn bộ file này một lần trong SQL Server Management Studio.
-- Thứ tự: tạo database và bảng -> tạo view báo cáo -> thêm dữ liệu kho mẫu.

-- =======================================================
-- KỊCH BẢN TẠO CƠ SỞ DỮ LIỆU DỰ ÁN RESTAURANT POS
-- =======================================================

-- 1. Tạo Cơ Sở Dữ Liệu
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'RestaurantPOS')
BEGIN
    CREATE DATABASE RestaurantPOS;
END
GO

USE RestaurantPOS;
GO

-- Xóa view báo cáo trước vì các view đang tham chiếu đến bảng dữ liệu.
-- Nhờ vậy script có thể chạy lại nhiều lần mà không bị lỗi.
IF OBJECT_ID('vw_DailyPaymentBreakdown', 'V') IS NOT NULL DROP VIEW vw_DailyPaymentBreakdown;
IF OBJECT_ID('vw_DailyBestSellingDishes', 'V') IS NOT NULL DROP VIEW vw_DailyBestSellingDishes;
IF OBJECT_ID('vw_DailySalesSummary', 'V') IS NOT NULL DROP VIEW vw_DailySalesSummary;
GO

-- Xóa các bảng cũ theo thứ tự từ bảng con đến bảng cha.
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

-- 2. Tạo Bảng employees
CREATE TABLE employees (
    employee_id INT IDENTITY(1,1) PRIMARY KEY,
    full_name NVARCHAR(100) NOT NULL,
    username VARCHAR(50) NOT NULL UNIQUE,
    password_hash VARCHAR(256) NOT NULL,
    role VARCHAR(20) NOT NULL CHECK (role IN ('waiter', 'kitchen', 'cashier', 'manager')),
    phone VARCHAR(15),
    is_active BIT NOT NULL DEFAULT 1
);

-- 3. Tạo Bảng customers
CREATE TABLE customers (
    customer_id INT IDENTITY(1,1) PRIMARY KEY,
    full_name NVARCHAR(100) NOT NULL,
    phone VARCHAR(15) UNIQUE,
    membership_tier VARCHAR(20) NOT NULL DEFAULT 'regular' CHECK (membership_tier IN ('regular', 'vip', 'vip_gold')),
    loyalty_points INT NOT NULL DEFAULT 0
);

-- 4. Tạo Bảng restaurant_tables
CREATE TABLE restaurant_tables (
    table_id INT IDENTITY(1,1) PRIMARY KEY,
    table_name NVARCHAR(50) NOT NULL UNIQUE,
    capacity INT DEFAULT 4,
    status VARCHAR(20) NOT NULL DEFAULT 'available' CHECK (status IN ('available', 'occupied', 'needs_cleaning', 'reserved')),
    area NVARCHAR(50)
);

-- 5. Tạo Bảng dining_sessions
CREATE TABLE dining_sessions (
    session_id INT IDENTITY(1,1) PRIMARY KEY,
    opened_at DATETIME NOT NULL DEFAULT GETDATE(),
    closed_at DATETIME,
    opened_by_employee_id INT NOT NULL FOREIGN KEY REFERENCES employees(employee_id),
    customer_id INT FOREIGN KEY REFERENCES customers(customer_id),
    status VARCHAR(20) NOT NULL DEFAULT 'open' CHECK (status IN ('open', 'closed'))
);

-- 6. Tạo Bảng table_sessions
CREATE TABLE table_sessions (
    table_id INT NOT NULL FOREIGN KEY REFERENCES restaurant_tables(table_id),
    session_id INT NOT NULL FOREIGN KEY REFERENCES dining_sessions(session_id),
    PRIMARY KEY (table_id, session_id)
);

-- 7. Tạo Bảng categories
CREATE TABLE categories (
    category_id INT IDENTITY(1,1) PRIMARY KEY,
    category_name NVARCHAR(100) NOT NULL UNIQUE
);

-- 8. Tạo Bảng dishes
CREATE TABLE dishes (
    dish_id INT IDENTITY(1,1) PRIMARY KEY,
    dish_name NVARCHAR(150) NOT NULL,
    price DECIMAL(10,2) NOT NULL,
    category_id INT FOREIGN KEY REFERENCES categories(category_id),
    availability_status VARCHAR(20) NOT NULL DEFAULT 'active' CHECK (availability_status IN ('active', 'discontinued')),
    image_url NVARCHAR(255)
);

-- 9. Tạo Bảng orders
CREATE TABLE orders (
    order_id INT IDENTITY(1,1) PRIMARY KEY,
    session_id INT NOT NULL FOREIGN KEY REFERENCES dining_sessions(session_id),
    created_by_employee_id INT NOT NULL FOREIGN KEY REFERENCES employees(employee_id),
    ordered_at DATETIME NOT NULL DEFAULT GETDATE()
);

-- 10. Tạo Bảng order_items
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

-- 11. Tạo Bảng ingredients
CREATE TABLE ingredients (
    ingredient_id INT IDENTITY(1,1) PRIMARY KEY,
    ingredient_name NVARCHAR(100) NOT NULL UNIQUE,
    unit NVARCHAR(20) NOT NULL,
    stock_quantity DECIMAL(10,2) NOT NULL DEFAULT 0,
    min_stock_alert DECIMAL(10,2)
);

-- 12. Tạo Bảng recipes
CREATE TABLE recipes (
    dish_id INT NOT NULL FOREIGN KEY REFERENCES dishes(dish_id),
    ingredient_id INT NOT NULL FOREIGN KEY REFERENCES ingredients(ingredient_id),
    quantity_per_serving DECIMAL(10,2) NOT NULL,
    PRIMARY KEY (dish_id, ingredient_id)
);

-- 13. Tạo Bảng stock_receipts
CREATE TABLE stock_receipts (
    receipt_id INT IDENTITY(1,1) PRIMARY KEY,
    ingredient_id INT NOT NULL FOREIGN KEY REFERENCES ingredients(ingredient_id),
    quantity DECIMAL(10,2) NOT NULL CHECK (quantity > 0),
    unit_cost DECIMAL(10,2),
    received_at DATETIME NOT NULL DEFAULT GETDATE(),
    received_by_employee_id INT FOREIGN KEY REFERENCES employees(employee_id),
    supplier NVARCHAR(150)
);

-- 14. Tạo Bảng invoices
CREATE TABLE invoices (
    invoice_id INT IDENTITY(1,1) PRIMARY KEY,
    session_id INT NOT NULL FOREIGN KEY REFERENCES dining_sessions(session_id),
    subtotal DECIMAL(10,2) NOT NULL,
    discount DECIMAL(10,2) NOT NULL DEFAULT 0,
    total_amount DECIMAL(10,2) NOT NULL,
    paid_at DATETIME NOT NULL DEFAULT GETDATE(),
    cashier_employee_id INT FOREIGN KEY REFERENCES employees(employee_id)
);

-- 15. Tạo Bảng payment_details
CREATE TABLE payment_details (
    payment_id INT IDENTITY(1,1) PRIMARY KEY,
    invoice_id INT NOT NULL FOREIGN KEY REFERENCES invoices(invoice_id),
    method VARCHAR(20) NOT NULL CHECK (method IN ('cash', 'bank_transfer', 'card')),
    amount DECIMAL(10,2) NOT NULL
);
GO

-- =======================================================
-- CHÈN DỮ LIỆU ĐỂ KIỂM THỬ BAN ĐẦU (SEED DATA)
-- =======================================================

-- Nhân viên mẫu
INSERT INTO employees (full_name, username, password_hash, role, phone, is_active) VALUES
(N'Manager', 'manager', '123456', 'manager', '0912345678', 1),
(N'Waiter', 'waiter', '123456', 'waiter', '0922345678', 1),
(N'Kitchen', 'kitchen', '123456', 'kitchen', '0932345678', 1),
(N'Cashier', 'cashier', '123456', 'cashier', '0942345678', 1);

-- Danh mục món ăn
INSERT INTO categories (category_name) VALUES
(N'Khai vị'),
(N'Món chính'),
(N'Lẩu'),
(N'Đồ uống'),
(N'Tráng miệng');

-- Các món ăn mẫu
INSERT INTO dishes (dish_name, price, category_id, availability_status) VALUES
(N'Gỏi ngó sen tôm thịt', 95000, 1, 'active'),
(N'Cơm chiên hải sản', 120000, 2, 'active'),
(N'Bò lúc lắc khoai tây chiên', 150000, 2, 'active'),
(N'Lẩu thái chua cay', 250000, 3, 'active'),
(N'Trà đào cam sả', 35000, 4, 'active'),
(N'Nước ngọt lon', 20000, 4, 'active'),
(N'Rau câu trái dừa', 40000, 5, 'active');

-- Bàn ăn mẫu
INSERT INTO restaurant_tables (table_name, capacity, status, area) VALUES
(N'Bàn 1 (Trệt)', 4, 'available', N'Tầng trệt'),
(N'Bàn 2 (Trệt)', 4, 'available', N'Tầng trệt'),
(N'Bàn 3 (Trệt)', 6, 'available', N'Tầng trệt'),
(N'Bàn 4 (Lầu 1)', 2, 'available', N'Tầng 1'),
(N'Bàn 5 (Lầu 1)', 8, 'available', N'Tầng 1');

-- Khách hàng mẫu
INSERT INTO customers (full_name, phone, membership_tier, loyalty_points) VALUES
(N'Khách vãng lai', '0000000000', 'regular', 0),
(N'Nguyễn Thị Hoa', '0909090909', 'vip', 150);
GO


-- =======================================================
-- PHẦN TIẾP THEO
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
-- PHẦN TIẾP THEO
-- =======================================================

-- =======================================================
-- CHÈN DỮ LIỆU MẪU CHO HỆ THỐNG KHO & NGUYÊN LIỆU
-- Chạy script này sau khi đã chạy database_schema.sql
-- =======================================================

USE RestaurantPOS;
GO

-- 1. Thêm nguyên liệu mẫu
INSERT INTO ingredients (ingredient_name, unit, stock_quantity, min_stock_alert) VALUES
(N'Thịt bò Mỹ', N'kg', 0, 5),
(N'Tôm sú', N'kg', 0, 3),
(N'Khoai tây', N'kg', 0, 10),
(N'Ngó sen', N'kg', 0, 2),
(N'Rau xà lách', N'kg', 0, 5),
(N'Gạo', N'kg', 0, 20),
(N'Nước mắm', N'chai', 0, 5),
(N'Dầu ăn', N'lít', 0, 10),
(N'Hành tím', N'kg', 0, 3),
(N'Tỏi', N'kg', 0, 2),
(N'Ớt tươi', N'kg', 0, 1),
(N'Chanh', N'kg', 0, 2),
(N'Nấm kim châm', N'gói', 0, 10),
(N'Bún tươi', N'kg', 0, 5),
(N'Nước cốt dừa', N'hộp', 0, 5),
(N'Trà Ô Long', N'gói', 0, 10),
(N'Đào ngâm', N'hộp', 0, 5),
(N'Cam tươi', N'kg', 0, 5),
(N'Sả', N'bó', 0, 5),
(N'Bột rau câu', N'gói', 0, 10);
GO

-- 2. Nhập kho nguyên liệu (Tạo phiếu nhập hàng mẫu)
-- Nhập kho Thịt bò Mỹ
INSERT INTO stock_receipts (ingredient_id, quantity, unit_cost, received_by_employee_id, supplier) VALUES
(1, 15, 220000, 1, N'Thực phẩm sạch Metro'),
(2, 10, 180000, 1, N'Hải sản tươi sống Biển Đông'),
(3, 20, 15000, 1, N'Chợ đầu mối Bình Điền'),
(4, 5, 35000, 1, N'Chợ đầu mối Bình Điền'),
(5, 8, 25000, 1, N'Nông trại rau sạch Đà Lạt'),
(6, 50, 18000, 1, N'Gạo ST25 Sóc Trăng'),
(7, 10, 22000, 1, N'Siêu thị Bách Hóa Xanh'),
(8, 15, 35000, 1, N'Dầu Tường An'),
(9, 5, 30000, 1, N'Chợ đầu mối Bình Điền'),
(10, 3, 45000, 1, N'Chợ đầu mối Bình Điền'),
(11, 2, 40000, 1, N'Chợ đầu mối Bình Điền'),
(12, 3, 20000, 1, N'Chợ đầu mối Bình Điền'),
(13, 20, 12000, 1, N'Siêu thị Bách Hóa Xanh'),
(14, 10, 15000, 1, N'Chợ đầu mối Bình Điền'),
(15, 10, 18000, 1, N'Siêu thị Bách Hóa Xanh'),
(16, 20, 25000, 1, N'Trà Thái Nguyên'),
(17, 10, 35000, 1, N'Siêu thị Bách Hóa Xanh'),
(18, 8, 25000, 1, N'Chợ đầu mối Bình Điền'),
(19, 10, 8000, 1, N'Chợ đầu mối Bình Điền'),
(20, 15, 15000, 1, N'Siêu thị Bách Hóa Xanh');
GO

-- Cập nhật tồn kho theo phiếu nhập (mô phỏng hệ thống tự động cộng dồn)
UPDATE ingredients SET stock_quantity = 15 WHERE ingredient_id = 1;   -- Thịt bò Mỹ
UPDATE ingredients SET stock_quantity = 10 WHERE ingredient_id = 2;   -- Tôm sú
UPDATE ingredients SET stock_quantity = 20 WHERE ingredient_id = 3;   -- Khoai tây
UPDATE ingredients SET stock_quantity = 5  WHERE ingredient_id = 4;   -- Ngó sen
UPDATE ingredients SET stock_quantity = 8  WHERE ingredient_id = 5;   -- Rau xà lách
UPDATE ingredients SET stock_quantity = 50 WHERE ingredient_id = 6;   -- Gạo
UPDATE ingredients SET stock_quantity = 10 WHERE ingredient_id = 7;   -- Nước mắm
UPDATE ingredients SET stock_quantity = 15 WHERE ingredient_id = 8;   -- Dầu ăn
UPDATE ingredients SET stock_quantity = 5  WHERE ingredient_id = 9;   -- Hành tím
UPDATE ingredients SET stock_quantity = 3  WHERE ingredient_id = 10;  -- Tỏi
UPDATE ingredients SET stock_quantity = 2  WHERE ingredient_id = 11;  -- Ớt tươi
UPDATE ingredients SET stock_quantity = 3  WHERE ingredient_id = 12;  -- Chanh
UPDATE ingredients SET stock_quantity = 20 WHERE ingredient_id = 13;  -- Nấm kim châm
UPDATE ingredients SET stock_quantity = 10 WHERE ingredient_id = 14;  -- Bún tươi
UPDATE ingredients SET stock_quantity = 10 WHERE ingredient_id = 15;  -- Nước cốt dừa
UPDATE ingredients SET stock_quantity = 20 WHERE ingredient_id = 16;  -- Trà Ô Long
UPDATE ingredients SET stock_quantity = 10 WHERE ingredient_id = 17;  -- Đào ngâm
UPDATE ingredients SET stock_quantity = 8  WHERE ingredient_id = 18;  -- Cam tươi
UPDATE ingredients SET stock_quantity = 10 WHERE ingredient_id = 19;  -- Sả
UPDATE ingredients SET stock_quantity = 15 WHERE ingredient_id = 20;  -- Bột rau câu
GO

-- 3. Tạo công thức định lượng mẫu cho các món ăn
-- Món 1: Gỏi ngó sen tôm thịt (dish_id = 1)
INSERT INTO recipes (dish_id, ingredient_id, quantity_per_serving) VALUES
(1, 4, 0.10),   -- Ngó sen 100g
(1, 2, 0.08),   -- Tôm sú 80g
(1, 12, 0.05),  -- Chanh 50g
(1, 11, 0.02),  -- Ớt tươi 20g
(1, 7, 0.02);   -- Nước mắm 20ml

-- Món 2: Cơm chiên hải sản (dish_id = 2)
INSERT INTO recipes (dish_id, ingredient_id, quantity_per_serving) VALUES
(2, 6, 0.20),   -- Gạo 200g
(2, 2, 0.10),   -- Tôm sú 100g
(2, 8, 0.03),   -- Dầu ăn 30ml
(2, 9, 0.02),   -- Hành tím 20g
(2, 10, 0.01),  -- Tỏi 10g
(2, 7, 0.02);   -- Nước mắm 20ml

-- Món 3: Bò lúc lắc khoai tây chiên (dish_id = 3)
INSERT INTO recipes (dish_id, ingredient_id, quantity_per_serving) VALUES
(3, 1, 0.20),   -- Thịt bò Mỹ 200g
(3, 3, 0.15),   -- Khoai tây 150g
(3, 8, 0.05),   -- Dầu ăn 50ml
(3, 10, 0.02),  -- Tỏi 20g
(3, 5, 0.05);   -- Rau xà lách 50g

-- Món 4: Lẩu thái chua cay (dish_id = 4)
INSERT INTO recipes (dish_id, ingredient_id, quantity_per_serving) VALUES
(4, 2, 0.15),   -- Tôm sú 150g
(4, 13, 1.00),  -- Nấm kim châm 1 gói
(4, 14, 0.20),  -- Bún tươi 200g
(4, 11, 0.03),  -- Ớt tươi 30g
(4, 19, 0.50),  -- Sả 0.5 bó
(4, 12, 0.05),  -- Chanh 50g
(4, 7, 0.03);   -- Nước mắm 30ml

-- Món 5: Trà đào cam sả (dish_id = 5)
INSERT INTO recipes (dish_id, ingredient_id, quantity_per_serving) VALUES
(5, 16, 0.50),  -- Trà Ô Long 0.5 gói
(5, 17, 0.30),  -- Đào ngâm 0.3 hộp
(5, 18, 0.10),  -- Cam tươi 100g
(5, 19, 0.20);  -- Sả 0.2 bó

-- Món 7: Rau câu trái dừa (dish_id = 7)
INSERT INTO recipes (dish_id, ingredient_id, quantity_per_serving) VALUES
(7, 20, 0.50),  -- Bột rau câu 0.5 gói
(7, 15, 0.30);  -- Nước cốt dừa 0.3 hộp
GO

PRINT N'✅ Đã chèn dữ liệu mẫu thành công cho Nguyên liệu, Phiếu nhập kho và Công thức định lượng!';
GO
