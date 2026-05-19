using BergerBytes.App.Services;
using BergerBytes.App.ViewModels;

namespace BergerBytes.App.Pages
{
    public partial class DashboardPage : ContentPage
    {
        private readonly DashboardPageViewModel _viewModel;

        public DashboardPage(IMealLogRepository repository, ISettingsRepository settingsRepository)
        {
            InitializeComponent();
            _viewModel = new DashboardPageViewModel(repository, settingsRepository);
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.InitializeAsync();
        }
    }
}
