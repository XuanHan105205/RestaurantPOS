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

-- Xóa các khóa ngoại cũ nếu có để tránh lỗi chạy lại script
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
