using SQLite;

namespace BergerBytes.Shared.Models
{
    public class MealLog
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string FoodName { get; set; } = string.Empty;

        public double Calories { get; set; }

        public double Protein { get; set; }

        public double Carbs { get; set; }

        public double Fat { get; set; }

        public MealType MealType { get; set; } = MealType.Other;

        public DateTime Timestamp { get; set; }

        // Display helper for meal type badge
        public string MealTypeDisplay => MealType switch
        {
            MealType.Breakfast => "🌅 Breakfast",
            MealType.Lunch => "☀️ Lunch",
            MealType.Dinner => "🌙 Dinner",
            MealType.Other => "🍽️ Other",
            _ => "🍽️ Other"
        };
    }
}
