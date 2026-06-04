using System.Collections.ObjectModel;
using System.Windows.Input;
using BergerBytes.App.Services;
using BergerBytes.Shared.DTOs;
using BergerBytes.Shared.Models;

namespace BergerBytes.App.ViewModels
{
    [QueryProperty(nameof(ScannedBarcode), "ScannedBarcode")]
    [QueryProperty(nameof(FoodName), "FoodName")]
    [QueryProperty(nameof(BrandName), "BrandName")]
    [QueryProperty(nameof(Calories), "Calories")]
    [QueryProperty(nameof(Protein), "Protein")]
    [QueryProperty(nameof(Carbs), "Carbs")]
    [QueryProperty(nameof(Fat), "Fat")]
    [QueryProperty(nameof(Quantity), "Quantity")]
    [QueryProperty(nameof(Unit), "Unit")]
    [QueryProperty(nameof(BarcodeFromServing), "Barcode")]
    [QueryProperty(nameof(SelectedFoodProduct), "SelectedFoodProduct")]
    [QueryProperty(nameof(SelectedRecentFood), "SelectedRecentFood")]
    [QueryProperty(nameof(EntryToken), "EntryToken")]
    public class EditMealPageViewModel : BindableObject
    {
        private readonly IMealLogRepository _repository;
        private readonly IFoodService _foodService;
        private readonly IRecentFoodRepository _recentFoodRepository;
        private MealGroup? _originalMealGroup;
        private int _selectedMealTypeIndex;
        private DateTime _mealDate;
        private TimeSpan _mealTime;
        private string _scannedBarcodeValue = string.Empty;
        private string _pendingBarcode = string.Empty;
        private string _lastProcessedBarcode = string.Empty;
        private bool _isLoadingProduct = false;
        private FoodProductDTO? _lastSelectedFoodProduct;
        private FoodProductDTO? _pendingFoodProduct;
        private RecentFoodItem? _lastSelectedRecentFood;
        private RecentFoodItem? _pendingRecentFood;
        private bool _hasPendingFoodEntry = false;
        private string _pendingEntryToken = string.Empty;
        private string _lastProcessedEntryToken = string.Empty;
        private string _pendingFoodName = string.Empty;
        private string _pendingBrandName = string.Empty;
        private string _pendingCalories = string.Empty;
        private string _pendingProtein = string.Empty;
        private string _pendingCarbs = string.Empty;
        private string _pendingFat = string.Empty;
        private string _pendingQuantity = "1";
        private string _pendingUnit = "serving";

