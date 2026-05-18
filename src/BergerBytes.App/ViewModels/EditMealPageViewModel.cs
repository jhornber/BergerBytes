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
    public class EditMealPageViewModel : BindableObject
    {
        private readonly IMealLogRepository _repository;
        private readonly IFoodService _foodService;
        private MealGroup? _originalMealGroup;
        private int _selectedMealTypeIndex;
        private DateTime _mealDate;
        private TimeSpan _mealTime;
        private string _scannedBarcodeValue = string.Empty;
        private bool _isLoadingProduct = false;

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
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _ = AddScannedFoodItemAsync(value, Calories, Protein, Carbs, Fat);
                }
            }
        }

        public string Calories { get; set; } = string.Empty;
        public string Protein { get; set; } = string.Empty;
        public string Carbs { get; set; } = string.Empty;
        public string Fat { get; set; } = string.Empty;

        public string BarcodeFromServing
        {
            set => _scannedBarcodeValue = value;
        }

        // Summary properties
        public double TotalCalories => MealItems.Sum(m => m.Calories);
        public double TotalProtein => MealItems.Sum(m => m.Protein);
        public double TotalCarbs => MealItems.Sum(m => m.Carbs);
        public double TotalFat => MealItems.Sum(m => m.Fat);

        public string TotalCaloriesDisplay => $"{TotalCalories:F0} cal";
        public string TotalMacrosDisplay => $"P: {TotalProtein:F1}g • C: {TotalCarbs:F1}g • F: {TotalFat:F1}g";

        public ICommand DeleteItemCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand ScanBarcodeCommand { get; }
        public ICommand ClearBarcodeCommand { get; }

        public EditMealPageViewModel(IMealLogRepository repository, IFoodService foodService)
        {
            _repository = repository;
            _foodService = foodService;
            DeleteItemCommand = new Command<MealLog>(DeleteItem);
            SaveCommand = new Command(async () => await SaveAsync());
            CancelCommand = new Command(async () => await CancelAsync());
            ScanBarcodeCommand = new Command(async () => await ScanBarcodeAsync());
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

        private async Task AddScannedFoodItemAsync(string foodName, string calories, string protein, string carbs, string fat)
        {
            if (string.IsNullOrWhiteSpace(foodName) || string.IsNullOrWhiteSpace(calories))
                return;

            var mealLog = new MealLog
            {
                FoodName = foodName,
                Calories = double.TryParse(calories, out var cal) ? cal : 0,
                Protein = double.TryParse(protein, out var prot) ? prot : 0,
                Carbs = double.TryParse(carbs, out var crb) ? crb : 0,
                Fat = double.TryParse(fat, out var ft) ? ft : 0,
                MealType = (MealType)SelectedMealTypeIndex,
                Timestamp = MealTimestamp
            };

            await _repository.SaveAsync(mealLog);
            MealItems.Add(mealLog);

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
