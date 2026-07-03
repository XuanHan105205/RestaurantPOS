using RestaurantPOS.Models;

namespace RestaurantPOS.Services
{
    public class AuthService : IAuthService
    {
        private static AuthService _instance;
        public static AuthService Instance => _instance ??= new AuthService();

        public Employee CurrentUser { get; private set; }

        private AuthService() { }

        public bool Login(string username, string password)
        {
            // TODO: Hàn (Leader) will implement Login verification here.
            return false;
        }

        public void Logout()
        {
            CurrentUser = null;
        }
    }
}
