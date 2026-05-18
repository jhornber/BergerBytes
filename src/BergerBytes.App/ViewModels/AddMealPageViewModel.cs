using System.Collections.ObjectModel;
using System.Windows.Input;
using BergerBytes.App.Services;
using BergerBytes.Shared.DTOs;
using BergerBytes.Shared.Models;

namespace BergerBytes.App.ViewModels
{
    [QueryProperty(nameof(ScannedBarcode), "ScannedBarcode")]
    [QueryProperty(nameof(FoodName), "FoodName")]
    [QueryProperty(nameof(Calories), "Calories")]
    [QueryProperty(nameof(Protein), "Protein")]
    [QueryProperty(nameof(Carbs), "Carbs")]
    [QueryProperty(nameof(Fat), "Fat")]
    [QueryProperty(nameof(BarcodeFromServing), "Barcode")]
    public class AddMealPageViewModel : BindableObject
    {
        private readonly IMealLogRepository _repository;
        private readonly IFoodService _foodService;
        private int _selectedMealTypeIndex = 3; // Default to "Other"
        private string _currentFoodName = string.Empty;
        private string _currentCalories = string.Empty;
        private string _currentProtein = string.Empty;
        private string _currentCarbs = string.Empty;
        private string _currentFat = string.Empty;
        private DateTime _mealDate = DateTime.Today;
        private TimeSpan _mealTime = DateTime.Now.TimeOfDay;
        private string _scannedBarcodeValue = string.Empty;
        private bool _isLoadingProduct = false;

        public ObservableCollection<FoodEntry> FoodEntries { get; } = new();

        public int SelectedMealTypeIndex
        {
            get => _selectedMealTypeIndex;
            set
            {
                _selectedMealTypeIndex = value;
                OnPropertyChanged();
            }
        }

        public DateTime MealDate
        {
            get => _mealDate;
            set
            {
                _mealDate = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(MealTimestamp));
            }
        }

