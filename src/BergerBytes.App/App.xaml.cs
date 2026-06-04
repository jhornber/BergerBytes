using BergerBytes.App.Pages;
using BergerBytes.App.Services;

namespace BergerBytes.App
{
    public partial class App : Application
    {
        private readonly ISettingsRepository _settingsRepository;
        private readonly IWeightLogRepository _weightLogRepository;

        public App(ISettingsRepository settingsRepository, IWeightLogRepository weightLogRepository)
        {
            InitializeComponent();
            _settingsRepository = settingsRepository;
            _weightLogRepository = weightLogRepository;
            ApplyThemeAsync(settingsRepository);
        }

        private static async void ApplyThemeAsync(ISettingsRepository settingsRepository)
        {
            var settings = await settingsRepository.GetSettingsAsync();
            if (Application.Current != null)
                Application.Current.UserAppTheme = settings.DarkModeEnabled ? AppTheme.Dark : AppTheme.Light;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            // Task.Run forces work onto a thread pool thread, avoiding a UI-thread deadlock
            // while still blocking CreateWindow until we know which page to show first.
            var settings = Task.Run(() => _settingsRepository.GetSettingsAsync()).GetAwaiter().GetResult();

            if (!settings.OnboardingCompleted)
                return new Window(new OnboardingPage(_settingsRepository, _weightLogRepository));

            return new Window(new AppShell());
        }
    }
}