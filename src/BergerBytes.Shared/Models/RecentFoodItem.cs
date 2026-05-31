using SQLite;

namespace BergerBytes.Shared.Models
{
    public class RecentFoodItem
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string FoodName { get; set; } = string.Empty;
        public double Calories { get; set; }
        public double Protein { get; set; }
        public double Carbs { get; set; }
        public double Fat { get; set; }
        public string Barcode { get; set; } = string.Empty;
        public DateTime LastUsedAt { get; set; }
        public double Quantity { get; set; } = 1.0;
        public string Unit { get; set; } = "serving";

        [Ignore]
        public string CaloriesSummary => $"{Calories:F0} cal";

        [Ignore]
        public string ServingSummary => $"{Quantity:G} {Unit}";

        [Ignore]
        public string MacrosSummary => $"P: {Protein:F0}g  C: {Carbs:F0}g  F: {Fat:F0}g";
    }
}
