using System.Windows.Input;

namespace BergerBytes.App.ViewModels
{
    public class ManualAddFoodViewModel : BindableObject
    {
        private string _foodName = string.Empty;
        private string _brandName = string.Empty;
        private string _unit = "each";
        private string _numberOfServings = "1";
        private string _caloriesPerServing = string.Empty;
        private string _proteinPerServing = string.Empty;
        private string _carbsPerServing = string.Empty;
        private string _fatPerServing = string.Empty;

        public string FoodName
        {
            get => _foodName;
            set { _foodName = value; OnPropertyChanged(); }
        }

        public string BrandName
        {
            get => _brandName;
            set { _brandName = value; OnPropertyChanged(); }
        }

        public string Unit
        {
            get => _unit;
            set { _unit = value; OnPropertyChanged(); }
        }

        public string NumberOfServings
        {
            get => _numberOfServings;
            set { _numberOfServings = value; OnPropertyChanged(); }
        }

        public string CaloriesPerServing
        {
            get => _caloriesPerServing;
            set { _caloriesPerServing = value; OnPropertyChanged(); }
        }

        public string ProteinPerServing
        {
            get => _proteinPerServing;
            set { _proteinPerServing = value; OnPropertyChanged(); }
        }

        public string CarbsPerServing
        {
            get => _carbsPerServing;
            set { _carbsPerServing = value; OnPropertyChanged(); }
        }

        public string FatPerServing
        {
            get => _fatPerServing;
            set { _fatPerServing = value; OnPropertyChanged(); }
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public ManualAddFoodViewModel()
        {
            SaveCommand = new Command(async () => await SaveAsync());
            CancelCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
        }

        private async Task SaveAsync()
        {
            var page = Application.Current?.Windows[0].Page;

            if (string.IsNullOrWhiteSpace(FoodName))
            {
                if (page != null)
                    await page.DisplayAlertAsync("Required", "Please enter a food name.", "OK");
                return;
            }

            if (!double.TryParse(CaloriesPerServing, out double calsPerServing) || calsPerServing < 0)
            {
                if (page != null)
                    await page.DisplayAlertAsync("Required", "Please enter a valid calorie amount per serving.", "OK");
                return;
            }

            if (!double.TryParse(NumberOfServings, out double numServings) || numServings <= 0)
                numServings = 1;

            double.TryParse(ProteinPerServing, out double proteinPerServing);
            double.TryParse(CarbsPerServing, out double carbsPerServing);
            double.TryParse(FatPerServing, out double fatPerServing);

            await Shell.Current.GoToAsync("..", new Dictionary<string, object>
            {
                { "FoodName",   FoodName.Trim() },
                { "BrandName",  BrandName.Trim() },
                { "Calories",   (calsPerServing * numServings).ToString("F1") },
                { "Protein",    (proteinPerServing * numServings).ToString("F1") },
                { "Carbs",      (carbsPerServing * numServings).ToString("F1") },
                { "Fat",        (fatPerServing * numServings).ToString("F1") },
                { "Quantity",   numServings.ToString("F2") },
                { "Unit",       string.IsNullOrWhiteSpace(Unit) ? "each" : Unit.Trim() },
                { "EntryToken", Guid.NewGuid().ToString() }
            });
        }
    }
}
