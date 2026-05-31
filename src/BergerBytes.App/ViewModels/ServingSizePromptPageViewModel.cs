using System.Collections.ObjectModel;
using System.Windows.Input;
using BergerBytes.Shared.DTOs;

namespace BergerBytes.App.ViewModels
{
    [QueryProperty(nameof(FoodProduct), "FoodProduct")]
    public class ServingSizePromptPageViewModel : BindableObject
    {
        private FoodProductDTO? _foodProduct;
        private readonly ObservableCollection<string> _units = new();
        private string _quantity = "1";
        private int _selectedUnitIndex = 0;
        private bool _showNutritionPreview;
        private double _previewCalories;
        private double _previewProtein;
        private double _previewCarbs;
        private double _previewFat;
        private bool _imageVisible;
        private string _imageUrl = string.Empty;

        public ObservableCollection<string> Units => _units;

        public ServingSizePromptPageViewModel()
        {
            InitializeUnits();
            AddToMealCommand = new Command(async () => await AddToMealAsync());
            CancelCommand = new Command(async () => await CancelAsync());
        }

        public FoodProductDTO? FoodProduct
        {
            get => _foodProduct;
            set
            {
                _foodProduct = value;
                OnPropertyChanged();
                UpdateFromProduct();
            }
        }

        public string Quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;
                OnPropertyChanged();
                UpdateNutritionPreview();
            }
        }

        public int SelectedUnitIndex
        {
            get => _selectedUnitIndex;
            set
            {
                _selectedUnitIndex = value;
                OnPropertyChanged();
                UpdateNutritionPreview();
            }
        }

        public string ProductName => _foodProduct?.ProductName ?? "Unknown Product";
        public string Barcode => _foodProduct?.Barcode ?? "";
        public bool HasServingSizeInfo => !string.IsNullOrWhiteSpace(_foodProduct?.ServingSizeText);
        public string ServingSizeHintText => HasServingSizeInfo ? $"Suggested serving: {_foodProduct!.ServingSizeText}" : "";

        public bool ShowNutritionPreview
        {
            get => _showNutritionPreview;
            private set { _showNutritionPreview = value; OnPropertyChanged(); }
        }

        public double PreviewCalories
        {
            get => _previewCalories;
            private set { _previewCalories = value; OnPropertyChanged(); }
        }

        public double PreviewProtein
        {
            get => _previewProtein;
            private set { _previewProtein = value; OnPropertyChanged(); }
        }

        public double PreviewCarbs
        {
            get => _previewCarbs;
            private set { _previewCarbs = value; OnPropertyChanged(); }
        }

        public double PreviewFat
        {
            get => _previewFat;
            private set { _previewFat = value; OnPropertyChanged(); }
        }

        public bool ImageVisible
        {
            get => _imageVisible;
            private set { _imageVisible = value; OnPropertyChanged(); }
        }

        public string ImageUrl
        {
            get => _imageUrl;
            private set { _imageUrl = value; OnPropertyChanged(); }
        }

        public ICommand AddToMealCommand { get; }
        public ICommand CancelCommand { get; }

        private void InitializeUnits()
        {
            _units.Add("grams (g)");
            _units.Add("serving");
            _units.Add("item(s)");
            _units.Add("cup(s)");
            _units.Add("tablespoon(s)");
            _units.Add("teaspoon(s)");
            _units.Add("ounce(s) (oz)");
            _units.Add("pound(s) (lb)");
        }

        private void UpdateFromProduct()
        {
            if (_foodProduct == null) return;

            OnPropertyChanged(nameof(ProductName));
            OnPropertyChanged(nameof(Barcode));
            OnPropertyChanged(nameof(HasServingSizeInfo));
            OnPropertyChanged(nameof(ServingSizeHintText));

            if (!string.IsNullOrWhiteSpace(_foodProduct.ImageUrl))
            {
                ImageUrl = _foodProduct.ImageUrl;
                ImageVisible = true;
            }

            if (_foodProduct.ServingSizeGrams.HasValue)
                SelectedUnitIndex = 1; // "serving"

            UpdateNutritionPreview();
        }

        private void UpdateNutritionPreview()
        {
            if (_foodProduct == null || !_foodProduct.IsFound ||
                !double.TryParse(Quantity, out var quantity) || quantity <= 0)
            {
                ShowNutritionPreview = false;
                return;
            }

            var gramsPerUnit = GetGramsPerUnit();
            if (gramsPerUnit <= 0)
            {
                ShowNutritionPreview = false;
                return;
            }

            var totalGrams = quantity * gramsPerUnit;
            PreviewCalories = _foodProduct.CalculateCaloriesForServing(totalGrams);
            PreviewProtein = _foodProduct.CalculateProteinForServing(totalGrams);
            PreviewCarbs = _foodProduct.CalculateCarbsForServing(totalGrams);
            PreviewFat = _foodProduct.CalculateFatForServing(totalGrams);
            ShowNutritionPreview = true;
        }

        private double GetGramsPerUnit()
        {
            if (_foodProduct == null || SelectedUnitIndex < 0 || SelectedUnitIndex >= _units.Count)
                return 0;

            // Use product density for volumetric units when available; default to water (1.0 g/mL).
            var density = _foodProduct.DensityGPerMl ?? 1.0;

            return _units[SelectedUnitIndex] switch
            {
                "grams (g)" => 1.0,
                "serving" => _foodProduct.ServingSizeGrams ?? 100.0,
                "item(s)" => _foodProduct.ServingSizeGrams ?? 100.0,
                "cup(s)" => 240.0 * density,
                "tablespoon(s)" => 15.0 * density,
                "teaspoon(s)" => 5.0 * density,
                "ounce(s) (oz)" => 28.35,
                "pound(s) (lb)" => 453.59,
                _ => 100.0
            };
        }

        private async Task AddToMealAsync()
        {
            if (_foodProduct == null) return;

            if (!double.TryParse(Quantity, out var quantity) || quantity <= 0)
            {
                await Application.Current!.MainPage!.DisplayAlertAsync(
                    "Invalid Quantity", "Please enter a valid quantity greater than 0.", "OK");
                return;
            }

            await Shell.Current.GoToAsync("..", new Dictionary<string, object>
            {
                { "FoodName", _foodProduct.ProductName },
                { "Calories", PreviewCalories.ToString("F0") },
                { "Protein", PreviewProtein.ToString("F1") },
                { "Carbs", PreviewCarbs.ToString("F1") },
                { "Fat", PreviewFat.ToString("F1") },
                { "Barcode", _foodProduct.Barcode },
                { "Quantity", Quantity },
                { "Unit", _units[SelectedUnitIndex] }
            });
        }

        private async Task CancelAsync()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}
