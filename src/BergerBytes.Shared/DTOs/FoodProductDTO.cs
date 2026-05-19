namespace BergerBytes.Shared.DTOs
{
    public class FoodProductDTO
    {
        public string Barcode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public double? CaloriesPer100g { get; set; }
        public double? ProteinPer100g { get; set; }
        public double? CarbsPer100g { get; set; }
        public double? FatPer100g { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsFound { get; set; }

        // Serving size information from Open Food Facts
        public double? ServingSizeGrams { get; set; }
        public string? ServingSizeText { get; set; } // e.g., "1 cup", "1 bar"

        // Density in g/mL derived from serving data; used for accurate volumetric unit conversions.
        // Null means density is unknown; callers should fall back to water density (1.0 g/mL).
        public double? DensityGPerMl { get; set; }

        // Calculate macros for a specific serving size in grams
        public double CalculateCaloriesForServing(double grams)
        {
            if (!CaloriesPer100g.HasValue) return 0;
            return (CaloriesPer100g.Value * grams) / 100.0;
        }

        public double CalculateProteinForServing(double grams)
        {
            if (!ProteinPer100g.HasValue) return 0;
            return (ProteinPer100g.Value * grams) / 100.0;
        }

        public double CalculateCarbsForServing(double grams)
        {
            if (!CarbsPer100g.HasValue) return 0;
            return (CarbsPer100g.Value * grams) / 100.0;
        }

        public double CalculateFatForServing(double grams)
        {
            if (!FatPer100g.HasValue) return 0;
            return (FatPer100g.Value * grams) / 100.0;
        }
    }
}
