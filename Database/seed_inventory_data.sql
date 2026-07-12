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
