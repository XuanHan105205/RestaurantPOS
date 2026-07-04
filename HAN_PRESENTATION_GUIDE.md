# CẨM NANG BẢO VỆ ĐỒ ÁN - DÀNH CHO HÀN (MEMBER 5)
*Tài liệu hướng dẫn chi tiết cách trả lời câu hỏi của Giảng viên khi phản biện đồ án Restaurant POS.*

---

## 🧭 PHẦN 1: TỔNG QUAN KIẾN TRÚC DỰ ÁN
Dự án được xây dựng trên mô hình **WPF (Windows Presentation Foundation)** kết hợp kiến trúc **MVVM (Model-View-ViewModel)** và **EF Core (Entity Framework Core)** làm ORM kết nối SQL Server.

### Sơ đồ luồng đi của dữ liệu:
`Giao diện (View/XAML)` ➔ `Điều khiển (ViewModel)` ➔ `Logic nghiệp vụ (Service)` ➔ `Kho dữ liệu (Repository)` ➔ `Cơ sở dữ liệu (DbContext / SQL Server)`

* **Lý do đi theo luồng này**: Tách biệt hoàn toàn giao diện (UI) và nghiệp vụ (Business Logic). Nếu sau này muốn đổi giao diện (từ WPF sang Web hoặc Mobile App), ta chỉ cần viết lại tầng View, toàn bộ logic nghiệp vụ bên dưới (Service, Repository, DbContext) giữ nguyên 100%.

---

## 🛠 PHẦN 2: CHI TIẾT CÁC FILE CODE HÀN ĐÃ SETUP

### 1. Thư mục `MVVM/` (Bộ khung cơ bản của dự án)
Đây là các class đóng vai trò làm "móng nhà", tất cả các thành viên khác khi code View/ViewModel đều phải kế thừa nó.

#### 📄 `ViewModelBase.cs`
* **Nhiệm vụ**: Thực thi Interface `INotifyPropertyChanged`.
* **Vì sao phải khai báo/sử dụng nó**: WPF liên kết dữ liệu qua cơ chế Data Binding. Khi một thuộc tính trong C# thay đổi (ví dụ: `ErrorMessage` của màn đăng nhập thay đổi), nếu không thông báo cho giao diện biết, ô chữ trên giao diện sẽ không tự cập nhật. 
* **Giúp ích gì**: Hàm `SetProperty` và sự kiện `PropertyChanged` trong class này giúp tự động bắn tín hiệu báo cho XAML biết: *"Thuộc tính này đã đổi giá trị rồi, vẽ lại giao diện đi!"*. Tất cả các ViewModel đều kế thừa class này.

#### 📄 `RelayCommand.cs` & `RelayCommand<T>.cs`
* **Nhiệm vụ**: Thực thi Interface `ICommand` của WPF.
* **Vì sao phải khai báo/sử dụng nó**: Trong WPF, ta không viết sự kiện click chuột trực tiếp kiểu `Button_Click` trong code-behind (vì vi phạm MVVM). Thay vào đó, ta sử dụng thuộc tính `Command` của nút bấm để Binding xuống một biến Command trong ViewModel.
* **Giúp ích gì**: Đóng gói các hàm thực thi hành động (như `ExecuteLogin` hoặc `ExecuteAddCustomer`) thành các đối tượng Command mà XAML có thể binding tới. Đồng thời có hàm `CanExecute` để kiểm soát nút bấm đó được phép ấn hay bị disable (ví dụ: chỉ cho ấn Đăng nhập khi đã gõ tài khoản).

#### 📄 `NavigationService.cs`
* **Nhiệm vụ**: Quản lý việc chuyển đổi giữa các màn hình (Views) bên trong cửa sổ chính `MainShellWindow`.
* **Vì sao phải gọi nó**: Đây là một class Singleton (chỉ có duy nhất một thực thể trong suốt vòng đời ứng dụng). Khi người dùng nhấn nút chuyển tab trên Sidebar, ViewModel sẽ gọi `NavigationService.Instance.CurrentViewModel = new XViewModel();`.
* **Giúp ích gì**: Giao diện chính của ứng dụng (`MainShellWindow`) lắng nghe biến `CurrentViewModel` này để tự động hiển thị View tương ứng thông qua cơ chế `DataTemplate` mà không cần tắt đi mở lại các cửa sổ mới.