        public TimeSpan MealTime
        {
            get => _mealTime;
            set
            {
                _mealTime = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(MealTimestamp));
            }
        }

        public DateTime MealTimestamp => MealDate.Date + MealTime;

        public string CurrentFoodName
        {
            get => _currentFoodName;
            set
            {
                _currentFoodName = value;
                OnPropertyChanged();
            }
        }

        public string CurrentCalories
        {
            get => _currentCalories;
            set
            {
                _currentCalories = value;
                OnPropertyChanged();
            }
        }

        public string CurrentProtein
        {
            get => _currentProtein;
            set
            {
                _currentProtein = value;
                OnPropertyChanged();
            }
        }

        public string CurrentCarbs
        {
            get => _currentCarbs;
            set
            {
                _currentCarbs = value;
                OnPropertyChanged();
            }
        }

        public string CurrentFat
        {
            get => _currentFat;
            set
            {
                _currentFat = value;
                OnPropertyChanged();
            }
        }

        public string ScannedBarcode
        {
            get => _scannedBarcodeValue;
            set
            {
                _scannedBarcodeValue = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasScannedBarcode));
                if (!string.IsNullOrEmpty(value))
                {
                    _ = HandleScannedBarcodeAsync(value);
                }
            }
        }

        public bool HasScannedBarcode => !string.IsNullOrWhiteSpace(_scannedBarcodeValue);

        public bool IsLoadingProduct
        {
            get => _isLoadingProduct;
            set
            {
                _isLoadingProduct = value;
                OnPropertyChanged();
            }
        }

        // Query properties from ServingSizePromptPage
        public string FoodName
        {
            set => CurrentFoodName = value;
        }

        public string Calories
        {
            set => CurrentCalories = value;
        }

        public string Protein
        {
            set => CurrentProtein = value;
        }

        public string Carbs
        {
            set => CurrentCarbs = value;
        }

        public string Fat
        {
            set => CurrentFat = value;
        }

        public string BarcodeFromServing
        {
            set => _scannedBarcodeValue = value;
        }

        public bool HasFoodEntries => FoodEntries.Count > 0;

        public double TotalCalories => FoodEntries.Sum(f => f.GetCalories());
        public double TotalProtein => FoodEntries.Sum(f => f.GetProtein());
        public double TotalCarbs => FoodEntries.Sum(f => f.GetCarbs());
        public double TotalFat => FoodEntries.Sum(f => f.GetFat());

        public ICommand AddFoodItemCommand { get; }
        public ICommand RemoveFoodItemCommand { get; }
        public ICommand SaveMealCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand ScanBarcodeCommand { get; }
        public ICommand ClearBarcodeCommand { get; }

        public AddMealPageViewModel(IMealLogRepository repository, IFoodService foodService)
        {
            _repository = repository;
            _foodService = foodService;
            AddFoodItemCommand = new Command(AddFoodItem);
            RemoveFoodItemCommand = new Command<FoodEntry>(RemoveFoodItem);
            SaveMealCommand = new Command(async () => await SaveMealAsync());
            CancelCommand = new Command(async () => await CancelAsync());
            ScanBarcodeCommand = new Command(async () => await ScanBarcodeAsync());
            ClearBarcodeCommand = new Command(ClearBarcode);

            FoodEntries.CollectionChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(HasFoodEntries));
                OnPropertyChanged(nameof(TotalCalories));
                OnPropertyChanged(nameof(TotalProtein));
                OnPropertyChanged(nameof(TotalCarbs));
                OnPropertyChanged(nameof(TotalFat));
            };
        }

        private async Task ScanBarcodeAsync()
        {
            await Shell.Current.GoToAsync("BarcodeScannerPage");
        }

        private async Task HandleScannedBarcodeAsync(string barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode))
                return;

            IsLoadingProduct = true;

            try
            {
                var product = await _foodService.GetProductByBarcodeAsync(barcode);

                if (product.IsFound)
                {
                    // Navigate to serving size prompt page
                    await Shell.Current.GoToAsync("ServingSizePromptPage", new Dictionary<string, object>
                    {
                        { "FoodProduct", product }
                    });
                }
                else
                {
                    // Product not found, allow manual entry with barcode displayed
                    await Application.Current!.MainPage!.DisplayAlert(
                        "Product Not Found",
                        $"Barcode {barcode} was not found in the database. You can enter the nutrition information manually below.",
                        "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current!.MainPage!.DisplayAlert(
                    "Error",
                    $"Failed to lookup product: {ex.Message}. You can enter the information manually.",
                    "OK");
            }
            finally
            {
                IsLoadingProduct = false;
            }
        }

        private void ClearBarcode()
        {
            ScannedBarcode = string.Empty;
        }

        private void AddFoodItem()
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(CurrentFoodName))
            {
                Application.Current?.MainPage?.DisplayAlert("Validation", "Please enter a food name.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(CurrentCalories))
            {
                Application.Current?.MainPage?.DisplayAlert("Validation", "Please enter calories.", "OK");
                return;
            }

            var foodEntry = new FoodEntry
            {
                Name = CurrentFoodName.Trim(),
                CaloriesText = CurrentCalories.Trim(),
                ProteinText = string.IsNullOrWhiteSpace(CurrentProtein) ? "0" : CurrentProtein.Trim(),
                CarbsText = string.IsNullOrWhiteSpace(CurrentCarbs) ? "0" : CurrentCarbs.Trim(),
                FatText = string.IsNullOrWhiteSpace(CurrentFat) ? "0" : CurrentFat.Trim()
            };

            if (!foodEntry.IsValid)
            {
                Application.Current?.MainPage?.DisplayAlert("Validation", "Please check your entries and try again.", "OK");
                return;
            }

            FoodEntries.Add(foodEntry);

            // Clear form for next item
            CurrentFoodName = string.Empty;
            CurrentCalories = string.Empty;
            CurrentProtein = string.Empty;
            CurrentCarbs = string.Empty;
            CurrentFat = string.Empty;
        }

        private void RemoveFoodItem(FoodEntry? item)
        {
            if (item != null)
            {
                FoodEntries.Remove(item);
            }
        }

        private async Task SaveMealAsync()
        {
            if (FoodEntries.Count == 0)
            {
                await Application.Current!.MainPage!.DisplayAlert("No Items", "Please add at least one food item.", "OK");
                return;
            }

            try
            {
                var mealType = (MealType)SelectedMealTypeIndex;
                var timestamp = MealTimestamp; // Use user-selected date and time

                foreach (var entry in FoodEntries)
                {
                    var mealLog = new MealLog
                    {
                        FoodName = entry.Name,
                        Calories = entry.GetCalories(),
                        Protein = entry.GetProtein(),
                        Carbs = entry.GetCarbs(),
                        Fat = entry.GetFat(),
                        MealType = mealType,
                        Timestamp = timestamp
                    };

                    await _repository.SaveAsync(mealLog);
                }

                // Navigate back immediately without confirmation dialog
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await Application.Current!.MainPage!.DisplayAlert("Error", $"Failed to save meal: {ex.Message}", "OK");
            }
        }

        private async Task CancelAsync()
        {
            if (FoodEntries.Count > 0)
            {
                bool confirm = await Application.Current!.MainPage!.DisplayAlert(
                    "Discard Changes?",
                    "You have unsaved items. Are you sure you want to cancel?",
                    "Yes", "No");

                if (!confirm)
                    return;
            }

            await Shell.Current.GoToAsync("..");
        }
    }
}
