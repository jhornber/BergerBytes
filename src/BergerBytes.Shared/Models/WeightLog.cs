using SQLite;

namespace BergerBytes.Shared.Models
{
    public class WeightLog
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public DateTime LoggedAt { get; set; } = DateTime.Now;

        /// <summary>Weight normalized to kilograms for consistent charting.</summary>
        public double WeightKg { get; set; }

        /// <summary>The value as the user entered it (may be in lbs).</summary>
        public double DisplayWeight { get; set; }

        /// <summary>"kg" or "lbs"</summary>
        public string Unit { get; set; } = "kg";
    }
}
