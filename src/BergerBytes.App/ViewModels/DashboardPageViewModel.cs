using System.Windows.Input;
using BergerBytes.App.Services;

namespace BergerBytes.App.ViewModels
{
    public class DashboardPageViewModel : BindableObject
    {
        private readonly IMealLogRepository _repository;
        private readonly ISettingsRepository _settingsRepository;
        private bool _isRefreshing;
        private double _caloriesConsumed;
        private double _proteinConsumed;
        private double _carbsConsumed;
        private double _fatConsumed;
        private double _calorieTarget = 2000;
        private double _proteinTarget = 150;
        private double _carbsTarget = 250;
        private double _fatTarget = 67;

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
            }
        }

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

        public double CaloriesPercentage => CalorieTarget > 0 ? (CaloriesConsumed / CalorieTarget) * 100 : 0;
        public double ProteinPercentage => ProteinTarget > 0 ? (ProteinConsumed / ProteinTarget) * 100 : 0;
        public double CarbsPercentage => CarbsTarget > 0 ? (CarbsConsumed / CarbsTarget) * 100 : 0;
        public double FatPercentage => FatTarget > 0 ? (FatConsumed / FatTarget) * 100 : 0;

        public bool IsCalorieGoalSet => CalorieTarget > 0;

        public ICommand RefreshCommand { get; }

        public DashboardPageViewModel(IMealLogRepository repository, ISettingsRepository settingsRepository)
        {
            _repository = repository;
            _settingsRepository = settingsRepository;
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
            }
            finally
            {
                IsRefreshing = false;
            }
        }
    }
}