        public ObservableCollection<MealLog> MealItems { get; } = new();

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
            }
        }

        public TimeSpan MealTime
        {
            get => _mealTime;
            set
            {
                _mealTime = value;
                OnPropertyChanged();
            }
        }

        public DateTime MealTimestamp => MealDate.Date + MealTime;

        public string ScannedBarcode
        {
            get => _scannedBarcodeValue;
            set
            {
                // Guard against MAUI Shell re-applying the same query property value when
                // EditMealPage becomes active again after popping a child route (e.g. ServingSizePromptPage).
                // Without this, re-application would trigger HandleScannedBarcodeAsync again,
                // causing a navigation loop back to ServingSizePromptPage.
                if (_scannedBarcodeValue == value) return;

                _scannedBarcodeValue = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasScannedBarcode));
                if (!string.IsNullOrEmpty(value) && value != _lastProcessedBarcode)
                    _pendingBarcode = value;
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

        // Query properties from ServingSizePromptPage / FoodSearchPage / ManualAddFoodPage
        public string FoodName
        {
            set
            {
                _pendingFoodName = value;
                if (!string.IsNullOrEmpty(value))
                    _hasPendingFoodEntry = true;
            }
        }

        public string BrandName
        {
            set => _pendingBrandName = value;
        }

        public string Calories
        {
            set => _pendingCalories = value;
        }

        public string Protein
        {
            set => _pendingProtein = value;
        }

        public string Carbs
        {
            set => _pendingCarbs = value;
        }

        public string Fat
        {
            set => _pendingFat = value;
        }

        public string Quantity
        {
            set => _pendingQuantity = value;
        }

        public string Unit
        {
            set => _pendingUnit = value;
        }

        public string BarcodeFromServing
        {
            set => _scannedBarcodeValue = value;
        }

        public string EntryToken
        {
            set => _pendingEntryToken = value ?? string.Empty;
        }

        // Summary properties
        public double TotalCalories => MealItems.Sum(m => m.Calories);
        public double TotalProtein => MealItems.Sum(m => m.Protein);
        public double TotalCarbs => MealItems.Sum(m => m.Carbs);
        public double TotalFat => MealItems.Sum(m => m.Fat);

        public string TotalCaloriesDisplay => $"{TotalCalories:F0} cal";
        public string TotalMacrosDisplay => $"P: {TotalProtein:F1}g • C: {TotalCarbs:F1}g • F: {TotalFat:F1}g";

        public FoodProductDTO? SelectedFoodProduct
        {
            set
            {
                if (value == null || ReferenceEquals(_lastSelectedFoodProduct, value)) return;
                _lastSelectedFoodProduct = value;
                _pendingFoodProduct = value;
            }
        }

        public RecentFoodItem? SelectedRecentFood
        {
            set
            {
                if (value == null || ReferenceEquals(_lastSelectedRecentFood, value)) return;
                _lastSelectedRecentFood = value;
                _pendingRecentFood = value;
            }
        }

        public ICommand DeleteItemCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand ScanBarcodeCommand { get; }
        public ICommand SearchFoodCommand { get; }
        public ICommand ManualAddFoodCommand { get; }
        public ICommand ClearBarcodeCommand { get; }

        public EditMealPageViewModel(IMealLogRepository repository, IFoodService foodService, IRecentFoodRepository recentFoodRepository)
        {
            _repository = repository;
            _foodService = foodService;
            _recentFoodRepository = recentFoodRepository;
            DeleteItemCommand = new Command<MealLog>(DeleteItem);
            SaveCommand = new Command(async () => await SaveAsync());
            CancelCommand = new Command(async () => await CancelAsync());
            ScanBarcodeCommand = new Command(async () => await ScanBarcodeAsync());
            SearchFoodCommand = new Command(async () => await SearchFoodAsync());
            ManualAddFoodCommand = new Command(async () => await Shell.Current.GoToAsync("ManualAddFoodPage"));
            ClearBarcodeCommand = new Command(ClearBarcode);

            MealItems.CollectionChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(TotalCalories));
                OnPropertyChanged(nameof(TotalProtein));
                OnPropertyChanged(nameof(TotalCarbs));
                OnPropertyChanged(nameof(TotalFat));
                OnPropertyChanged(nameof(TotalCaloriesDisplay));
                OnPropertyChanged(nameof(TotalMacrosDisplay));
            };
        }

        public void Initialize(MealGroup mealGroup)
        {
            _originalMealGroup = mealGroup;
            SelectedMealTypeIndex = (int)mealGroup.MealType;
            MealDate = mealGroup.Timestamp.Date;
            MealTime = mealGroup.Timestamp.TimeOfDay;

            MealItems.Clear();
            foreach (var item in mealGroup.Items)
            {
                MealItems.Add(item);
            }
        }

        private void DeleteItem(MealLog? item)
        {
            if (item != null)
            {
                MealItems.Remove(item);
            }
        }

        private async Task ScanBarcodeAsync()
        {
            await Shell.Current.GoToAsync("BarcodeScannerPage");
        }

        private async Task SearchFoodAsync()
        {
            await Shell.Current.GoToAsync("FoodSearchPage");
        }

        public async Task ProcessPendingFoodEntryAsync()
        {
            if (!_hasPendingFoodEntry) return;
            if (_pendingEntryToken == _lastProcessedEntryToken)
            {
                _hasPendingFoodEntry = false;
                return;
            }
            _lastProcessedEntryToken = _pendingEntryToken;
            _hasPendingFoodEntry = false;
            await AddScannedFoodItemAsync(_pendingFoodName, _pendingBrandName, _pendingCalories, _pendingProtein, _pendingCarbs, _pendingFat, _pendingQuantity, _pendingUnit);
            _pendingFoodName = string.Empty;
            _pendingBrandName = string.Empty;
            _pendingCalories = string.Empty;
            _pendingProtein = string.Empty;
            _pendingCarbs = string.Empty;
            _pendingFat = string.Empty;
            _pendingQuantity = "1";
            _pendingUnit = "serving";
        }

        public async Task ProcessPendingFoodProductAsync()
        {
            if (_pendingFoodProduct == null) return;
            // Discard any stale re-applied FoodName from the previous navigation round-trip.
            _hasPendingFoodEntry = false;
            var product = _pendingFoodProduct;
            _pendingFoodProduct = null;
            await Shell.Current.GoToAsync("ServingSizePromptPage", new Dictionary<string, object>
            {
                { "FoodProduct", product }
            });
        }

        public async Task ProcessPendingRecentFoodAsync()
        {
            if (_pendingRecentFood == null) return;
            _hasPendingFoodEntry = false;
            var item = _pendingRecentFood;
            _pendingRecentFood = null;
            await Shell.Current.GoToAsync("ServingSizePromptPage", new Dictionary<string, object>
            {
                { "RecentFood", item }
            });
        }

        public async Task ProcessPendingBarcodeAsync()
        {
            if (string.IsNullOrEmpty(_pendingBarcode)) return;
            // Discard any stale re-applied FoodName from the previous navigation round-trip.
            _hasPendingFoodEntry = false;
            var barcode = _pendingBarcode;
            _pendingBarcode = string.Empty;
            await HandleScannedBarcodeAsync(barcode);
        }

        private async Task HandleScannedBarcodeAsync(string barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode))
                return;

            _lastProcessedBarcode = barcode;
            IsLoadingProduct = true;

            try
            {
                var product = await _foodService.GetProductByBarcodeAsync(barcode);

                if (product.IsFound)
                {
                    await Shell.Current.GoToAsync("ServingSizePromptPage", new Dictionary<string, object>
                    {
                        { "FoodProduct", product }
                    });
                }
                else
                {
                    await Application.Current!.MainPage!.DisplayAlert(
                        "Product Not Found",
                        $"Barcode {barcode} was not found in the database.",
                        "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current!.MainPage!.DisplayAlert(
                    "Error",
                    $"Failed to lookup product: {ex.Message}",
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

        private async Task AddScannedFoodItemAsync(string foodName, string brandName, string calories, string protein, string carbs, string fat, string quantity = "1", string unit = "serving")
        {
            if (string.IsNullOrWhiteSpace(foodName) || string.IsNullOrWhiteSpace(calories))
                return;

            var mealLog = new MealLog
            {
                FoodName = foodName,
                BrandName = brandName,
                Calories = double.TryParse(calories, out var cal) ? cal : 0,
                Protein = double.TryParse(protein, out var prot) ? prot : 0,
                Carbs = double.TryParse(carbs, out var crb) ? crb : 0,
                Fat = double.TryParse(fat, out var ft) ? ft : 0,
                Quantity = double.TryParse(quantity, out var qty) && qty > 0 ? qty : 1.0,
                Unit = string.IsNullOrWhiteSpace(unit) ? "serving" : unit,
                MealType = (MealType)SelectedMealTypeIndex,
                Timestamp = MealTimestamp
            };

            await _repository.SaveAsync(mealLog);
            MealItems.Add(mealLog);

            // Save to recent foods (fire and forget)
            _ = _recentFoodRepository.RecordFoodUsageAsync(
                mealLog.FoodName,
                mealLog.Calories,
                mealLog.Protein,
                mealLog.Carbs,
                mealLog.Fat,
                _scannedBarcodeValue,
                mealLog.Quantity,
                mealLog.Unit);

            // Clear the barcode
            ScannedBarcode = string.Empty;
        }

        private async Task SaveAsync()
        {
            if (MealItems.Count == 0)
            {
                await Application.Current!.MainPage!.DisplayAlert(
                    "Delete Meal?",
                    "All items have been removed. Do you want to delete this entire meal?",
                    "Yes", "No");

                // Delete all original items from database
                if (_originalMealGroup != null)
                {
                    foreach (var item in _originalMealGroup.Items)
                    {
                        await _repository.DeleteAsync(item.Id);
                    }
                }

                await Shell.Current.GoToAsync("..");
                return;
            }

            try
            {
                var mealType = (MealType)SelectedMealTypeIndex;
                var timestamp = MealTimestamp;

                // Update all items with new values
                foreach (var item in MealItems)
                {
                    item.MealType = mealType;
                    item.Timestamp = timestamp;
                    await _repository.SaveAsync(item);
                }

                // Delete items that were removed (in original but not in current)
                if (_originalMealGroup != null)
                {
                    var removedItems = _originalMealGroup.Items
                        .Where(orig => !MealItems.Any(current => current.Id == orig.Id))
                        .ToList();

                    foreach (var removed in removedItems)
                    {
                        await _repository.DeleteAsync(removed.Id);
                    }
                }

                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await Application.Current!.MainPage!.DisplayAlert("Error", $"Failed to save changes: {ex.Message}", "OK");
            }
        }

        private async Task CancelAsync()
        {
            bool hasChanges = HasChanges();

            if (hasChanges)
            {
                bool confirm = await Application.Current!.MainPage!.DisplayAlert(
                    "Discard Changes?",
                    "You have unsaved changes. Are you sure you want to cancel?",
                    "Yes", "No");

                if (!confirm)
                    return;
            }

            await Shell.Current.GoToAsync("..");
        }

        private bool HasChanges()
        {
            if (_originalMealGroup == null)
                return false;

            // Check if meal type or timestamp changed
            if ((int)_originalMealGroup.MealType != SelectedMealTypeIndex ||
                _originalMealGroup.Timestamp != MealTimestamp)
                return true;

            // Check if items were removed
            if (MealItems.Count != _originalMealGroup.Items.Count)
                return true;

            // Check if any item IDs are different
            var originalIds = _originalMealGroup.Items.Select(i => i.Id).OrderBy(id => id).ToList();
            var currentIds = MealItems.Select(i => i.Id).OrderBy(id => id).ToList();

            return !originalIds.SequenceEqual(currentIds);
        }
    }
}
