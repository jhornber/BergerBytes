using System.Windows.Input;
using BergerBytes.App.Services;
using BergerBytes.Shared.Models;

namespace BergerBytes.App.ViewModels
{
    public class ExerciseLogPageViewModel : BindableObject
    {
        // MET values keyed by activity → intensity
        private static readonly Dictionary<string, Dictionary<string, double>> MetValues = new()
        {
            ["Running"]         = new() { ["Light"] = 7.0,  ["Moderate"] = 9.8,  ["Vigorous"] = 12.8 },
            ["Jogging"]         = new() { ["Light"] = 6.0,  ["Moderate"] = 7.0,  ["Vigorous"] = 9.0  },
            ["Walking"]         = new() { ["Light"] = 2.5,  ["Moderate"] = 3.5,  ["Vigorous"] = 4.3  },
            ["Cycling"]         = new() { ["Light"] = 4.0,  ["Moderate"] = 6.8,  ["Vigorous"] = 10.0 },
            ["Swimming"]        = new() { ["Light"] = 5.0,  ["Moderate"] = 7.0,  ["Vigorous"] = 9.8  },
            ["HIIT"]            = new() { ["Light"] = 8.0,  ["Moderate"] = 10.0, ["Vigorous"] = 14.0 },
            ["Weight Training"] = new() { ["Light"] = 3.0,  ["Moderate"] = 5.0,  ["Vigorous"] = 6.0  },
            ["Yoga"]            = new() { ["Light"] = 2.5,  ["Moderate"] = 3.0,  ["Vigorous"] = 4.0  },
            ["Elliptical"]      = new() { ["Light"] = 4.5,  ["Moderate"] = 6.0,  ["Vigorous"] = 8.5  },
            ["Rowing"]          = new() { ["Light"] = 4.0,  ["Moderate"] = 7.0,  ["Vigorous"] = 9.0  },
            ["Jump Rope"]       = new() { ["Light"] = 8.0,  ["Moderate"] = 11.0, ["Vigorous"] = 13.0 },
            ["Dancing"]         = new() { ["Light"] = 3.0,  ["Moderate"] = 4.8,  ["Vigorous"] = 6.0  },
            ["Hiking"]          = new() { ["Light"] = 5.0,  ["Moderate"] = 6.0,  ["Vigorous"] = 7.5  },
            ["CrossFit"]        = new() { ["Light"] = 8.0,  ["Moderate"] = 11.0, ["Vigorous"] = 13.0 },
            ["Pilates"]         = new() { ["Light"] = 2.8,  ["Moderate"] = 3.5,  ["Vigorous"] = 4.0  },
        };

        private readonly IExerciseLogRepository _repository;
        private readonly IWeightLogRepository _weightLogRepository;
        private double _userWeightKg = 70.0;

        private string _selectedActivity = string.Empty;
        private string _description = string.Empty;
        private DateTime _loggedDate = DateTime.Today;
        private TimeSpan _loggedTime = DateTime.Now.TimeOfDay;
        private int _durationHours;
        private int _durationMinutes = 30;
        private string _selectedIntensity = "Moderate";
        private string _caloriesOverrideText = string.Empty;
        private bool _isSaving;

        public List<string> Activities { get; } = new()
        {
            "Running", "Jogging", "Walking", "Cycling", "Swimming",
            "HIIT", "Weight Training", "Yoga", "Elliptical", "Rowing",
            "Jump Rope", "Dancing", "Hiking", "CrossFit", "Pilates"
        };

        public List<string> Intensities { get; } = new() { "Light", "Moderate", "Vigorous" };

        public string SelectedActivity
        {
            get => _selectedActivity;
            set
            {
                _selectedActivity = value ?? string.Empty;
                OnPropertyChanged();
                NotifyCaloriesChanged();
            }
        }

        public string Description
        {
            get => _description;
            set { _description = value ?? string.Empty; OnPropertyChanged(); }
        }

        public DateTime LoggedDate
        {
            get => _loggedDate;
            set { _loggedDate = value; OnPropertyChanged(); }
        }

