using BergerBytes.App.Services;
using BergerBytes.App.ViewModels;

namespace BergerBytes.App.Pages
{
    public partial class WeightLogPage : ContentPage
    {
        private readonly WeightLogPageViewModel _viewModel;

        public WeightLogPage(IWeightLogRepository repository, ISettingsRepository settingsRepository)
        {
            InitializeComponent();
            _viewModel = new WeightLogPageViewModel(repository, settingsRepository);
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.InitializeAsync();
        }
    }
}