---

### 2. Thư mục `Data/` (Tầng kết nối cơ sở dữ liệu)

#### 📄 `RestaurantPOSDbContext.cs` (Entity Framework Core)
* **Nhiệm vụ**: Quản lý phiên kết nối với SQL Server cục bộ và ánh xạ các bảng CSDL thành các đối tượng C#.
* **Vì sao phải khai báo/sử dụng nó**: Đây là trái tim của tầng dữ liệu. Nó kế thừa lớp `DbContext` của EF Core. 
* **Chi tiết bên trong**:
  * Hàm `OnConfiguring`: Đọc chuỗi kết nối từ file cấu hình `appsettings.json` để kết nối vào Database.
  * Hàm `OnModelCreating` (Fluent API): Ánh xạ tên bảng và các tên cột dạng gạch dưới của SQL Server (như `customer_id`, `full_name`) thành các thuộc tính C# dạng chữ hoa đầu từ (`CustomerId`, `FullName`) để code sạch sẽ hơn.
* **Giúp ích gì**: Giúp loại bỏ hoàn toàn việc viết câu lệnh SQL bằng chuỗi string thủ công. Giảng viên rất thích cái này vì nó thể hiện kỹ năng lập trình hướng đối tượng chuyên nghiệp.

---

### 3. Thư mục `Repositories/` (Tầng đọc/ghi dữ liệu thô)

#### 📄 `EmployeeRepository.cs` & `CustomerRepository.cs`
* **Nhiệm vụ**: Trực tiếp giao tiếp với `RestaurantPOSDbContext` để thực hiện các thao tác CRUD dữ liệu.
* **Vì sao khai báo**: Tách biệt mã nguồn truy xuất dữ liệu. ViewModel và Service không cần quan tâm dữ liệu lấy từ SQL Server hay file Text, nó chỉ cần gọi Repository để lấy đối tượng C#.
* **Giúp ích gì**: 
  * Class `EmployeeRepository` chứa hàm `GetByUsername(string username)` sử dụng LINQ: `context.Employees.FirstOrDefault(e => e.Username == username)`.
  * Class `CustomerRepository` chứa đầy đủ các hàm Add, Update, Delete, GetAll bằng các API ngắn gọn của EF Core (ví dụ: `context.Customers.Add(entity)` và `context.SaveChanges()`).

---

### 4. Thư mục `Services/` (Tầng xử lý logic nghiệp vụ)

#### 📄 `AuthService.cs` (Dịch vụ xác thực)
* **Nhiệm vụ**: Xử lý logic Đăng nhập / Đăng xuất và lưu giữ thông tin nhân viên đang đăng nhập hiện tại (`CurrentUser`).
* **Vì sao phải gọi**: Khi người dùng nhấn Đăng nhập, ViewModel không tự gọi xuống DB, mà gọi qua `AuthService.Instance.Login(username, password)`.
* **Nghiệp vụ bên trong**: Lấy thông tin nhân viên từ `EmployeeRepository`. So khớp trạng thái `IsActive` (nhân viên còn làm việc không) và so sánh mật khẩu (hiện tại đang so sánh chuỗi trần `PasswordHash == password` phục vụ cho việc kiểm thử nhanh của giáo viên).

#### 📄 `CustomerService.cs` (Dịch vụ khách hàng)
* **Nhiệm vụ**: Chứa logic nghiệp vụ liên quan đến khách hàng, tiêu biểu là **Tự động thăng hạng thành viên**.
* **Nghiệp vụ bên trong (Hàm `UpdateMembershipTier`)**:
  * Nếu điểm tích lũy (`LoyaltyPoints`) >= 1000: Tự thăng hạng thành `vip_gold`.
  * Nếu điểm tích lũy >= 500: Thăng hạng thành `vip`.
  * Dưới 500 điểm: Hạng `regular`.
* **Giúp ích gì**: Đảm bảo nghiệp vụ thăng hạng diễn ra tự động bất cứ khi nào thông tin khách hàng được lưu hoặc cập nhật điểm.

---

### 5. Thư mục `Views/` và `ViewModels/` (Tầng Giao diện)

