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
