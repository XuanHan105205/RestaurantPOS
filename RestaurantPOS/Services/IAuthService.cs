using RestaurantPOS.Models;

namespace RestaurantPOS.Services
{
    public interface IAuthService
    {
        Employee? CurrentUser { get; }
        bool Login(string username, string password);
        bool Login(string username, string password, out string errorMessage);
        void Logout();
    }
}
