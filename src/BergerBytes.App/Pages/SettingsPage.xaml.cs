using BergerBytes.App.Services;
using BergerBytes.App.ViewModels;

namespace BergerBytes.App.Pages
{
    public partial class SettingsPage : ContentPage
    {
        private readonly SettingsPageViewModel _viewModel;

        public SettingsPage(ISettingsRepository settingsRepository)
        {
            InitializeComponent();
            _viewModel = new SettingsPageViewModel(settingsRepository);
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.InitializeAsync();
        }
    }
}
