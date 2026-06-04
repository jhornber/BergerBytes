using System.Windows.Input;
using BergerBytes.App.Services;
using BergerBytes.Shared.Models;

namespace BergerBytes.App.ViewModels
{
    public class SettingsPageViewModel : BindableObject
    {
        private readonly ISettingsRepository _settingsRepository;
        private readonly IWeightLogRepository _weightLogRepository;
        private UserSettings _settings;
        private string _dailyCalorieGoalText = string.Empty;
        private string _dailyProteinGoalText = string.Empty;
        private string _dailyCarbsGoalText = string.Empty;
        private string _dailyFatGoalText = string.Empty;
        private bool _darkModeEnabled;
        private string _weightUnit = "kg";
        private string _weightGoalText = string.Empty;
        private string _weightGoalPaceText = string.Empty;
        private string _heightText = string.Empty;
        private string _ageText = string.Empty;
        private string _selectedSex = string.Empty;
        private string _selectedActivityLevel = "Not Active";
        private string _selectedDietPreset = "Balanced (1.0g protein/kg)";
        private double _currentWeightKg;
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

        public string WeightUnit
        {
            get => _weightUnit;
            set
            {
                if (_weightUnit == value) return;
                var oldUnit = _weightUnit;
                _weightUnit = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HeightUnit));

                // Convert displayed height when switching unit systems
                if (double.TryParse(_heightText, out double currentH) && currentH > 0)
                {
                    double heightCm = oldUnit == "lbs" ? currentH * 2.54 : currentH;
                    double newH = value == "lbs" ? heightCm / 2.54 : heightCm;
                    _heightText = newH.ToString("F1");
                    OnPropertyChanged(nameof(HeightText));
                }

