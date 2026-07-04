using RestaurantPOS.MVVM;
using RestaurantPOS.Services;
using RestaurantPOS.Views.Core;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RestaurantPOS.ViewModels.Core
{
    public class LoginViewModel : ViewModelBase
    {
        private string _username = string.Empty;
        private string _errorMessage = string.Empty;
        // Thuộc tính liên kết với TextBox nhập tên tài khoản
        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }
        // Thuộc tính hiển thị thông báo lỗi nếu đăng nhập sai
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                if (SetProperty(ref _errorMessage, value))
                {
                    OnPropertyChanged(nameof(HasError));
                }
            }
        }
        // Thuộc tính để kiểm tra xem có lỗi đang hiển thị hay không (dùng cho Visibility Converter)
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
        // Command liên kết với nút bấm Đăng nhập.
        // Dùng kiểu RelayCommand<PasswordBox> để nhận đối tượng PasswordBox làm tham số đầu vào.
        public ICommand LoginCommand { get; }
        public LoginViewModel()
        {
            // Khởi tạo command đăng nhập
            LoginCommand = new RelayCommand<PasswordBox>(ExecuteLogin, CanExecuteLogin);
        }
        // Hàm kiểm tra xem nút Đăng nhập có được sáng lên cho phép bấm hay không
        private bool CanExecuteLogin(PasswordBox passwordBox)
        {
            // Chỉ cho phép nhấn đăng nhập nếu người dùng đã gõ tên tài khoản
            return !string.IsNullOrWhiteSpace(Username);
        }
        // Hàm thực thi khi người dùng bấm nút Đăng nhập
        private void ExecuteLogin(PasswordBox passwordBox)
        {
            if (passwordBox == null) return;
            // Lấy mật khẩu một cách an toàn trực tiếp từ PasswordBox
            string password = passwordBox.Password;
            
            try
            {
                // 1. Gọi dịch vụ xác thực kiểm tra dưới CSDL
                bool isSuccess = AuthService.Instance.Login(Username, password);
                if (isSuccess)
                {
                    ErrorMessage = string.Empty;
                    // 2. Đăng nhập thành công -> Khởi tạo màn hình chính MainShellWindow
                    var mainShell = new MainShellWindow();
                    
                    // Thiết lập MainWindow mới để tránh ứng dụng tự tắt khi đóng LoginWindow
                    if (Application.Current != null)
                    {
                        Application.Current.MainWindow = mainShell;
                    }

                    mainShell.Show();
                    // 3. Tìm cửa sổ đăng nhập hiện tại thông qua PasswordBox và đóng nó lại
                    var currentWindow = Window.GetWindow(passwordBox);
                    currentWindow?.Close();
                }
                else
                {
                    // 4. Nếu thất bại -> Cập nhật thông báo lỗi lên màn hình
                    ErrorMessage = "Tên đăng nhập hoặc mật khẩu không đúng!";
                }
            }
            catch (System.Exception ex)
            {
                ErrorMessage = $"Lỗi kết nối DB: {ex.Message}";
            }
        }
    }
}
