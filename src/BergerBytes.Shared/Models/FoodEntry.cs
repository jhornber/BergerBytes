namespace BergerBytes.Shared.Models
{
    public class FoodEntry
    {
        public string Name { get; set; } = string.Empty;
        public string CaloriesText { get; set; } = string.Empty;
        public string ProteinText { get; set; } = string.Empty;
        public string CarbsText { get; set; } = string.Empty;
        public string FatText { get; set; } = string.Empty;

        // Validation helpers
        public bool IsValid => !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(CaloriesText);

        public double GetCalories() => double.TryParse(CaloriesText, out var val) && val >= 0 ? val : 0;
        public double GetProtein() => double.TryParse(ProteinText, out var val) && val >= 0 ? val : 0;
        public double GetCarbs() => double.TryParse(CarbsText, out var val) && val >= 0 ? val : 0;
        public double GetFat() => double.TryParse(FatText, out var val) && val >= 0 ? val : 0;
    }
}
