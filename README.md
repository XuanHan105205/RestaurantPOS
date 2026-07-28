# RestaurantPOS

Ứng dụng quản lý nhà hàng viết bằng C# WPF theo mô hình MVVM, sử dụng EF Core và SQL Server.

## Chức năng chính

- Đăng nhập và phân quyền nhân viên.
- Quản lý bàn, phiên dùng bàn và gọi món.
- Màn hình bếp với luồng `pending -> cooking -> ready -> served`.
- Quản lý nguyên liệu, nhập kho và công thức món ăn.
- Tự động trừ kho khi món chuyển sang `ready`.
- Thanh toán, chia nhiều phương thức, tích điểm và báo cáo doanh thu.

## Yêu cầu

- Windows 10/11.
- .NET 8 SDK.
- SQL Server và SQL Server Management Studio.
- Visual Studio 2022 với workload .NET Desktop Development.

## Cài đặt database

1. Mở `Database/database_schema.sql` bằng SQL Server Management Studio và chạy script.
2. Chạy `Database/create_views.sql` để tạo các view báo cáo nếu cần.
3. Nếu muốn thêm dữ liệu kho mẫu, chạy `Database/seed_inventory_data.sql`.
4. Mở `RestaurantPOS/appsettings.json` và sửa tên SQL Server cho phù hợp với máy.

Chuỗi kết nối mặc định:

```json
"Server=localhost;Database=RestaurantPOS;Trusted_Connection=True;TrustServerCertificate=True;"
```

## Chạy chương trình

```powershell
dotnet restore
dotnet build RestaurantPOS.slnx
dotnet run --project RestaurantPOS/RestaurantPOS.csproj
```

## Chạy kiểm thử

```powershell
dotnet test RestaurantPOS.slnx
```

## Kiến trúc

Mọi thao tác database tuân theo luồng:

```text
View -> ViewModel -> Service -> Repository -> RestaurantPOSDbContext
```

Chi tiết phân công và quy tắc của nhóm nằm trong `MEMBER_ASSIGNMENT.md`.
