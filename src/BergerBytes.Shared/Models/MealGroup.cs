using System.Collections.ObjectModel;

namespace BergerBytes.Shared.Models
{
    public class MealGroup
    {
        public DateTime Timestamp { get; set; }
        public MealType MealType { get; set; }
        public ObservableCollection<MealLog> Items { get; set; } = new();

        // Summary properties
        public double TotalCalories => Items.Sum(m => m.Calories);
        public double TotalProtein => Items.Sum(m => m.Protein);
        public double TotalCarbs => Items.Sum(m => m.Carbs);
        public double TotalFat => Items.Sum(m => m.Fat);

        // Display helpers
        public string MealTypeDisplay => MealType switch
        {
            MealType.Breakfast => "🌅 Breakfast",
            MealType.Lunch => "☀️ Lunch",
            MealType.Dinner => "🌙 Dinner",
            MealType.Other => "🍽️ Other",
            _ => "🍽️ Other"
        };

        public string TimeDisplay => Timestamp.ToString("h:mm tt");

        public string CaloriesSummary => $"{TotalCalories:F0} calories";

        public string MacrosSummary => $"P: {TotalProtein:F1}g • C: {TotalCarbs:F1}g • F: {TotalFat:F1}g";

        public int ItemCount => Items.Count;

        public string ItemCountDisplay => ItemCount == 1 ? "1 item" : $"{ItemCount} items";
    }
}