                OnPropertyChanged(nameof(TdeeEstimateDisplay));
                OnPropertyChanged(nameof(HasTdeeEstimate));
                _settings.WeightUnit = value;
                _ = _settingsRepository.SaveSettingsAsync(_settings);
            }
        }

        public List<string> WeightUnits { get; } = new() { "kg", "lbs" };

        public string HeightText
        {
            get => _heightText;
            set
            {
                _heightText = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TdeeEstimateDisplay));
                OnPropertyChanged(nameof(HasTdeeEstimate));
                OnPropertyChanged(nameof(CanCalculateGoals));
            }
        }

        public string AgeText
        {
            get => _ageText;
            set
            {
                _ageText = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TdeeEstimateDisplay));
                OnPropertyChanged(nameof(HasTdeeEstimate));
                OnPropertyChanged(nameof(CanCalculateGoals));
            }
        }

        public string SelectedSex
        {
            get => _selectedSex;
            set
            {
                _selectedSex = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TdeeEstimateDisplay));
                OnPropertyChanged(nameof(HasTdeeEstimate));
                OnPropertyChanged(nameof(CanCalculateGoals));
            }
        }

        public string SelectedActivityLevel
        {
            get => _selectedActivityLevel;
            set
            {
                _selectedActivityLevel = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TdeeEstimateDisplay));
                OnPropertyChanged(nameof(HasTdeeEstimate));
            }
        }

        public List<string> SexOptions { get; } = new() { "Male", "Female" };
        public List<string> ActivityLevels { get; } = new() { "Not Active", "Lightly Active", "Active", "Very Active" };

        /// <summary>Display label for height entry (cm or in depending on weight unit).</summary>
        public string HeightUnit => _weightUnit == "lbs" ? "in" : "cm";

        /// <summary>Live TDEE estimate shown as a hint while the user fills in body metrics.</summary>
        public string TdeeEstimateDisplay
        {
            get
            {
                if (!double.TryParse(_heightText, out double heightDisplay) || heightDisplay <= 0) return string.Empty;
                if (!int.TryParse(_ageText, out int age) || age <= 0) return string.Empty;
                if (string.IsNullOrEmpty(_selectedSex)) return string.Empty;
                if (_currentWeightKg <= 0) return string.Empty;

                double heightCm = _weightUnit == "lbs" ? heightDisplay * 2.54 : heightDisplay;
                double bmr = (10 * _currentWeightKg) + (6.25 * heightCm) - (5 * age)
                             + (_selectedSex == "Male" ? 5 : -161);

                double multiplier = _selectedActivityLevel switch
                {
                    "Lightly Active" => 1.375,
                    "Active"         => 1.55,
                    "Very Active"    => 1.725,
                    _                => 1.2   // Not Active
                };
                double tdee = bmr * multiplier;
                return $"Est. daily calories burned: ~{tdee:F0}";
            }
        }

        public bool HasTdeeEstimate => !string.IsNullOrEmpty(TdeeEstimateDisplay);

        public string SelectedDietPreset
        {
            get => _selectedDietPreset;
            set { _selectedDietPreset = value; OnPropertyChanged(); }
        }

        public List<string> DietPresets { get; } = new()
        {
            "Balanced (1.0g protein/kg)",
            "High Protein (1.8g protein/kg)",
            "Endurance (1.2–1.8g protein/kg)",
            "Low Carb (1.6g protein/kg)"
        };

        /// <summary>True when all body metrics required for TDEE computation are present in the current form.</summary>
        public bool CanCalculateGoals
        {
            get
            {
                if (_currentWeightKg <= 0) return false;
                if (!double.TryParse(_heightText, out double h) || h <= 0) return false;
                if (!int.TryParse(_ageText, out int a) || a <= 0) return false;
                return !string.IsNullOrEmpty(_selectedSex);
            }
        }

        public string WeightGoalText
        {
            get => _weightGoalText;
            set
            {
                _weightGoalText = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(WeightGoalHint));
                OnPropertyChanged(nameof(HasWeightGoalHint));
            }
        }

        public string WeightGoalPaceText
        {
            get => _weightGoalPaceText;
            set
            {
                _weightGoalPaceText = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(WeightGoalHint));
                OnPropertyChanged(nameof(HasWeightGoalHint));
            }
        }

        public string WeightGoalHint => ComputeWeightGoalHint();
        public bool HasWeightGoalHint => !string.IsNullOrEmpty(WeightGoalHint);

        public ICommand SaveSettingsCommand { get; }
        public ICommand CalculateGoalsCommand { get; }

        public SettingsPageViewModel(ISettingsRepository settingsRepository, IWeightLogRepository weightLogRepository)
        {
            _settingsRepository = settingsRepository;
            _weightLogRepository = weightLogRepository;
            _settings = new UserSettings();
            SaveSettingsCommand = new Command(async () => await SaveSettingsAsync());
            CalculateGoalsCommand = new Command(async () => await CalculateGoalsAsync());
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

            _weightUnit = _settings.WeightUnit ?? "kg";
            OnPropertyChanged(nameof(WeightUnit));

            // Load current weight for goal validation
            var weightLogs = await _weightLogRepository.GetWeightLogsAsync();
            _currentWeightKg = weightLogs.OrderByDescending(w => w.LoggedAt).FirstOrDefault()?.WeightKg ?? 0;
            OnPropertyChanged(nameof(CanCalculateGoals));

            // Load weight goal (convert from kg to display unit)
            if (_settings.WeightGoalKg > 0)
            {
                double displayGoal = _weightUnit == "lbs"
                    ? _settings.WeightGoalKg / 0.45359237
                    : _settings.WeightGoalKg;
                _weightGoalText = displayGoal.ToString("F1");
                OnPropertyChanged(nameof(WeightGoalText));
            }
            if (_settings.WeightGoalPaceKgPerWeek > 0)
            {
                double displayPace = _weightUnit == "lbs"
                    ? _settings.WeightGoalPaceKgPerWeek / 0.45359237
                    : _settings.WeightGoalPaceKgPerWeek;
                _weightGoalPaceText = displayPace.ToString("F2");
                OnPropertyChanged(nameof(WeightGoalPaceText));
            }
            OnPropertyChanged(nameof(WeightGoalHint));
            OnPropertyChanged(nameof(HasWeightGoalHint));

            // Load body metrics
            if (_settings.HeightCm > 0)
            {
                double displayHeight = _weightUnit == "lbs" ? _settings.HeightCm / 2.54 : _settings.HeightCm;
                _heightText = displayHeight.ToString("F1");
                OnPropertyChanged(nameof(HeightText));
            }
            if (_settings.AgeYears > 0)
            {
                _ageText = _settings.AgeYears.ToString();
                OnPropertyChanged(nameof(AgeText));
            }
            _selectedSex = _settings.Sex ?? string.Empty;
            OnPropertyChanged(nameof(SelectedSex));
            _selectedActivityLevel = string.IsNullOrEmpty(_settings.ActivityLevel) ? "Not Active" : _settings.ActivityLevel;
            OnPropertyChanged(nameof(SelectedActivityLevel));
            OnPropertyChanged(nameof(HeightUnit));
            OnPropertyChanged(nameof(TdeeEstimateDisplay));
            OnPropertyChanged(nameof(HasTdeeEstimate));
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

                // Weight goal: validate pace then save
                double goalWeightDisplay = ParseGoalValue(WeightGoalText);
                double paceDisplay = ParseGoalValue(WeightGoalPaceText);
                double goalWeightKg = goalWeightDisplay > 0
                    ? (_weightUnit == "lbs" ? goalWeightDisplay * 0.45359237 : goalWeightDisplay)
                    : 0;
                double paceKgPerWeek = paceDisplay > 0
                    ? (_weightUnit == "lbs" ? paceDisplay * 0.45359237 : paceDisplay)
                    : 0;

                if (paceKgPerWeek > 2.27) // > 5 lbs/week is unsafe
                {
                    double paceLbs = paceKgPerWeek / 0.45359237;
                    await Application.Current!.MainPage!.DisplayAlertAsync(
                        "Pace Too Aggressive",
                        $"That requires ~{paceLbs:F1} lbs/week, which isn't safe. Try 0.5–1.0 kg/week (1–2 lbs/week).",
                        "OK");
                    return;
                }

                _settings.WeightGoalKg = goalWeightKg;
                _settings.WeightGoalPaceKgPerWeek = paceKgPerWeek;

                // Body metrics
                if (double.TryParse(_heightText, out double heightDisplay) && heightDisplay > 0)
                    _settings.HeightCm = _weightUnit == "lbs" ? heightDisplay * 2.54 : heightDisplay;
                else
                    _settings.HeightCm = 0;

                _settings.AgeYears = int.TryParse(_ageText, out int ageVal) && ageVal > 0 ? ageVal : 0;
                _settings.Sex = _selectedSex ?? string.Empty;
                _settings.ActivityLevel = string.IsNullOrEmpty(_selectedActivityLevel) ? "Not Active" : _selectedActivityLevel;

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

        private async Task CalculateGoalsAsync()
        {
            if (!CanCalculateGoals) return;

            // Re-compute TDEE from current form values (user may not have saved yet)
            double.TryParse(_heightText, out double heightDisplay);
            int.TryParse(_ageText, out int age);
            double heightCm = _weightUnit == "lbs" ? heightDisplay * 2.54 : heightDisplay;

            double bmr = (10 * _currentWeightKg) + (6.25 * heightCm) - (5 * age)
                         + (_selectedSex == "Male" ? 5 : -161);

            double multiplier = _selectedActivityLevel switch
            {
                "Lightly Active" => 1.375,
                "Active"         => 1.55,
                "Very Active"    => 1.725,
                _                => 1.2   // Not Active
            };
            double tdee = bmr * multiplier;

            // Adjust for weight goal if set
            double dailyCalories = tdee;
            double goalWeightDisplay = ParseGoalValue(WeightGoalText);
            double paceDisplay2 = ParseGoalValue(WeightGoalPaceText);
            if (goalWeightDisplay > 0 && paceDisplay2 > 0)
            {
                double goalWeightKg2 = _weightUnit == "lbs" ? goalWeightDisplay * 0.45359237 : goalWeightDisplay;
                double diffKg = _currentWeightKg - goalWeightKg2;
                double paceKgPerWeek2 = _weightUnit == "lbs" ? paceDisplay2 * 0.45359237 : paceDisplay2;
                double sign = diffKg > 0 ? 1.0 : -1.0;
                double dailyDelta = sign * paceKgPerWeek2 * 7700.0 / 7.0; // + = deficit
                dailyCalories = tdee - dailyDelta;
                double floor = _selectedSex == "Male" ? 1500 : 1200;
                dailyCalories = Math.Max(floor, dailyCalories);
            }
            dailyCalories = Math.Round(dailyCalories);

            // Apply macro split — protein is always anchored to body weight since protein needs
            // scale with lean mass, not calorie intake. Fat is a percentage of calories.
            // Carbs fill the remainder.
            int proteinG, carbsG, fatG;
            if (_selectedDietPreset.StartsWith("Endurance"))
            {
                // Baseline training: 1.3 g/kg (midpoint of 1.2–1.4 range)
                // Deficit: 1.7 g/kg (midpoint of 1.4–1.8 range) to prevent catabolism
                bool isDeficit = goalWeightDisplay > 0
                                 && (_currentWeightKg - (_weightUnit == "lbs" ? goalWeightDisplay * 0.45359237 : goalWeightDisplay)) > 0.1;
                double proteinPerKg = isDeficit ? 1.7 : 1.3;
                proteinG = (int)Math.Round(_currentWeightKg * proteinPerKg);
                fatG     = (int)Math.Round((dailyCalories * 0.25) / 9);
                double remaining = dailyCalories - (proteinG * 4) - (fatG * 9);
                carbsG   = (int)Math.Round(Math.Max(0, remaining) / 4);
            }
            else
            {
                var (proteinPerKg, fatPct) = GetPresetParams();
                proteinG = (int)Math.Round(_currentWeightKg * proteinPerKg);
                fatG     = (int)Math.Round((dailyCalories * fatPct) / 9);
                double remaining = dailyCalories - (proteinG * 4) - (fatG * 9);
                carbsG   = (int)Math.Round(Math.Max(0, remaining) / 4);
            }

            // Enforce minimum protein floor: 1.0g/kg body weight (practical app minimum; RDA is 0.8g/kg).
            // Only relevant for percentage-based presets at very low calorie targets.
            // If protein falls short, carbs absorb the shortfall since they are the most flexible macro.
            int minProteinG = (int)Math.Round(_currentWeightKg * 1.0);
            if (proteinG < minProteinG)
            {
                int proteinBump = minProteinG - proteinG;
                proteinG = minProteinG;
                // Reduce carbs to keep total calories constant; clamp at zero
                int carbsReduction = Math.Min(carbsG, (int)Math.Round(proteinBump * 4.0 / 4.0));
                carbsG = Math.Max(0, carbsG - carbsReduction);
            }

            // Populate the goal fields
            DailyCalorieGoalText = dailyCalories.ToString("F0");
            DailyProteinGoalText = proteinG.ToString();
            DailyCarbsGoalText   = carbsG.ToString();
            DailyFatGoalText     = fatG.ToString();

            await Application.Current!.MainPage!.DisplayAlertAsync(
                "Goals Calculated",
                $"{dailyCalories:F0} kcal  ·  {proteinG}g protein  ·  {carbsG}g carbs  ·  {fatG}g fat\n\nReview the values and tap Save Settings to keep them.",
                "OK");
        }

        /// <summary>Returns the (protein g/kg, fat% of calories) for non-Endurance presets.
        /// Protein is body-weight-anchored; fat scales with calories; carbs fill the remainder.</summary>
        private (double proteinPerKg, double fatPct) GetPresetParams() => _selectedDietPreset switch
        {
            "High Protein (1.8g protein/kg)" => (1.8, 0.25),
            "Low Carb (1.6g protein/kg)"     => (1.6, 0.45),
            _                                => (1.0, 0.30), // Balanced — RDA-adjacent
        };

        private double ParseGoalValue(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return 0;
            if (double.TryParse(text, out double value))
                return value < 0 ? 0 : value;
            return 0;
        }

        private string ComputeWeightGoalHint()
        {
            if (!double.TryParse(WeightGoalText, out double goalDisplay) || goalDisplay <= 0)
                return string.Empty;
            if (!double.TryParse(WeightGoalPaceText, out double paceDisplay) || paceDisplay <= 0)
                return string.Empty;
            if (_currentWeightKg <= 0)
                return string.Empty;

            double goalKg = _weightUnit == "lbs" ? goalDisplay * 0.45359237 : goalDisplay;
            double paceKgPerWeek = _weightUnit == "lbs" ? paceDisplay * 0.45359237 : paceDisplay;
            double diffKg = _currentWeightKg - goalKg;

            if (Math.Abs(diffKg) < 0.1)
                return "Your current weight already matches this goal.";

            int estimatedWeeks = (int)Math.Ceiling(Math.Abs(diffKg) / paceKgPerWeek);
            string direction = diffKg > 0 ? "lose" : "gain";

            if (paceKgPerWeek <= 0.45)   // ≤ 1 lb/week
                return $"Gentle pace — ~{estimatedWeeks} weeks to {direction} {Math.Abs(diffKg):F1} kg. Very sustainable!";
            if (paceKgPerWeek <= 0.91)   // ≤ 2 lbs/week
                return $"Healthy pace — ~{estimatedWeeks} weeks to goal. Great choice!";
            if (paceKgPerWeek <= 2.27)   // ≤ 5 lbs/week
                return $"Aggressive — ~{estimatedWeeks} weeks. Requires real discipline.";

            return $"⚠ Too fast! Maximum recommended is 1 kg/week (2 lbs/week).";
        }
    }
}
