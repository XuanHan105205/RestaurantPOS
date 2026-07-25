using System.Windows;

namespace RestaurantPOS.Services
{
    public class WpfDialogService : IDialogService
    {
        public bool Confirm(string title, string message)
        {
            return MessageBox.Show(
                message,
                title,
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning) == MessageBoxResult.Yes;
        }

        public void ShowMessage(string title, string message)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
