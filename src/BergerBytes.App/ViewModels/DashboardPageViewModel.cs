using System.Windows.Input;
using BergerBytes.App.Services;
using BergerBytes.Shared.Models;
using Microsoft.Maui.Graphics;

namespace BergerBytes.App.ViewModels
{
    public class DashboardPageViewModel : BindableObject
    {
        private readonly IMealLogRepository _repository;
        private readonly ISettingsRepository _settingsRepository;
        private readonly IWeightLogRepository _weightLogRepository;
        private readonly IExerciseLogRepository _exerciseLogRepository;
        private bool _isRefreshing;
        private double _caloriesConsumed;
        private double _caloriesBurned;
        private double _proteinConsumed;
        private double _carbsConsumed;
        private double _fatConsumed;
        private double _calorieTarget = 2000;
        private double _proteinTarget = 150;
        private double _carbsTarget = 250;
        private double _fatTarget = 67;
        private WeightLog? _lastWeightLog;
        private bool _hasWeightHistory;
        private IDrawable? _weightChartDrawable;
        private double _weightGoalKg;
        private int _weightGoalWeeks;
        private string _weightUnit = "kg";
        private UserSettings? _settings;
        private double _tdee;

        public double CalorieTarget
        {
            get => _calorieTarget;
            set
            {
                _calorieTarget = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CaloriesDisplay));
                OnPropertyChanged(nameof(CaloriesPercentage));
                OnPropertyChanged(nameof(IsCalorieGoalSet));
            }
        }

        public double ProteinTarget
        {
            get => _proteinTarget;
            set
            {
                _proteinTarget = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ProteinDisplay));
                OnPropertyChanged(nameof(ProteinPercentage));
            }
        }

        public double CarbsTarget
        {
            get => _carbsTarget;
            set
            {
                _carbsTarget = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CarbsDisplay));
                OnPropertyChanged(nameof(CarbsPercentage));
            }
        }

        public double FatTarget
        {
            get => _fatTarget;
            set
            {
                _fatTarget = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FatDisplay));
                OnPropertyChanged(nameof(FatPercentage));
            }
        }

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set
            {
                _isRefreshing = value;
                OnPropertyChanged();
            }
        }

        public double CaloriesConsumed
        {
            get => _caloriesConsumed;
            set
            {
                _caloriesConsumed = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CaloriesDisplay));
                OnPropertyChanged(nameof(CaloriesPercentage));
                OnPropertyChanged(nameof(NetCalories));
                OnPropertyChanged(nameof(NetCaloriesDisplay));
            }
        }

        public double CaloriesBurned
        {
            get => _caloriesBurned;
            set
            {
                _caloriesBurned = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasCaloriesBurned));
                OnPropertyChanged(nameof(CaloriesBurnedDisplay));
                OnPropertyChanged(nameof(CaloriesPercentage));
                OnPropertyChanged(nameof(NetCalories));
                OnPropertyChanged(nameof(NetCaloriesDisplay));
            }
        }

        public bool HasCaloriesBurned => _caloriesBurned > 0;
        public double NetCalories => _caloriesConsumed - _caloriesBurned;
        public string CaloriesBurnedDisplay => $"{_caloriesBurned:F0} kcal burned";
        public string NetCaloriesDisplay => CalorieTarget > 0
            ? $"{NetCalories:F0} / {CalorieTarget:F0} kcal net"
            : $"{NetCalories:F0} kcal net";

        public double ProteinConsumed
        {
            get => _proteinConsumed;
            set
            {
                _proteinConsumed = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ProteinDisplay));
                OnPropertyChanged(nameof(ProteinPercentage));
            }
        }

        public double CarbsConsumed
        {
            get => _carbsConsumed;
            set
            {
                _carbsConsumed = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CarbsDisplay));
                OnPropertyChanged(nameof(CarbsPercentage));
            }
        }

        public double FatConsumed
        {
            get => _fatConsumed;
            set
            {
                _fatConsumed = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FatDisplay));
                OnPropertyChanged(nameof(FatPercentage));
            }
        }

        // Formatted display properties
        public string CaloriesDisplay => CalorieTarget > 0
            ? $"{CaloriesConsumed:F0} / {CalorieTarget:F0} kcal"
            : $"{CaloriesConsumed:F0} kcal";

        public string ProteinDisplay => ProteinTarget > 0
            ? $"{ProteinConsumed:F0}g / {ProteinTarget:F0}g ({ProteinPercentage:F0}%)"
            : $"{ProteinConsumed:F0}g";

        public string CarbsDisplay => CarbsTarget > 0
            ? $"{CarbsConsumed:F0}g / {CarbsTarget:F0}g ({CarbsPercentage:F0}%)"
            : $"{CarbsConsumed:F0}g";

        public string FatDisplay => FatTarget > 0
            ? $"{FatConsumed:F0}g / {FatTarget:F0}g ({FatPercentage:F0}%)"
            : $"{FatConsumed:F0}g";

        // CaloriesPercentage uses NET calories so exercise credit is reflected in the progress bar
        public double CaloriesPercentage => CalorieTarget > 0 ? (NetCalories / CalorieTarget) * 100 : 0;
        public double ProteinPercentage => ProteinTarget > 0 ? (ProteinConsumed / ProteinTarget) * 100 : 0;
        public double CarbsPercentage => CarbsTarget > 0 ? (CarbsConsumed / CarbsTarget) * 100 : 0;
        public double FatPercentage => FatTarget > 0 ? (FatConsumed / FatTarget) * 100 : 0;

        public bool IsCalorieGoalSet => CalorieTarget > 0;

        /// <summary>True when the user has set all body metrics needed for TDEE computation and has a weight log.</summary>
        public bool HasProfileData => _settings?.HasCompleteProfile == true && _lastWeightLog != null;

        /// <summary>Maintenance energy expenditure to display on the dashboard.</summary>
        public string TdeeDisplay => _tdee > 0 ? $"TDEE: ~{_tdee:F0} kcal/day" : string.Empty;

        /// <summary>True when WeightGoalSuggestedIntakeDisplay has a value to show.</summary>
        public bool HasSuggestedIntake => !string.IsNullOrEmpty(WeightGoalSuggestedIntakeDisplay);

        // ── Weight goal calorie calculation ──────────────────────────────────

        public bool HasWeightGoal =>
            _weightGoalKg > 0 && _weightGoalWeeks > 0 && _lastWeightLog != null;

        public string WeightGoalSummary
        {
            get
            {
                if (!HasWeightGoal) return string.Empty;
                double display = _weightUnit == "lbs" ? _weightGoalKg / 0.45359237 : _weightGoalKg;
                return $"{display:F1} {_weightUnit} in {_weightGoalWeeks} weeks";
            }
        }

        public string WeightGoalPaceDisplay
        {
            get
            {
                if (!HasWeightGoal) return string.Empty;
                double diffKg = Math.Abs(_lastWeightLog!.WeightKg - _weightGoalKg);
                double weeklyKg = diffKg / _weightGoalWeeks;
                double weeklyDisplay = _weightUnit == "lbs" ? weeklyKg / 0.45359237 : weeklyKg;
                return $"{weeklyDisplay:F1} {_weightUnit}/week";
            }
        }

        /// <summary>"~500 kcal/day deficit" or "~200 kcal/day surplus"</summary>
        public string WeightGoalDailyDeltaDisplay
        {
            get
            {
                if (!HasWeightGoal) return string.Empty;
                double diffKg = _lastWeightLog!.WeightKg - _weightGoalKg; // + = losing
                if (Math.Abs(diffKg) < 0.05) return "You've reached your goal!";
                double dailyDelta = diffKg * 7700.0 / (_weightGoalWeeks * 7.0);
                string label = dailyDelta > 0 ? "deficit" : "surplus";
                return $"~{Math.Abs(dailyDelta):F0} kcal/day {label}";
            }
        }

        /// <summary>Suggested intake based on TDEE (when profile is complete) or calorie goal (fallback). Empty when neither is available.</summary>
        public string WeightGoalSuggestedIntakeDisplay
        {
            get
            {
                if (!HasWeightGoal) return string.Empty;
                double diffKg = _lastWeightLog!.WeightKg - _weightGoalKg;
                if (Math.Abs(diffKg) < 0.05) return string.Empty;
                double dailyDelta = diffKg * 7700.0 / (_weightGoalWeeks * 7.0); // + = deficit

                if (HasProfileData && _tdee > 0)
                {
                    // Physiologically accurate: TDEE adjusted for weight goal
                    double suggested = _tdee - dailyDelta;
                    double floor = _settings!.Sex == "Male" ? 1500 : 1200;
                    return $"~{Math.Max(floor, suggested):F0} kcal/day";
                }

                // Fallback: offset the user-set calorie goal
                if (CalorieTarget > 0)
                {
                    double suggested = CalorieTarget - dailyDelta;
                    return $"~{Math.Max(800, suggested):F0} kcal/day";
                }

                return string.Empty;
            }
        }

        public bool HasWeightData => _lastWeightLog != null;

        public bool HasWeightHistory
        {
            get => _hasWeightHistory;
            private set { _hasWeightHistory = value; OnPropertyChanged(); }
        }

        public string LastWeightDisplay => _lastWeightLog != null
            ? $"{_lastWeightLog.DisplayWeight:F1} {_lastWeightLog.Unit}"
            : string.Empty;

        public string LastWeightDateDisplay => _lastWeightLog != null
            ? _lastWeightLog.LoggedAt.ToString("MMM d, yyyy")
            : string.Empty;

        public IDrawable? WeightChartDrawable
        {
            get => _weightChartDrawable;
            private set { _weightChartDrawable = value; OnPropertyChanged(); }
        }

        public ICommand RefreshCommand { get; }

        public DashboardPageViewModel(IMealLogRepository repository, ISettingsRepository settingsRepository, IWeightLogRepository weightLogRepository, IExerciseLogRepository exerciseLogRepository)
        {
            _repository = repository;
            _settingsRepository = settingsRepository;
            _weightLogRepository = weightLogRepository;
            _exerciseLogRepository = exerciseLogRepository;
            RefreshCommand = new Command(async () => await RefreshStatsAsync());
        }

        public async Task InitializeAsync()
        {
            // Load user settings first
            var settings = await _settingsRepository.GetSettingsAsync();
            CalorieTarget = settings.DailyCalorieGoal;
            ProteinTarget = settings.DailyProteinGoal;
            CarbsTarget = settings.DailyCarbsGoal;
            FatTarget = settings.DailyFatGoal;
            _weightGoalKg = settings.WeightGoalKg;
            _weightGoalWeeks = settings.WeightGoalWeeks;
            _weightUnit = settings.WeightUnit ?? "kg";
            _settings = settings;

            await RefreshStatsAsync();
        }

        private async Task RefreshStatsAsync()
        {
            IsRefreshing = true;

            try
            {
                // Get all meals from today
                var allMeals = await _repository.GetAllAsync();
                var today = DateTime.Today;
                var todaysMeals = allMeals.Where(m => m.Timestamp.Date == today).ToList();

                // Calculate totals
                CaloriesConsumed = todaysMeals.Sum(m => m.Calories);
                ProteinConsumed = todaysMeals.Sum(m => m.Protein);
                CarbsConsumed = todaysMeals.Sum(m => m.Carbs);
                FatConsumed = todaysMeals.Sum(m => m.Fat);

                // Load today's exercise calories burned
                var allExercises = await _exerciseLogRepository.GetExerciseLogsAsync();
                CaloriesBurned = allExercises
                    .Where(e => e.LoggedAt.Date == today)
                    .Sum(e => e.CaloriesBurned);

                // Load weight data
                var weightLogs = await _weightLogRepository.GetWeightLogsAsync();
                _lastWeightLog = weightLogs.OrderByDescending(w => w.LoggedAt).FirstOrDefault();

                var uniqueDayCount = weightLogs.Select(w => w.LoggedAt.Date).Distinct().Count();
                HasWeightHistory = uniqueDayCount > 1;

                if (HasWeightHistory)
                    WeightChartDrawable = new WeightChartDrawable(weightLogs, _weightUnit);
                else
                    WeightChartDrawable = null;

                OnPropertyChanged(nameof(HasWeightData));
                OnPropertyChanged(nameof(LastWeightDisplay));
                OnPropertyChanged(nameof(LastWeightDateDisplay));

                // Compute TDEE with the latest weight
                _tdee = _settings?.CalculateTdee(_lastWeightLog?.WeightKg ?? 0) ?? 0;
                OnPropertyChanged(nameof(HasProfileData));
                OnPropertyChanged(nameof(TdeeDisplay));

                OnPropertyChanged(nameof(HasWeightGoal));
                OnPropertyChanged(nameof(WeightGoalSummary));
                OnPropertyChanged(nameof(WeightGoalPaceDisplay));
                OnPropertyChanged(nameof(WeightGoalDailyDeltaDisplay));
                OnPropertyChanged(nameof(WeightGoalSuggestedIntakeDisplay));
                OnPropertyChanged(nameof(HasSuggestedIntake));
            }
            finally
            {
                IsRefreshing = false;
            }
        }
    }
}
