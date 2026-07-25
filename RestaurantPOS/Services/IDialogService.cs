namespace RestaurantPOS.Services
{
    public interface IDialogService
    {
        bool Confirm(string title, string message);
        void ShowMessage(string title, string message);
    }
}
