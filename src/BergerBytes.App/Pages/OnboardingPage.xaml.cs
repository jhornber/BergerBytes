using BergerBytes.App.Services;
using BergerBytes.App.ViewModels;

namespace BergerBytes.App.Pages
{
    public partial class OnboardingPage : ContentPage
    {
        private readonly OnboardingViewModel _viewModel;

        public OnboardingPage(ISettingsRepository settingsRepository, IWeightLogRepository weightLogRepository)
        {
            InitializeComponent();
            _viewModel = new OnboardingViewModel(settingsRepository, weightLogRepository);
            BindingContext = _viewModel;
        }
    }
}
