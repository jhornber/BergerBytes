using SQLite;

namespace BergerBytes.Shared.Models
{
    public class UserSettings
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public double DailyCalorieGoal { get; set; } = 0;

        public double DailyProteinGoal { get; set; } = 0;

        public double DailyCarbsGoal { get; set; } = 0;

        public double DailyFatGoal { get; set; } = 0;

        public bool DarkModeEnabled { get; set; } = false;

        /// <summary>"kg" or "lbs" — default unit for weight logging.</summary>
        public string WeightUnit { get; set; } = "kg";

        /// <summary>Target body weight in kilograms. 0 = no goal set.</summary>
        public double WeightGoalKg { get; set; } = 0;

        /// <summary>Number of weeks to reach WeightGoalKg. 0 = no goal set.</summary>
        public int WeightGoalWeeks { get; set; } = 0;

        /// <summary>Height in centimeters. 0 = not set.</summary>
        public double HeightCm { get; set; } = 0;

        /// <summary>Age in years. 0 = not set.</summary>
        public int AgeYears { get; set; } = 0;

        /// <summary>"Male" or "Female". Empty string = not set.</summary>
        public string Sex { get; set; } = string.Empty;

        /// <summary>Activity level for TDEE calculation. One of: Sedentary, Lightly Active, Moderately Active, Very Active, Extra Active.</summary>
        public string ActivityLevel { get; set; } = "Sedentary";

        public DateTime LastModified { get; set; } = DateTime.Now;

        // ── Computed helpers (not persisted) ────────────────────────────────

        /// <summary>True when all fields required for BMR/TDEE are present.</summary>
        [Ignore]
        public bool HasCompleteProfile => HeightCm > 0 && AgeYears > 0 && !string.IsNullOrEmpty(Sex);

        /// <summary>
        /// Calculates TDEE (Total Daily Energy Expenditure) using the Mifflin-St Jeor BMR
        /// formula multiplied by the activity level multiplier.
        /// Returns 0 when profile data is incomplete.
        /// </summary>
        public double CalculateTdee(double weightKg)
        {
            if (!HasCompleteProfile || weightKg <= 0) return 0;

            // Mifflin-St Jeor BMR
            double bmr = (10 * weightKg) + (6.25 * HeightCm) - (5 * AgeYears)
                         + (Sex == "Male" ? 5 : -161);

            double multiplier = ActivityLevel switch
            {
                "Lightly Active"    => 1.375,
                "Moderately Active" => 1.55,
                "Very Active"       => 1.725,
                "Extra Active"      => 1.9,
                _                   => 1.2   // Sedentary
            };

            return bmr * multiplier;
        }
    }
}
