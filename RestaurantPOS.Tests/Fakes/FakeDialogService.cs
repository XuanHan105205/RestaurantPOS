using System.Collections.Generic;
using RestaurantPOS.Models;
using RestaurantPOS.Services;

namespace RestaurantPOS.Tests.Fakes
{
    public class FakeDialogService : IDialogService
    {
        public List<string> LastMessages { get; } = new();
        public List<string> LastConfirmations { get; } = new();
        public bool ConfirmResult { get; set; } = true;
        public bool? ShowOrderDetailPopupResult { get; set; } = true;
        public bool ShowOrderDetailPopupCalled { get; private set; }

        public void ShowMessage(string message, string title = "Thông báo")
        {
            LastMessages.Add($"{title}: {message}");
        }

        public bool Confirm(string message, string title = "Xác nhận")
        {
            LastConfirmations.Add($"{title}: {message}");
            return ConfirmResult;
        }

        public bool? ShowOrderDetailPopup(DiningSession session, string tableName)
        {
            ShowOrderDetailPopupCalled = true;
            return ShowOrderDetailPopupResult;
        }
    }
}
