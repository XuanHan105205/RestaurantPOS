using RestaurantPOS.MVVM;

namespace RestaurantPOS.ViewModels.Billing
{
    public class BillingViewModel : ViewModelBase
    {
        public CheckoutViewModel CheckoutVM { get; }
        public ReportDashboardViewModel ReportVM { get; }

        public BillingViewModel()
        {
            CheckoutVM = new CheckoutViewModel();
            ReportVM = new ReportDashboardViewModel();
        }
    }
}
