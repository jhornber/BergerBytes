using System.Windows.Input;
using BergerBytes.App.Services;
using BergerBytes.Shared.Models;

namespace BergerBytes.App.ViewModels
{
    /// <summary>A label/value pair for the onboarding summary screen.</summary>
    public record SummaryItem(string Label, string Value);

    /// <summary>
    /// ViewModel for the first-launch onboarding wizard.
    /// Steps: 0=Welcome, 1=Units, 2=CurrentWeight, 3=BodyMetrics, 4=Activity, 5=MacroSplit, 6=WeightGoal, 7=Done
    /// </summary>
    public class OnboardingViewModel : BindableObject
    {
        private readonly ISettingsRepository _settingsRepository;
        private readonly IWeightLogRepository _weightLogRepository;

        private int _currentStep = 0;
        private const int TotalSteps = 8;

        // ── Step 1: Units ────────────────────────────────────────────────────
        private string _weightUnit = "kg";

        // ── Step 2: Current Weight ───────────────────────────────────────────
        private string _currentWeightText = string.Empty;

        // ── Step 3: Body Metrics ─────────────────────────────────────────────
        private string _ageText = string.Empty;
        private string _selectedSex = string.Empty;
        private string _heightText = string.Empty;

        // ── Step 4: Activity Level ───────────────────────────────────────────
        private string _selectedActivityLevel = "Not Active";

        // ── Step 5: Macro Split ──────────────────────────────────────────────
        private string _selectedDietPreset = "Balanced (1.0g protein/kg)";

        // ── Step 6: Weight Goal ──────────────────────────────────────────────
        private bool _isMaintainMode = false;
        private string _weightGoalText = string.Empty;
        private string _weightGoalPaceText = string.Empty;

        // ── Step 7: Done ─────────────────────────────────────────────────────
        private bool _isSaving = false;

