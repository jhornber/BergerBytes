using SQLite;

namespace BergerBytes.Shared.Models
{
    [Table("ExerciseLog")]
    public class ExerciseLog
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        /// <summary>Start date/time of the activity.</summary>
        public DateTime LoggedAt { get; set; } = DateTime.Now;

        public string ActivityType { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int DurationMinutes { get; set; }

        /// <summary>"Light", "Moderate", or "Vigorous"</summary>
        public string Intensity { get; set; } = "Moderate";

        public int CaloriesBurned { get; set; }

        // ── Display helpers (not persisted) ──────────────────────────────────

        [Ignore]
        public bool HasDescription => !string.IsNullOrWhiteSpace(Description);

        [Ignore]
        public string TimeDisplay => LoggedAt.ToString("h:mm tt");

        [Ignore]
        public string DurationDisplay
        {
            get
            {
                int h = DurationMinutes / 60;
                int m = DurationMinutes % 60;
                string dur = h > 0 ? (m > 0 ? $"{h}h {m}m" : $"{h}h") : $"{m}m";
                return $"{dur} · {Intensity}";
            }
        }
    }
}
