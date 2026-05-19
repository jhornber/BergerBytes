using System.Windows.Input;
using BergerBytes.App.Services;
using BergerBytes.Shared.Models;

namespace BergerBytes.App.ViewModels
{
    public class SettingsPageViewModel : BindableObject
    {
        private readonly ISettingsRepository _settingsRepository;
        private UserSettings _settings;
        private string _dailyCalorieGoalText = string.Empty;
        private string _dailyProteinGoalText = string.Empty;
        private string _dailyCarbsGoalText = string.Empty;
        private string _dailyFatGoalText = string.Empty;
        private bool _darkModeEnabled;
        private bool _isSaving;

        public string DailyCalorieGoalText
        {
            get => _dailyCalorieGoalText;
            set
            {
                _dailyCalorieGoalText = value;
                OnPropertyChanged();
            }
        }

        public string DailyProteinGoalText
        {
            get => _dailyProteinGoalText;
            set
            {
                _dailyProteinGoalText = value;
                OnPropertyChanged();
            }
        }

        public string DailyCarbsGoalText
        {
            get => _dailyCarbsGoalText;
            set
            {
                _dailyCarbsGoalText = value;
                OnPropertyChanged();
            }
        }

        public string DailyFatGoalText
        {
            get => _dailyFatGoalText;
            set
            {
                _dailyFatGoalText = value;
                OnPropertyChanged();
            }
        }

        public bool DarkModeEnabled
        {
            get => _darkModeEnabled;
            set
            {
                _darkModeEnabled = value;
                OnPropertyChanged();
                if (Application.Current != null)
                    Application.Current.UserAppTheme = value ? AppTheme.Dark : AppTheme.Light;
                _settings.DarkModeEnabled = value;
                _ = _settingsRepository.SaveSettingsAsync(_settings);
            }
        }

        public bool IsSaving
        {
            get => _isSaving;
            set
            {
                _isSaving = value;
                OnPropertyChanged();
            }
        }

        public ICommand SaveSettingsCommand { get; }

        public SettingsPageViewModel(ISettingsRepository settingsRepository)
        {
            _settingsRepository = settingsRepository;
            _settings = new UserSettings();
            SaveSettingsCommand = new Command(async () => await SaveSettingsAsync());
        }

        public async Task InitializeAsync()
        {
            _settings = await _settingsRepository.GetSettingsAsync();

            DailyCalorieGoalText = _settings.DailyCalorieGoal > 0 ? _settings.DailyCalorieGoal.ToString("F0") : string.Empty;
            DailyProteinGoalText = _settings.DailyProteinGoal > 0 ? _settings.DailyProteinGoal.ToString("F0") : string.Empty;
            DailyCarbsGoalText = _settings.DailyCarbsGoal > 0 ? _settings.DailyCarbsGoal.ToString("F0") : string.Empty;
            DailyFatGoalText = _settings.DailyFatGoal > 0 ? _settings.DailyFatGoal.ToString("F0") : string.Empty;

            // Set backing field directly to apply theme on load without triggering a redundant save
            _darkModeEnabled = _settings.DarkModeEnabled;
            OnPropertyChanged(nameof(DarkModeEnabled));
            if (Application.Current != null)
                Application.Current.UserAppTheme = _darkModeEnabled ? AppTheme.Dark : AppTheme.Light;
        }

        private async Task SaveSettingsAsync()
        {
            IsSaving = true;

            try
            {
                _settings.DailyCalorieGoal = ParseGoalValue(DailyCalorieGoalText);
                _settings.DailyProteinGoal = ParseGoalValue(DailyProteinGoalText);
                _settings.DailyCarbsGoal = ParseGoalValue(DailyCarbsGoalText);
                _settings.DailyFatGoal = ParseGoalValue(DailyFatGoalText);

                await _settingsRepository.SaveSettingsAsync(_settings);

                // Show success message
                await Application.Current!.MainPage!.DisplayAlertAsync("Success", "Settings saved successfully!", "OK");
            }
            catch (Exception ex)
            {
                await Application.Current!.MainPage!.DisplayAlertAsync("Error", $"Failed to save settings: {ex.Message}", "OK");
            }
            finally
            {
                IsSaving = false;
            }
        }

        private double ParseGoalValue(string text)
        {
            // If empty or whitespace, return 0
            if (string.IsNullOrWhiteSpace(text))
                return 0;

            // Try to parse the value
            if (double.TryParse(text, out double value))
            {
                // If negative, return 0
                return value < 0 ? 0 : value;
            }

            // If parsing fails, return 0
            return 0;
        }
    }
}