        // ── Navigation ───────────────────────────────────────────────────────
        public int CurrentStep
        {
            get => _currentStep;
            private set
            {
                _currentStep = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsStep0));
                OnPropertyChanged(nameof(IsStep1));
                OnPropertyChanged(nameof(IsStep2));
                OnPropertyChanged(nameof(IsStep3));
                OnPropertyChanged(nameof(IsStep4));
                OnPropertyChanged(nameof(IsStep5));
                OnPropertyChanged(nameof(IsStep6));
                OnPropertyChanged(nameof(IsStep7));
                OnPropertyChanged(nameof(IsMiddleStep));
                OnPropertyChanged(nameof(IsFinalStep));
                OnPropertyChanged(nameof(Progress));
                OnPropertyChanged(nameof(StepIndicatorText));
                OnPropertyChanged(nameof(CanGoNext));
                OnPropertyChanged(nameof(TdeeEstimateDisplay));
                OnPropertyChanged(nameof(HasTdeeEstimate));
                OnPropertyChanged(nameof(SummaryText));
                OnPropertyChanged(nameof(SummaryRows));
            }
        }

        public bool IsStep0 => _currentStep == 0;
        public bool IsStep1 => _currentStep == 1;
        public bool IsStep2 => _currentStep == 2;
        public bool IsStep3 => _currentStep == 3;
        public bool IsStep4 => _currentStep == 4;
        public bool IsStep5 => _currentStep == 5;
        public bool IsStep6 => _currentStep == 6;
        public bool IsStep7 => _currentStep == 7;

        /// <summary>True for steps 1–6 (back + next buttons visible).</summary>
        public bool IsMiddleStep => _currentStep >= 1 && _currentStep <= 6;
        /// <summary>True only on the final Done screen.</summary>
        public bool IsFinalStep => _currentStep == 7;

        public double Progress => TotalSteps <= 1 ? 1.0 : (double)_currentStep / (TotalSteps - 1);
        public string StepIndicatorText => _currentStep == 0 ? string.Empty : $"Step {_currentStep} of {TotalSteps - 1}";

        // ── Step 1 properties ────────────────────────────────────────────────
        public string WeightUnit
        {
            get => _weightUnit;
            set
            {
                if (_weightUnit == value) return;
                _weightUnit = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(WeightUnitLabel));
                OnPropertyChanged(nameof(HeightUnit));
                OnPropertyChanged(nameof(TdeeEstimateDisplay));
                OnPropertyChanged(nameof(HasTdeeEstimate));
                OnPropertyChanged(nameof(WeightGoalHint));
                OnPropertyChanged(nameof(HasWeightGoalHint));
            }
        }

        public List<string> WeightUnits { get; } = new() { "kg", "lbs" };
        public string WeightUnitLabel => _weightUnit;

        // ── Step 2 properties ────────────────────────────────────────────────
        public string CurrentWeightText
        {
            get => _currentWeightText;
            set
            {
                _currentWeightText = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanGoNext));
                OnPropertyChanged(nameof(TdeeEstimateDisplay));
                OnPropertyChanged(nameof(HasTdeeEstimate));
                OnPropertyChanged(nameof(WeightGoalHint));
                OnPropertyChanged(nameof(HasWeightGoalHint));
            }
        }

        // ── Step 3 properties ────────────────────────────────────────────────
        public string AgeText
        {
            get => _ageText;
            set
            {
                _ageText = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanGoNext));
                OnPropertyChanged(nameof(TdeeEstimateDisplay));
                OnPropertyChanged(nameof(HasTdeeEstimate));
            }
        }

        public string SelectedSex
        {
            get => _selectedSex;
            set
            {
                _selectedSex = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanGoNext));
                OnPropertyChanged(nameof(TdeeEstimateDisplay));
                OnPropertyChanged(nameof(HasTdeeEstimate));
            }
        }

        public List<string> SexOptions { get; } = new() { "Male", "Female" };

        public string HeightText
        {
            get => _heightText;
            set
            {
                _heightText = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanGoNext));
                OnPropertyChanged(nameof(TdeeEstimateDisplay));
                OnPropertyChanged(nameof(HasTdeeEstimate));
            }
        }

        /// <summary>Display label for height entry (cm or in depending on unit).</summary>
        public string HeightUnit => _weightUnit == "lbs" ? "in" : "cm";

        // ── Step 4 properties ────────────────────────────────────────────────
        public string SelectedActivityLevel
        {
            get => _selectedActivityLevel;
            set
            {
                _selectedActivityLevel = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ActivityLevelDescription));
                OnPropertyChanged(nameof(TdeeEstimateDisplay));
                OnPropertyChanged(nameof(HasTdeeEstimate));
            }
        }

        public List<string> ActivityLevels { get; } = new()
        {
            "Not Active",
            "Lightly Active",
            "Active",
            "Very Active"
        };

        public string ActivityLevelDescription => _selectedActivityLevel switch
        {
            "Lightly Active" => "On your feet most of the day — think teacher, retail worker, or salesperson.",
            "Active"         => "Physical activity for a good part of the day — server, landscaper, or active tradesperson.",
            "Very Active"    => "Heavy physical work most of the day — bike messenger, construction worker, or farmer.",
            _                => "Most of the day seated — desk job, student, or working from home."
        };

        // ── Step 5 properties ────────────────────────────────────────────────
        public string SelectedDietPreset
        {
            get => _selectedDietPreset;
            set
            {
                _selectedDietPreset = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DietPresetDescription));
            }
        }

        public List<string> DietPresets { get; } = new()
        {
            "Balanced (1.0g protein/kg)",
            "High Protein (1.8g protein/kg)",
            "Endurance (1.2–1.8g protein/kg)",
            "Low Carb (1.6g protein/kg)"
        };

        public string DietPresetDescription => _selectedDietPreset switch
        {
            "High Protein (1.8g protein/kg)" => "Maximizes muscle retention during a cut. Great for strength athletes.",
            "Endurance (1.2–1.8g protein/kg)"  => "Higher protein to repair and rebuild after training. Scales up automatically in a deficit to prevent muscle breakdown.",
            "Low Carb (1.6g protein/kg)"     => "Reduced carbs with higher fat. Good for metabolic flexibility.",
            _                                => "A solid all-rounder: 30% fat, carbs fill the rest. Great starting point."
        };

        // ── Step 6 properties ────────────────────────────────────────────────
        public bool IsMaintainMode
        {
            get => _isMaintainMode;
            set
            {
                _isMaintainMode = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowGoalInputs));
                OnPropertyChanged(nameof(CanGoNext));
                OnPropertyChanged(nameof(WeightGoalHint));
                OnPropertyChanged(nameof(HasWeightGoalHint));
            }
        }

        public bool ShowGoalInputs => !_isMaintainMode;

        public string WeightGoalText
        {
            get => _weightGoalText;
            set
            {
                _weightGoalText = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanGoNext));
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
                OnPropertyChanged(nameof(CanGoNext));
                OnPropertyChanged(nameof(WeightGoalHint));
                OnPropertyChanged(nameof(HasWeightGoalHint));
            }
        }

        public string WeightGoalHint => ComputeWeightGoalHint();
        public bool HasWeightGoalHint => !string.IsNullOrEmpty(WeightGoalHint);

        // ── Step 7 properties ────────────────────────────────────────────────
        public bool IsSaving
        {
            get => _isSaving;
            set { _isSaving = value; OnPropertyChanged(); }
        }

        public string SummaryText => BuildSummary();

        public List<SummaryItem> SummaryRows => BuildSummaryRows();

        // ── Computed cross-step helpers ──────────────────────────────────────

        /// <summary>Live TDEE estimate shown on step 4 once body metrics are filled in.</summary>
        public string TdeeEstimateDisplay
        {
            get
            {
                if (!TryGetCurrentWeightKg(out double weightKg) || weightKg <= 0) return string.Empty;
                if (!double.TryParse(_heightText, out double heightDisplay) || heightDisplay <= 0) return string.Empty;
                if (!int.TryParse(_ageText, out int age) || age <= 0) return string.Empty;
                if (string.IsNullOrEmpty(_selectedSex)) return string.Empty;

                double heightCm = _weightUnit == "lbs" ? heightDisplay * 2.54 : heightDisplay;
                double bmr = (10 * weightKg) + (6.25 * heightCm) - (5 * age)
                             + (_selectedSex == "Male" ? 5 : -161);
                double multiplier = _selectedActivityLevel switch
                {
                    "Lightly Active" => 1.375,
                    "Active"         => 1.55,
                    "Very Active"    => 1.725,
                    _                => 1.2   // Not Active
                };
                double tdee = bmr * multiplier;
                return $"~{tdee:F0}";
            }
        }

        public bool HasTdeeEstimate => !string.IsNullOrEmpty(TdeeEstimateDisplay);

        /// <summary>Returns true when enough data exists to validate Step 6 (goal).</summary>
        public bool CanGoNext
        {
            get
            {
                return _currentStep switch
                {
                    0 => true,
                    1 => true,
                    2 => double.TryParse(_currentWeightText, out double w) && w > 0,
                    3 => int.TryParse(_ageText, out int a) && a > 0
                         && !string.IsNullOrEmpty(_selectedSex)
                         && double.TryParse(_heightText, out double h) && h > 0,
                    4 => true,
                    5 => true,
                    6 => _isMaintainMode
                         || (double.TryParse(_weightGoalText, out double g) && g > 0
                             && double.TryParse(_weightGoalPaceText, out double p) && p > 0),
                    _ => false
                };
            }
        }

        // ── Commands ─────────────────────────────────────────────────────────
        public ICommand NextCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand FinishCommand { get; }
        public ICommand SelectKgCommand { get; }
        public ICommand SelectLbsCommand { get; }
        public ICommand SelectMaleCommand { get; }
        public ICommand SelectFemaleCommand { get; }

        public OnboardingViewModel(ISettingsRepository settingsRepository, IWeightLogRepository weightLogRepository)
        {
            _settingsRepository = settingsRepository;
            _weightLogRepository = weightLogRepository;

            NextCommand  = new Command(GoNext,  () => CanGoNext);
            BackCommand  = new Command(GoBack);
            FinishCommand = new Command(async () => await FinishAsync(), () => !_isSaving);
            SelectKgCommand    = new Command(() => WeightUnit = "kg");
            SelectLbsCommand   = new Command(() => WeightUnit = "lbs");
            SelectMaleCommand  = new Command(() => SelectedSex = "Male");
            SelectFemaleCommand = new Command(() => SelectedSex = "Female");
        }

        private void GoNext()
        {
            if (_currentStep < TotalSteps - 1 && CanGoNext)
            {
                CurrentStep++;
                ((Command)NextCommand).ChangeCanExecute();
            }
        }

        private void GoBack()
        {
            if (_currentStep > 0)
                CurrentStep--;
        }

        private async Task FinishAsync()
        {
            IsSaving = true;
            try
            {
                var settings = await _settingsRepository.GetSettingsAsync();

                // ── Units ──────────────────────────────────────────────────
                settings.WeightUnit = _weightUnit;

                // ── Body metrics ───────────────────────────────────────────
                settings.AgeYears = int.TryParse(_ageText, out int age) && age > 0 ? age : 0;
                settings.Sex = _selectedSex ?? string.Empty;

                if (double.TryParse(_heightText, out double heightDisplay) && heightDisplay > 0)
                    settings.HeightCm = _weightUnit == "lbs" ? heightDisplay * 2.54 : heightDisplay;

                settings.ActivityLevel = string.IsNullOrEmpty(_selectedActivityLevel) ? "Not Active" : _selectedActivityLevel;

                // ── Weight goal ────────────────────────────────────────────
                TryGetCurrentWeightKg(out double currentWeightKg);
                if (_isMaintainMode)
                {
                    settings.WeightGoalKg = currentWeightKg;
                    settings.WeightGoalPaceKgPerWeek = 0;
                }
                else
                {
                    if (double.TryParse(_weightGoalText, out double goalDisplay) && goalDisplay > 0)
                        settings.WeightGoalKg = _weightUnit == "lbs" ? goalDisplay * 0.45359237 : goalDisplay;

                    if (double.TryParse(_weightGoalPaceText, out double paceDisplay) && paceDisplay > 0)
                        settings.WeightGoalPaceKgPerWeek = _weightUnit == "lbs" ? paceDisplay * 0.45359237 : paceDisplay;
                }

                // ── Calculated macro goals ─────────────────────────────────
                if (currentWeightKg > 0 && settings.HasCompleteProfile)
                {
                    double tdee = settings.CalculateTdee(currentWeightKg);
                    double dailyCalories = tdee;

                    if (!_isMaintainMode && settings.WeightGoalPaceKgPerWeek > 0)
                    {
                        double diffKg = currentWeightKg - settings.WeightGoalKg;
                        double sign = diffKg > 0 ? 1.0 : -1.0;
                        double dailyDelta = sign * settings.WeightGoalPaceKgPerWeek * 7700.0 / 7.0;
                        dailyCalories = tdee - dailyDelta;
                        double floor = settings.Sex == "Male" ? 1500 : 1200;
                        dailyCalories = Math.Max(floor, dailyCalories);
                    }
                    dailyCalories = Math.Round(dailyCalories);

                    int proteinG, carbsG, fatG;
                    if (_selectedDietPreset.StartsWith("Endurance"))
                    {
                        // Baseline training: 1.3 g/kg (midpoint of 1.2–1.4 range)
                        // Deficit: 1.7 g/kg (midpoint of 1.4–1.8 range) to prevent catabolism
                        bool isDeficit = !_isMaintainMode && settings.WeightGoalPaceKgPerWeek > 0
                                         && (currentWeightKg - settings.WeightGoalKg) > 0.1;
                        double proteinPerKg = isDeficit ? 1.7 : 1.3;
                        proteinG = (int)Math.Round(currentWeightKg * proteinPerKg);
                        fatG     = (int)Math.Round((dailyCalories * 0.25) / 9);
                        carbsG   = (int)Math.Round(Math.Max(0, dailyCalories - (proteinG * 4) - (fatG * 9)) / 4);
                    }
                    else
                    {
                        var (proteinPerKg, fatPct) = GetPresetParams();
                        proteinG = (int)Math.Round(currentWeightKg * proteinPerKg);
                        fatG     = (int)Math.Round((dailyCalories * fatPct) / 9);
                        carbsG   = (int)Math.Round(Math.Max(0, dailyCalories - (proteinG * 4) - (fatG * 9)) / 4);
                    }

                    int minProteinG = (int)Math.Round(currentWeightKg * 1.0);
                    if (proteinG < minProteinG)
                    {
                        int bump = minProteinG - proteinG;
                        proteinG = minProteinG;
                        carbsG   = Math.Max(0, carbsG - bump);
                    }

                    settings.DailyCalorieGoal = dailyCalories;
                    settings.DailyProteinGoal = proteinG;
                    settings.DailyCarbsGoal   = carbsG;
                    settings.DailyFatGoal     = fatG;
                }

                settings.OnboardingCompleted = true;
                await _settingsRepository.SaveSettingsAsync(settings);

                // ── Save initial weight log ────────────────────────────────
                if (currentWeightKg > 0)
                {
                    double displayWeight = double.TryParse(_currentWeightText, out double dw) ? dw : currentWeightKg;
                    var weightLog = new WeightLog
                    {
                        WeightKg      = currentWeightKg,
                        DisplayWeight = displayWeight,
                        Unit          = _weightUnit,
                        LoggedAt      = DateTime.Now
                    };
                    await _weightLogRepository.SaveWeightLogAsync(weightLog);
                }

                // ── Navigate to main app ───────────────────────────────────
                if (Application.Current?.Windows.Count > 0)
                    Application.Current.Windows[0].Page = new AppShell();
            }
            catch (Exception ex)
            {
                var page = Application.Current?.Windows.Count > 0 ? Application.Current.Windows[0].Page : null;
                if (page != null)
                    await page.DisplayAlertAsync("Oops!", $"Something went wrong: {ex.Message}", "OK");
            }
            finally
            {
                IsSaving = false;
            }
        }

        // ── Private helpers ───────────────────────────────────────────────────

        private bool TryGetCurrentWeightKg(out double weightKg)
        {
            if (!double.TryParse(_currentWeightText, out double displayWeight) || displayWeight <= 0)
            {
                weightKg = 0;
                return false;
            }
            weightKg = _weightUnit == "lbs" ? displayWeight * 0.45359237 : displayWeight;
            return true;
        }

        private string ComputeWeightGoalHint()
        {
            if (_isMaintainMode) return "You'll maintain your current weight. No calorie deficit needed!";
            if (!double.TryParse(_weightGoalText, out double goalDisplay) || goalDisplay <= 0)
                return string.Empty;
            if (!double.TryParse(_weightGoalPaceText, out double paceDisplay) || paceDisplay <= 0)
                return string.Empty;
            if (!TryGetCurrentWeightKg(out double currentWeightKg) || currentWeightKg <= 0)
                return string.Empty;

            double goalKg = _weightUnit == "lbs" ? goalDisplay * 0.45359237 : goalDisplay;
            double paceKgPerWeek = _weightUnit == "lbs" ? paceDisplay * 0.45359237 : paceDisplay;
            double diffKg = currentWeightKg - goalKg;

            if (Math.Abs(diffKg) < 0.1)
                return "Your goal weight matches your current weight. Switch to Maintain!";

            if (paceKgPerWeek > 2.27)
                return "⚠ Too fast! Maximum safe pace is ~1 kg/week (2 lbs/week).";

            int estimatedWeeks = (int)Math.Ceiling(Math.Abs(diffKg) / paceKgPerWeek);
            string direction = diffKg > 0 ? "lose" : "gain";

            if (paceKgPerWeek <= 0.45)
                return $"Gentle — ~{estimatedWeeks} weeks to {direction} {Math.Abs(diffKg):F1} kg. Very sustainable!";
            if (paceKgPerWeek <= 0.91)
                return $"Healthy pace — ~{estimatedWeeks} weeks to goal. Great choice!";
            return $"Aggressive — ~{estimatedWeeks} weeks. Requires real discipline.";
        }

        private string BuildSummary()
        {
            var lines = new List<string>();

            if (!string.IsNullOrEmpty(_weightUnit))
                lines.Add($"Units: {_weightUnit}");

            if (TryGetCurrentWeightKg(out double wKg) && wKg > 0)
            {
                double display = _weightUnit == "lbs" ? wKg / 0.45359237 : wKg;
                lines.Add($"Current weight: {display:F1} {_weightUnit}");
            }

            if (int.TryParse(_ageText, out int a) && a > 0)
                lines.Add($"Age: {a}");
            if (!string.IsNullOrEmpty(_selectedSex))
                lines.Add($"Sex: {_selectedSex}");
            if (double.TryParse(_heightText, out double h) && h > 0)
                lines.Add($"Height: {h:F0} {HeightUnit}");
            if (!string.IsNullOrEmpty(_selectedActivityLevel))
                lines.Add($"Activity: {_selectedActivityLevel}");
            if (!string.IsNullOrEmpty(_selectedDietPreset))
                lines.Add($"Macro preset: {_selectedDietPreset}");

            if (_isMaintainMode)
            {
                lines.Add("Goal: Maintain current weight");
            }
            else if (double.TryParse(_weightGoalText, out double gd) && gd > 0)
            {
                lines.Add($"Target weight: {gd:F1} {_weightUnit}");
                if (double.TryParse(_weightGoalPaceText, out double pd) && pd > 0)
                    lines.Add($"Pace: {pd:F2} {_weightUnit}/week");
            }

            if (!string.IsNullOrEmpty(TdeeEstimateDisplay))
                lines.Add(TdeeEstimateDisplay);

            return string.Join("\n", lines);
        }

        private List<SummaryItem> BuildSummaryRows()
        {
            var rows = new List<SummaryItem>();

            rows.Add(new SummaryItem("Units", _weightUnit));

            if (TryGetCurrentWeightKg(out double wKg) && wKg > 0)
            {
                double display = _weightUnit == "lbs" ? wKg / 0.45359237 : wKg;
                rows.Add(new SummaryItem("Current Weight", $"{display:F1} {_weightUnit}"));
            }

            if (int.TryParse(_ageText, out int a) && a > 0)
                rows.Add(new SummaryItem("Age", $"{a} years"));
            if (!string.IsNullOrEmpty(_selectedSex))
                rows.Add(new SummaryItem("Sex", _selectedSex));
            if (double.TryParse(_heightText, out double h) && h > 0)
                rows.Add(new SummaryItem("Height", $"{h:F0} {HeightUnit}"));
            if (!string.IsNullOrEmpty(_selectedActivityLevel))
                rows.Add(new SummaryItem("Activity Level", _selectedActivityLevel));
            if (!string.IsNullOrEmpty(_selectedDietPreset))
                rows.Add(new SummaryItem("Macro Preset", _selectedDietPreset.Split('(')[0].Trim()));

            if (_isMaintainMode)
            {
                rows.Add(new SummaryItem("Goal", "Maintain weight"));
            }
            else if (double.TryParse(_weightGoalText, out double gd) && gd > 0)
            {
                rows.Add(new SummaryItem("Target Weight", $"{gd:F1} {_weightUnit}"));
                if (double.TryParse(_weightGoalPaceText, out double pd) && pd > 0)
                    rows.Add(new SummaryItem("Weekly Pace", $"{pd:F2} {_weightUnit}/wk"));
            }

            if (!string.IsNullOrEmpty(TdeeEstimateDisplay))
                rows.Add(new SummaryItem("Daily Calorie Burn", TdeeEstimateDisplay));

            return rows;
        }

        private (double proteinPerKg, double fatPct) GetPresetParams() => _selectedDietPreset switch
        {
            "High Protein (1.8g protein/kg)" => (1.8, 0.25),
            "Low Carb (1.6g protein/kg)"     => (1.6, 0.45),
            _                                => (1.0, 0.30),
        };
    }
}
