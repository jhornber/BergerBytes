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

        public DateTime LastModified { get; set; } = DateTime.Now;
    }
}
