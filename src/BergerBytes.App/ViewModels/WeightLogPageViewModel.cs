using System.Windows.Input;
using BergerBytes.App.Services;
using BergerBytes.Shared.Models;

namespace BergerBytes.App.ViewModels
{
    public class WeightLogPageViewModel : BindableObject
    {
        private readonly IWeightLogRepository _repository;
        private readonly ISettingsRepository _settingsRepository;
        private DateTime _loggedDate = DateTime.Today;
        private TimeSpan _loggedTime = DateTime.Now.TimeOfDay;
        private string _weightText = string.Empty;
        private string _selectedUnit = "kg";
        private bool _isSaving;

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

        public string WeightText
        {
            get => _weightText;
            set { _weightText = value; OnPropertyChanged(); }
        }

        public string SelectedUnit
        {
            get => _selectedUnit;
            set { _selectedUnit = value; OnPropertyChanged(); }
        }

        public bool IsSaving
        {
            get => _isSaving;
            set { _isSaving = value; OnPropertyChanged(); }
        }

        public List<string> Units { get; } = new() { "kg", "lbs" };

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public WeightLogPageViewModel(IWeightLogRepository repository, ISettingsRepository settingsRepository)
        {
            _repository = repository;
            _settingsRepository = settingsRepository;
            SaveCommand = new Command(async () => await SaveAsync(), () => !IsSaving);
            CancelCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
        }

        public async Task InitializeAsync()
        {
            var settings = await _settingsRepository.GetSettingsAsync();
            SelectedUnit = settings.WeightUnit ?? "kg";
        }

        private async Task SaveAsync()
        {
            var text = WeightText?.Trim();
            if (!double.TryParse(text, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.CurrentCulture, out double displayWeight)
                || displayWeight <= 0)
            {
                await Shell.Current.DisplayAlert("Invalid Weight", "Please enter a valid weight value.", "OK");
                return;
            }

            IsSaving = true;
            ((Command)SaveCommand).ChangeCanExecute();

            try
            {
                double weightKg = _selectedUnit == "lbs"
                    ? displayWeight * 0.45359237
                    : displayWeight;

                var entry = new WeightLog
                {
                    LoggedAt = _loggedDate.Date + _loggedTime,
                    WeightKg = weightKg,
                    DisplayWeight = displayWeight,
                    Unit = _selectedUnit
                };

                await _repository.SaveWeightLogAsync(entry);
                await Shell.Current.GoToAsync("..");
            }
            finally
            {
                IsSaving = false;
                ((Command)SaveCommand).ChangeCanExecute();
            }
        }
    }
}