#### 📄 Màn hình Đăng nhập (`LoginWindow.xaml` & `LoginViewModel.cs`)
* **Nhiệm vụ**: Cho phép nhân viên đăng nhập vào hệ thống.
* **Các điểm đặc biệt cần giải thích với thầy cô**:
  * **Xử lý Mật khẩu an toàn**: WPF không cho phép Binding trực tiếp thuộc tính `Password` của `PasswordBox` vì lý do bảo mật. Do đó, ta truyền chính đối tượng `PasswordBox` vào CommandParameter của nút Đăng nhập: `CommandParameter="{Binding ElementName=TxtPassword}"`. Trong ViewModel, ta lấy mật khẩu ra bằng: `string password = passwordBox.Password;`.
  * **Tránh lỗi tự tắt ứng dụng**: Khi đăng nhập thành công và mở màn hình chính `MainShellWindow`, ta phải gán: `Application.Current.MainWindow = mainShell;` rồi mới đóng cửa sổ đăng nhập cũ. Điều này báo cho WPF biết cửa sổ chính đã thay đổi, tránh ứng dụng tự tắt.
  * **Bẫy lỗi kết nối**: Bọc toàn bộ quá trình đăng nhập trong khối `try-catch`. Nếu SQL Server chưa bật, chương trình sẽ báo lỗi *"Lỗi kết nối DB..."* thay vì crash ứng dụng.

#### 📄 Màn hình chính (`MainShellWindow.xaml` & `MainShellViewModel.cs`)
* **Nhiệm vụ**: Đóng vai trò là "Khung sườn" chính của ứng dụng. Chứa Sidebar bên trái để chuyển tab và vùng nội dung chính bên phải.
* **Các điểm đặc biệt cần giải thích với thầy cô**:
  * **Phân quyền Sidebar động**: Sử dụng thuộc tính Role của tài khoản đăng nhập thành công để kiểm soát ẩn/hiện các nút bấm menu thông qua thuộc tính `Visibility` kết hợp bộ chuyển đổi `BooleanToVisibilityConverter` của WPF. Ví dụ: Nếu là Bếp (`kitchen`), chỉ nút "Màn hình nhà bếp" hiện lên, các nút khác ẩn đi.
  * **Chuyển đổi View bằng DataTemplate**: Trong tài nguyên của Window (`Window.Resources`), ta khai báo ánh xạ: cứ ViewModel nào thì đi với View đó. Ví dụ:
    ```xml
    <DataTemplate DataType="{x:Type vm:CustomerManagementViewModel}">
        <coreView:CustomerManagementView />
    </DataTemplate>
    ```
    Bên góc phải giao diện ta dùng một thẻ `<ContentControl Content="{Binding Navigation.CurrentViewModel}" />`. Khi người dùng click nút trên Sidebar, ta chỉ cần thay đổi đối tượng `CurrentViewModel`, WPF sẽ tự động tìm và vẽ đúng View tương ứng lên vùng phải màn hình mà không cần mở thêm Window mới.

#### 📄 Giao diện Khách hàng (`CustomerManagementView.xaml` & `CustomerManagementViewModel.cs`)
* **Nhiệm vụ**: Quản lý hồ sơ khách hàng, tìm kiếm khách hàng, tính điểm.
* **Các điểm đặc biệt cần giải thích với thầy cô**:
  * **Layout Split-screen**: Chia làm 2 cột bằng Grid. Cột trái (rộng) là danh sách khách hàng dạng `DataGrid` kèm thanh tìm kiếm. Cột phải (hẹp hơn, 360px) là Form nhập liệu chi tiết. Thiết kế này giúp người dùng vừa nhìn được tổng quan vừa thao tác sửa đổi nhanh.
  * **Tạo Placeholder tìm kiếm nguyên bản**: Sử dụng `VisualBrush` vẽ chữ *"Nhập số điện thoại để tìm kiếm..."* ẩn đi khi ô nhập liệu có văn bản mà không cần dùng code-behind.
  * **Tự động điền dữ liệu (Auto-fill Form)**: Lắng nghe sự kiện chọn dòng trên Grid thông qua thuộc tính `SelectedCustomer`. Khi thuộc tính này thay đổi, ViewModel lập tức copy các giá trị tương ứng (`FullName`, `Phone`, `LoyaltyPoints`, `MembershipTier`) đổ vào các thuộc tính liên kết với Form để hiển thị lên cho người dùng chỉnh sửa.