        public TimeSpan LoggedTime
        {
            get => _loggedTime;
            set { _loggedTime = value; OnPropertyChanged(); }
        }

        public int DurationHours
        {
            get => _durationHours;
            set
            {
                _durationHours = value < 0 ? 0 : value;
                OnPropertyChanged();
                NotifyCaloriesChanged();
            }
        }

        public int DurationMinutes
        {
            get => _durationMinutes;
            set
            {
                _durationMinutes = value < 0 ? 0 : Math.Min(59, value);
                OnPropertyChanged();
                NotifyCaloriesChanged();
            }
        }

        public string SelectedIntensity
        {
            get => _selectedIntensity;
            set
            {
                _selectedIntensity = value ?? "Moderate";
                OnPropertyChanged();
                NotifyCaloriesChanged();
            }
        }

        public string CaloriesPreview
        {
            get
            {
                int cal = ComputeCalories();
                return cal > 0 ? $"~{cal} kcal (estimated)" : string.Empty;
            }
        }

        public bool HasCaloriesPreview => ComputeCalories() > 0;

        public string CaloriesOverrideText
        {
            get => _caloriesOverrideText;
            set { _caloriesOverrideText = value ?? string.Empty; OnPropertyChanged(); }
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public ExerciseLogPageViewModel(IExerciseLogRepository repository, IWeightLogRepository weightLogRepository)
        {
            _repository = repository;
            _weightLogRepository = weightLogRepository;
            SaveCommand = new Command(async () => await SaveAsync(), () => !_isSaving);
            CancelCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
        }

        public async Task InitializeAsync()
        {
            var weightLogs = await _weightLogRepository.GetWeightLogsAsync();
            var latest = weightLogs.OrderByDescending(w => w.LoggedAt).FirstOrDefault();
            if (latest != null)
                _userWeightKg = latest.WeightKg;
        }

        private int ComputeCalories()
        {
            if (string.IsNullOrEmpty(SelectedActivity) || string.IsNullOrEmpty(SelectedIntensity))
                return 0;
            int totalMinutes = DurationHours * 60 + DurationMinutes;
            if (totalMinutes <= 0) return 0;
            if (!MetValues.TryGetValue(SelectedActivity, out var intensityMap)) return 0;
            if (!intensityMap.TryGetValue(SelectedIntensity, out double met)) return 0;
            return (int)Math.Round(met * _userWeightKg * (totalMinutes / 60.0));
        }

        private void NotifyCaloriesChanged()
        {
            OnPropertyChanged(nameof(CaloriesPreview));
            OnPropertyChanged(nameof(HasCaloriesPreview));
        }

        /// <summary>Returns the user's override if provided and valid, otherwise the MET estimate.</summary>
        private int ResolveCalories()
        {
            if (!string.IsNullOrWhiteSpace(CaloriesOverrideText)
                && int.TryParse(CaloriesOverrideText, out int overrideVal)
                && overrideVal > 0)
                return overrideVal;
            return ComputeCalories();
        }

        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(SelectedActivity))
            {
                await Application.Current!.MainPage!.DisplayAlertAsync(
                    "Missing Activity", "Please select an activity.", "OK");
                return;
            }

            int totalMinutes = DurationHours * 60 + DurationMinutes;
            if (totalMinutes <= 0)
            {
                await Application.Current!.MainPage!.DisplayAlertAsync(
                    "Invalid Duration", "Please enter a duration greater than 0.", "OK");
                return;
            }

            _isSaving = true;
            ((Command)SaveCommand).ChangeCanExecute();

            try
            {
                var entry = new ExerciseLog
                {
                    LoggedAt = LoggedDate.Date + LoggedTime,
                    ActivityType = SelectedActivity,
                    Description = Description.Trim(),
                    DurationMinutes = totalMinutes,
                    Intensity = SelectedIntensity,
                    CaloriesBurned = ResolveCalories()
                };

                await _repository.SaveExerciseLogAsync(entry);
                await Shell.Current.GoToAsync("..");
            }
            finally
            {
                _isSaving = false;
                ((Command)SaveCommand).ChangeCanExecute();
            }
        }
    }
}
