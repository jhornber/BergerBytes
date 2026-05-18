using BergerBytes.Shared.DTOs;
using System.Collections.ObjectModel;

namespace BergerBytes.App.Pages
{
    [QueryProperty(nameof(FoodProduct), "FoodProduct")]
    public partial class ServingSizePromptPage : ContentPage
    {
        private FoodProductDTO? _foodProduct;
        private readonly ObservableCollection<string> _units = new();

        public ServingSizePromptPage()
        {
            InitializeComponent();
            BindingContext = this;
            InitializeUnits();

            QuantityEntry.TextChanged += OnQuantityChanged;
            UnitPicker.SelectedIndexChanged += OnUnitChanged;
        }

        public FoodProductDTO? FoodProduct
        {
            get => _foodProduct;
            set
            {
                _foodProduct = value;
                OnPropertyChanged();
                UpdateUI();
            }
        }

        private string _quantity = "1";
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

        private int _selectedUnitIndex = 0;
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

        public string ServingSizeHintText
        {
            get
            {
                if (_foodProduct == null || !HasServingSizeInfo)
                    return "";
                return $"Suggested serving: {_foodProduct.ServingSizeText}";
            }
        }

        private bool _showNutritionPreview;
        public bool ShowNutritionPreview
        {
            get => _showNutritionPreview;
            set
            {
                _showNutritionPreview = value;
                OnPropertyChanged();
            }
        }

        private double _previewCalories;
        public double PreviewCalories
        {
            get => _previewCalories;
            set
            {
                _previewCalories = value;
                OnPropertyChanged();
            }
        }

        private double _previewProtein;
        public double PreviewProtein
        {
            get => _previewProtein;
            set
            {
                _previewProtein = value;
                OnPropertyChanged();
            }
        }

        private double _previewCarbs;
        public double PreviewCarbs
        {
            get => _previewCarbs;
            set
            {
                _previewCarbs = value;
                OnPropertyChanged();
            }
        }

        private double _previewFat;
        public double PreviewFat
        {
            get => _previewFat;
            set
            {
                _previewFat = value;
                OnPropertyChanged();
            }
        }

        private void InitializeUnits()
        {
            _units.Clear();
            _units.Add("grams (g)");
            _units.Add("serving");
            _units.Add("item(s)");
            _units.Add("cup(s)");
            _units.Add("tablespoon(s)");
            _units.Add("teaspoon(s)");
            _units.Add("ounce(s) (oz)");
            _units.Add("pound(s) (lb)");

            UnitPicker.ItemsSource = _units;
            UnitPicker.SelectedIndex = 0;
        }

        private void UpdateUI()
        {
            if (_foodProduct == null)
                return;

            OnPropertyChanged(nameof(ProductName));
            OnPropertyChanged(nameof(Barcode));
            OnPropertyChanged(nameof(HasServingSizeInfo));
            OnPropertyChanged(nameof(ServingSizeHintText));

            // Load image if available
            if (!string.IsNullOrWhiteSpace(_foodProduct.ImageUrl))
            {
                ProductImage.Source = _foodProduct.ImageUrl;
                ProductImage.IsVisible = true;
            }

            // If serving size is available, set it as default
            if (_foodProduct.ServingSizeGrams.HasValue)
            {
                SelectedUnitIndex = 1; // "serving"
            }

            UpdateNutritionPreview();
        }

        private void OnQuantityChanged(object? sender, TextChangedEventArgs e)
        {
            UpdateNutritionPreview();
        }

        private void OnUnitChanged(object? sender, EventArgs e)
        {
            UpdateNutritionPreview();
        }

        private void UpdateNutritionPreview()
        {
            if (_foodProduct == null || !_foodProduct.IsFound)
            {
                ShowNutritionPreview = false;
                return;
            }

            if (!double.TryParse(Quantity, out var quantity) || quantity <= 0)
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

            var selectedUnit = _units[SelectedUnitIndex];

            return selectedUnit switch
            {
                "grams (g)" => 1.0,
                "serving" => _foodProduct.ServingSizeGrams ?? 100.0,
                "item(s)" => _foodProduct.ServingSizeGrams ?? 100.0,
                "cup(s)" => 240.0, // Approximate
                "tablespoon(s)" => 15.0,
                "teaspoon(s)" => 5.0,
                "ounce(s) (oz)" => 28.35,
                "pound(s) (lb)" => 453.59,
                _ => 100.0
            };
        }

        private async void OnAddToMealClicked(object sender, EventArgs e)
        {
            if (_foodProduct == null)
                return;

            if (!double.TryParse(Quantity, out var quantity) || quantity <= 0)
            {
                await DisplayAlert("Invalid Quantity", "Please enter a valid quantity greater than 0.", "OK");
                return;
            }

            var gramsPerUnit = GetGramsPerUnit();
            var totalGrams = quantity * gramsPerUnit;

            // Pass back the calculated nutrition data
            await Shell.Current.GoToAsync("..", new Dictionary<string, object>
            {
                { "FoodName", _foodProduct.ProductName },
                { "Calories", PreviewCalories.ToString("F0") },
                { "Protein", PreviewProtein.ToString("F1") },
                { "Carbs", PreviewCarbs.ToString("F1") },
                { "Fat", PreviewFat.ToString("F1") },
                { "Barcode", _foodProduct.Barcode }
            });
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}
