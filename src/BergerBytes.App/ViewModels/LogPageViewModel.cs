using System.Collections.ObjectModel;
using System.Windows.Input;
using BergerBytes.Shared.Models;
using BergerBytes.App.Services;

namespace BergerBytes.App.ViewModels
{
    public class LogPageViewModel : BindableObject
    {
        private readonly IMealLogRepository _repository;
        private bool _isRefreshing;
        private DateTime _selectedDate = DateTime.Today;

        public ObservableCollection<MealGroup> MealGroups { get; } = new();

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set
            {
                _isRefreshing = value;
                OnPropertyChanged();
            }
        }

        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                if (_selectedDate != value)
                {
                    _selectedDate = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(SelectedDateDisplay));
                    OnPropertyChanged(nameof(IsToday));
                    OnPropertyChanged(nameof(CanGoToNextDay));

                    // Update command states
                    NextDayCommand.ChangeCanExecute();
                    GoToTodayCommand.ChangeCanExecute();

                    _ = RefreshMealLogsAsync();
                }
            }
        }

        public string SelectedDateDisplay
        {
            get
            {
                if (SelectedDate.Date == DateTime.Today)
                    return "Today";
                else if (SelectedDate.Date == DateTime.Today.AddDays(-1))
                    return "Yesterday";
                else
                    return SelectedDate.ToString("MMMM d, yyyy");
            }
        }

        public bool IsToday => SelectedDate.Date == DateTime.Today;

        public bool CanGoToNextDay => SelectedDate.Date < DateTime.Today;

        // Daily Summary Properties
        public double DailyTotalCalories => MealGroups.Sum(g => g.TotalCalories);
        public double DailyTotalProtein => MealGroups.Sum(g => g.TotalProtein);
        public double DailyTotalCarbs => MealGroups.Sum(g => g.TotalCarbs);
        public double DailyTotalFat => MealGroups.Sum(g => g.TotalFat);
        public int DailyMealCount => MealGroups.Count;

        public string DailySummaryCalories => $"{DailyTotalCalories:F0} cal";
        public string DailySummaryMacros => $"P: {DailyTotalProtein:F1}g • C: {DailyTotalCarbs:F1}g • F: {DailyTotalFat:F1}g";
        public string DailyMealCountDisplay => DailyMealCount == 1 ? "1 meal" : $"{DailyMealCount} meals";

        public ICommand RefreshCommand { get; }
        public ICommand DeleteMealGroupCommand { get; }
        public ICommand DeleteItemCommand { get; }
        public ICommand EditMealCommand { get; }
        public ICommand AddMealCommand { get; }
        public ICommand PreviousDayCommand { get; }
        public Command NextDayCommand { get; }
        public Command GoToTodayCommand { get; }
        public ICommand SelectDateCommand { get; }

        public LogPageViewModel(IMealLogRepository repository)
        {
            _repository = repository;
            RefreshCommand = new Command(async () => await RefreshMealLogsAsync());
            DeleteMealGroupCommand = new Command<MealGroup>(async (group) => await DeleteMealGroupAsync(group));
            DeleteItemCommand = new Command<MealLog>(async (meal) => await DeleteMealAsync(meal));
            EditMealCommand = new Command<MealGroup>(async (group) => await NavigateToEditMealAsync(group));
            AddMealCommand = new Command(async () => await NavigateToAddMealAsync());
            PreviousDayCommand = new Command(() => SelectedDate = SelectedDate.AddDays(-1));
            NextDayCommand = new Command(() => SelectedDate = SelectedDate.AddDays(1), () => CanGoToNextDay);
            GoToTodayCommand = new Command(() => SelectedDate = DateTime.Today, () => !IsToday);
            SelectDateCommand = new Command(async () => await SelectDateAsync());
        }

        public async Task InitializeAsync()
        {
            await RefreshMealLogsAsync();
        }

        private async Task NavigateToAddMealAsync()
        {
            await Shell.Current.GoToAsync("addmeal");
        }

        private async Task NavigateToEditMealAsync(MealGroup? mealGroup)
        {
            if (mealGroup == null)
                return;

            var navigationParameter = new Dictionary<string, object>
            {
                { "mealGroup", mealGroup }
            };

            await Shell.Current.GoToAsync("editmeal", navigationParameter);
        }

        private async Task RefreshMealLogsAsync()
        {
            IsRefreshing = true;

            try
            {
                var meals = await _repository.GetAllAsync();

                // Filter meals for the selected date
                var selectedDateStart = SelectedDate.Date;
                var selectedDateEnd = selectedDateStart.AddDays(1);

                var filteredMeals = meals
                    .Where(m => m.Timestamp >= selectedDateStart && m.Timestamp < selectedDateEnd)
                    .ToList();

                // Group meals by timestamp (rounded to nearest second to group items from same meal)
                var groupedMeals = filteredMeals
                    .GroupBy(m => new { m.Timestamp, m.MealType })
                    .Select(g => new MealGroup
                    {
                        Timestamp = g.Key.Timestamp,
                        MealType = g.Key.MealType,
                        Items = new ObservableCollection<MealLog>(g.OrderBy(m => m.Id))
                    })
                    .OrderByDescending(g => g.Timestamp)
                    .ToList();

                MealGroups.Clear();
                foreach (var group in groupedMeals)
                {
                    MealGroups.Add(group);
                }

                // Update daily summary properties
                OnPropertyChanged(nameof(DailyTotalCalories));
                OnPropertyChanged(nameof(DailyTotalProtein));
                OnPropertyChanged(nameof(DailyTotalCarbs));
                OnPropertyChanged(nameof(DailyTotalFat));
                OnPropertyChanged(nameof(DailyMealCount));
                OnPropertyChanged(nameof(DailySummaryCalories));
                OnPropertyChanged(nameof(DailySummaryMacros));
                OnPropertyChanged(nameof(DailyMealCountDisplay));
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        private async Task SelectDateAsync()
        {
            // For now, we'll use a simple action sheet to select recent dates
            // In the future, this could be replaced with a calendar picker
            var options = new List<string>();

            for (int i = 0; i < 7; i++)
            {
                var date = DateTime.Today.AddDays(-i);
                var label = i == 0 ? "Today" : 
                           i == 1 ? "Yesterday" : 
                           date.ToString("MMMM d, yyyy");
                options.Add(label);
            }

            var result = await Application.Current!.MainPage!.DisplayActionSheet(
                "Select Date", 
                "Cancel", 
                null, 
                options.ToArray());

            if (result != null && result != "Cancel")
            {
                var selectedIndex = options.IndexOf(result);
                if (selectedIndex >= 0)
                {
                    SelectedDate = DateTime.Today.AddDays(-selectedIndex);
                }
            }
        }

        private async Task DeleteMealGroupAsync(MealGroup group)
        {
            if (group == null)
                return;

            // Delete all items in the meal group
            foreach (var meal in group.Items)
            {
                await _repository.DeleteAsync(meal.Id);
            }

            MealGroups.Remove(group);

            // Update daily summary
            UpdateDailySummary();
        }

        private async Task DeleteMealAsync(MealLog meal)
        {
            if (meal == null)
                return;

            await _repository.DeleteAsync(meal.Id);

            // Find and update the group
            var group = MealGroups.FirstOrDefault(g => g.Items.Contains(meal));
            if (group != null)
            {
                group.Items.Remove(meal);

                // If group is now empty, remove it
                if (group.Items.Count == 0)
                {
                    MealGroups.Remove(group);
                }
                else
                {
                    // Trigger property change notifications for summary updates
                    OnPropertyChanged(nameof(MealGroups));
                }

                // Update daily summary
                UpdateDailySummary();
            }
        }

        private void UpdateDailySummary()
        {
            OnPropertyChanged(nameof(DailyTotalCalories));
            OnPropertyChanged(nameof(DailyTotalProtein));
            OnPropertyChanged(nameof(DailyTotalCarbs));
            OnPropertyChanged(nameof(DailyTotalFat));
            OnPropertyChanged(nameof(DailyMealCount));
            OnPropertyChanged(nameof(DailySummaryCalories));
            OnPropertyChanged(nameof(DailySummaryMacros));
            OnPropertyChanged(nameof(DailyMealCountDisplay));
        }
    }
}
