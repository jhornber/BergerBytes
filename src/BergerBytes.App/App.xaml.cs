using BergerBytes.App.Services;

namespace BergerBytes.App
{
    public partial class App : Application
    {
        public App(ISettingsRepository settingsRepository)
        {
            InitializeComponent();
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
            return new Window(new AppShell());
        }
    }
}