USE RestaurantPOS;
GO

/* Nâng cấp nghiệp vụ vừa đủ cho đồ án. Script có thể chạy lại an toàn. */

IF COL_LENGTH('employees', 'created_at') IS NULL
    ALTER TABLE employees ADD created_at DATETIME NOT NULL CONSTRAINT DF_employees_created_at DEFAULT GETDATE();
GO

DECLARE @employeeRoleConstraint sysname;
SELECT TOP 1 @employeeRoleConstraint = cc.name
FROM sys.check_constraints cc
JOIN sys.columns c ON c.object_id = cc.parent_object_id AND c.column_id = cc.parent_column_id
WHERE cc.parent_object_id = OBJECT_ID('employees') AND c.name = 'role';
IF @employeeRoleConstraint IS NOT NULL EXEC('ALTER TABLE employees DROP CONSTRAINT [' + @employeeRoleConstraint + ']');
ALTER TABLE employees ADD CONSTRAINT CK_employees_role
CHECK (role IN ('waiter', 'kitchen', 'cashier', 'manager', 'inventory'));
GO

IF COL_LENGTH('restaurant_tables', 'is_active') IS NULL
    ALTER TABLE restaurant_tables ADD is_active BIT NOT NULL CONSTRAINT DF_tables_is_active DEFAULT 1;
GO

IF COL_LENGTH('invoices', 'invoice_number') IS NULL
BEGIN
    ALTER TABLE invoices ADD invoice_number VARCHAR(30) NULL;
    EXEC('UPDATE invoices SET invoice_number = ''HD'' + RIGHT(''00000000'' + CAST(invoice_id AS VARCHAR(8)), 8) WHERE invoice_number IS NULL');
    ALTER TABLE invoices ALTER COLUMN invoice_number VARCHAR(30) NOT NULL;
    ALTER TABLE invoices ADD CONSTRAINT UQ_invoices_invoice_number UNIQUE(invoice_number);
END;
IF COL_LENGTH('invoices', 'status') IS NULL
    ALTER TABLE invoices ADD status VARCHAR(20) NOT NULL CONSTRAINT DF_invoices_status DEFAULT 'paid';
IF COL_LENGTH('invoices', 'cancelled_at') IS NULL ALTER TABLE invoices ADD cancelled_at DATETIME NULL;
IF COL_LENGTH('invoices', 'cancelled_by_employee_id') IS NULL
    ALTER TABLE invoices ADD cancelled_by_employee_id INT NULL FOREIGN KEY REFERENCES employees(employee_id);
IF COL_LENGTH('invoices', 'cancellation_reason') IS NULL ALTER TABLE invoices ADD cancellation_reason NVARCHAR(255) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_invoices_status')
    ALTER TABLE invoices ADD CONSTRAINT CK_invoices_status CHECK (status IN ('paid', 'cancelled'));
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_invoices_session' AND object_id = OBJECT_ID('invoices'))
    CREATE UNIQUE INDEX UX_invoices_session ON invoices(session_id);
GO

IF OBJECT_ID('reservations', 'U') IS NULL
CREATE TABLE reservations (
    reservation_id INT IDENTITY(1,1) PRIMARY KEY,
    customer_id INT NOT NULL FOREIGN KEY REFERENCES customers(customer_id),
    table_id INT NOT NULL FOREIGN KEY REFERENCES restaurant_tables(table_id),
    reservation_time DATETIME NOT NULL,
    guest_count INT NOT NULL CHECK (guest_count > 0),
    status VARCHAR(20) NOT NULL DEFAULT 'confirmed'
        CHECK (status IN ('confirmed', 'checked_in', 'cancelled', 'no_show')),
    note NVARCHAR(255),
    created_by_employee_id INT NOT NULL FOREIGN KEY REFERENCES employees(employee_id),
    created_at DATETIME NOT NULL DEFAULT GETDATE()
);
GO

IF OBJECT_ID('stock_movements', 'U') IS NULL
CREATE TABLE stock_movements (
    movement_id INT IDENTITY(1,1) PRIMARY KEY,
    ingredient_id INT NOT NULL FOREIGN KEY REFERENCES ingredients(ingredient_id),
    movement_type VARCHAR(20) NOT NULL
        CHECK (movement_type IN ('receipt', 'sale', 'adjustment_in', 'adjustment_out', 'waste')),
    quantity DECIMAL(10,2) NOT NULL CHECK (quantity > 0),
    quantity_before DECIMAL(10,2) NOT NULL,
    quantity_after DECIMAL(10,2) NOT NULL CHECK (quantity_after >= 0),
    reason NVARCHAR(255),
    reference_id INT,
    employee_id INT NULL FOREIGN KEY REFERENCES employees(employee_id),
    created_at DATETIME NOT NULL DEFAULT GETDATE()
);
GO

IF OBJECT_ID('audit_logs', 'U') IS NULL
CREATE TABLE audit_logs (
    audit_log_id INT IDENTITY(1,1) PRIMARY KEY,
    employee_id INT NULL FOREIGN KEY REFERENCES employees(employee_id),
    action VARCHAR(50) NOT NULL,
    entity_type VARCHAR(50) NOT NULL,
    entity_id INT,
    description NVARCHAR(500),
    created_at DATETIME NOT NULL DEFAULT GETDATE()
);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_reservations_table_time' AND object_id = OBJECT_ID('reservations'))
    CREATE INDEX IX_reservations_table_time ON reservations(table_id, reservation_time, status);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_stock_movements_ingredient_time' AND object_id = OBJECT_ID('stock_movements'))
    CREATE INDEX IX_stock_movements_ingredient_time ON stock_movements(ingredient_id, created_at);
GO

PRINT N'Đã nâng cấp database cho phạm vi nghiệp vụ đồ án.';
GO

CREATE OR ALTER VIEW vw_DailySalesSummary AS
SELECT CAST(paid_at AS DATE) SaleDate, COUNT(invoice_id) TotalInvoices,
       SUM(subtotal) TotalSubtotal, SUM(discount) TotalDiscount, SUM(total_amount) TotalRevenue
FROM invoices WHERE status = 'paid' GROUP BY CAST(paid_at AS DATE);
GO

CREATE OR ALTER VIEW vw_DailyBestSellingDishes AS
SELECT CAST(i.paid_at AS DATE) SaleDate, d.dish_id DishId, d.dish_name DishName,
       SUM(oi.quantity) TotalQuantity, SUM(oi.quantity * oi.unit_price) TotalRevenue
FROM invoices i JOIN dining_sessions ds ON i.session_id=ds.session_id JOIN orders o ON ds.session_id=o.session_id
JOIN order_items oi ON o.order_id=oi.order_id JOIN dishes d ON oi.dish_id=d.dish_id
WHERE i.status='paid' AND oi.status IN ('ready','served')
GROUP BY CAST(i.paid_at AS DATE), d.dish_id, d.dish_name;
GO

CREATE OR ALTER VIEW vw_DailyPaymentBreakdown AS
SELECT CAST(i.paid_at AS DATE) SaleDate, pd.method PaymentMethod, SUM(pd.amount) TotalAmount, COUNT(pd.payment_id) TransactionCount
FROM payment_details pd JOIN invoices i ON pd.invoice_id=i.invoice_id
WHERE i.status='paid' GROUP BY CAST(i.paid_at AS DATE), pd.method;
GO
