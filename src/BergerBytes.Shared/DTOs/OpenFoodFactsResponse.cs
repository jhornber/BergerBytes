using System.Text.Json.Serialization;

namespace BergerBytes.Shared.DTOs
{
    // Root response from Open Food Facts API
    public class OpenFoodFactsResponse
    {
        [JsonPropertyName("code")]
        public string? Code { get; set; }

        [JsonPropertyName("product")]
        public Product? Product { get; set; }

        [JsonPropertyName("status")]
        public int Status { get; set; } // 0 = not found, 1 = found

        [JsonPropertyName("status_verbose")]
        public string? StatusVerbose { get; set; }
    }

    public class Product
    {
        [JsonPropertyName("product_name")]
        public string? ProductName { get; set; }

        [JsonPropertyName("product_name_en")]
        public string? ProductNameEn { get; set; }

        [JsonPropertyName("brands")]
        public string? Brands { get; set; }

        [JsonPropertyName("quantity")]
        public string? Quantity { get; set; }

        [JsonPropertyName("serving_size")]
        public string? ServingSize { get; set; }

        [JsonPropertyName("serving_quantity")]
        public string? ServingQuantity { get; set; }

        [JsonPropertyName("nutriments")]
        public Nutriments? Nutriments { get; set; }

        [JsonPropertyName("image_url")]
        public string? ImageUrl { get; set; }

        [JsonPropertyName("image_front_url")]
        public string? ImageFrontUrl { get; set; }
    }

    public class Nutriments
    {
        [JsonPropertyName("energy-kcal_100g")]
        public double? EnergyKcal100g { get; set; }

        [JsonPropertyName("proteins_100g")]
        public double? Proteins100g { get; set; }

        [JsonPropertyName("carbohydrates_100g")]
        public double? Carbohydrates100g { get; set; }

        [JsonPropertyName("fat_100g")]
        public double? Fat100g { get; set; }

        [JsonPropertyName("fiber_100g")]
        public double? Fiber100g { get; set; }

        [JsonPropertyName("sugars_100g")]
        public double? Sugars100g { get; set; }

        [JsonPropertyName("sodium_100g")]
        public double? Sodium100g { get; set; }
    }
}
