namespace RestaurantPOS.MVVM
{
    public class NavigationService : ViewModelBase
    {
        private static NavigationService _instance;
        public static NavigationService Instance => _instance ??= new NavigationService();

        private ViewModelBase _currentViewModel;
        public ViewModelBase CurrentViewModel
        {
            get => _currentViewModel;
            set => SetProperty(ref _currentViewModel, value);
        }

        private NavigationService() { }
    }
}