---

## 🙋 PHẦN 3: CÁC CÂU HỎI GIÁO VIÊN THƯỜNG HỎI (Q&A BẢO VỆ)

**Câu 1: Tại sao em lại dùng mô hình MVVM? Nó có ưu điểm gì so với việc viết code trực tiếp trong sự kiện (Code-Behind)?**
* **Trả lời**: MVVM giúp tách biệt hoàn toàn giao diện (View) và logic nghiệp vụ (ViewModel). Việc này giúp dự án dễ bảo trì, dễ viết kiểm thử tự động (Unit Test) cho các hàm xử lý dữ liệu mà không cần chạy giao diện lên để ấn nút. Ngoài ra, nó giúp nhiều thành viên trong nhóm có thể làm việc song song (người thiết kế giao diện XAML, người viết logic C#) mà không sợ bị đè code lên nhau gây xung đột Git.

**Câu 2: Tại sao em dùng EF Core thay vì viết câu lệnh SQL thuần (ADO.NET)?**
* **Trả lời**: EF Core là một thư viện ORM mạnh mẽ. Nó giúp giảm thiểu tối đa các câu lệnh SQL viết tay dạng chuỗi (dễ viết sai chính tả tên bảng/tên cột và khó debug khi biên dịch). EF Core tự động ánh xạ dữ liệu bảng thành các Class trong C#, giúp chúng ta truy vấn dữ liệu bằng cú pháp LINQ trực quan, hỗ trợ nhắc lệnh thông minh (IntelliSense) và đảm bảo an toàn tuyệt đối trước các cuộc tấn công SQL Injection.

**Câu 3: Cơ chế chuyển đổi màn hình (Navigation) trong ứng dụng của em hoạt động thế nào?**
* **Trả lời**: Em sử dụng cơ chế liên kết View-ViewModel động của WPF. Em khai báo các `DataTemplate` trong tài nguyên của cửa sổ chính để gán cặp View và ViewModel tương ứng. Trên giao diện, em đặt một `ContentControl` liên kết với thuộc tính `CurrentViewModel` của `NavigationService`. Khi người dùng chọn chức năng trên Sidebar, ViewModel chỉ việc đổi giá trị của `CurrentViewModel`, WPF sẽ tự nhận diện và nạp giao diện View tương ứng vào `ContentControl` một cách mượt mà.

**Câu 4: Em xử lý vấn đề bảo mật mật khẩu ở màn hình Đăng nhập như thế nào khi sử dụng MVVM?**
* **Trả lời**: Trong WPF, thuộc tính `Password` của `PasswordBox` không phải là một Dependency Property nên không thể binding trực tiếp xuống ViewModel vì lý do an toàn thông tin (tránh lộ mật khẩu trong bộ nhớ). Để giải quyết việc này đúng chuẩn MVVM, em đã chuyển đối tượng `PasswordBox` làm tham số lệnh (`CommandParameter`) gửi xuống hàm `ExecuteLogin(PasswordBox passwordBox)` trong ViewModel. Tại đây, mật khẩu mới được giải mã tạm thời để truyền xuống tầng dịch vụ xác thực kiểm tra dưới cơ sở dữ liệu.

**Câu 5: Tại sao lớp Service lại đứng giữa ViewModel và Repository?**
* **Trả lời**: Lớp Repository chỉ làm nhiệm vụ đọc/ghi dữ liệu thô từ cơ sở dữ liệu lên. Lớp Service là nơi chứa **logic nghiệp vụ (Business Rules)** thực sự của ứng dụng. Ví dụ: khi sửa đổi thông tin khách hàng, trước khi lưu xuống DB qua Repository, ta cần chạy logic tự động tính toán để thăng hạng VIP hay Gold dựa trên số điểm tích lũy của họ. Logic tính toán đó thuộc về lớp Service. ViewModel chỉ chịu trách nhiệm điều khiển hiển thị và gọi Service để thực thi nghiệp vụ.
