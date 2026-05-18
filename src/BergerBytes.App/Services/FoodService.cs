using System.Net.Http.Json;
using System.Text.RegularExpressions;
using BergerBytes.Shared.DTOs;

namespace BergerBytes.App.Services
{
    public class FoodService : IFoodService
    {
        private readonly HttpClient _httpClient;
        private const string OpenFoodFactsBaseUrl = "https://world.openfoodfacts.org/api/v2/product";

        public FoodService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.Timeout = TimeSpan.FromSeconds(10);
        }

        public async Task<FoodProductDTO> GetProductByBarcodeAsync(string barcode, CancellationToken ct = default)
        {
            try
            {
                var url = $"{OpenFoodFactsBaseUrl}/{barcode}.json";
                var response = await _httpClient.GetAsync(url, ct);

                if (!response.IsSuccessStatusCode)
                {
                    return new FoodProductDTO
                    {
                        Barcode = barcode,
                        IsFound = false
                    };
                }

                var offResponse = await response.Content.ReadFromJsonAsync<OpenFoodFactsResponse>(cancellationToken: ct);

                if (offResponse == null || offResponse.Status == 0 || offResponse.Product == null)
                {
                    return new FoodProductDTO
                    {
                        Barcode = barcode,
                        IsFound = false
                    };
                }

                return MapToDTO(offResponse, barcode);
            }
            catch (Exception ex)
            {
                // Log error (could use ILogger here in future)
                System.Diagnostics.Debug.WriteLine($"Error fetching product from Open Food Facts: {ex.Message}");

                return new FoodProductDTO
                {
                    Barcode = barcode,
                    IsFound = false
                };
            }
        }

        private FoodProductDTO MapToDTO(OpenFoodFactsResponse response, string barcode)
        {
            var product = response.Product!;

            // Get best product name available
            var productName = !string.IsNullOrWhiteSpace(product.ProductName)
                ? product.ProductName
                : product.ProductNameEn ?? "Unknown Product";

            // Add brand if available
            if (!string.IsNullOrWhiteSpace(product.Brands))
            {
                productName = $"{product.Brands} - {productName}";
            }

            // Parse serving size in grams
            var servingSizeGrams = ParseServingSizeToGrams(product.ServingSize, product.ServingQuantity);

            return new FoodProductDTO
            {
                Barcode = barcode,
                ProductName = productName,
                CaloriesPer100g = product.Nutriments?.EnergyKcal100g,
                ProteinPer100g = product.Nutriments?.Proteins100g,
                CarbsPer100g = product.Nutriments?.Carbohydrates100g,
                FatPer100g = product.Nutriments?.Fat100g,
                ImageUrl = product.ImageFrontUrl ?? product.ImageUrl,
                ServingSizeGrams = servingSizeGrams,
                ServingSizeText = product.ServingSize,
                IsFound = true
            };
        }

        private double? ParseServingSizeToGrams(string? servingSize, string? servingQuantity)
        {
            if (string.IsNullOrWhiteSpace(servingSize))
            {
                if (!string.IsNullOrWhiteSpace(servingQuantity) && double.TryParse(servingQuantity, out var quantity))
                {
                    return quantity;
                }
                return null;
            }

            // Try to extract grams from serving size string
            // Examples: "100g", "100 g", "100 grams", "3.5 oz (100g)"
            var match = Regex.Match(servingSize, @"(\d+(?:\.\d+)?)\s*g(?:rams?)?", RegexOptions.IgnoreCase);
            if (match.Success && double.TryParse(match.Groups[1].Value, out var grams))
            {
                return grams;
            }

            // Try to parse just the number if the string starts with a number
            match = Regex.Match(servingSize, @"^(\d+(?:\.\d+)?)");
            if (match.Success && double.TryParse(match.Groups[1].Value, out var value))
            {
                return value;
            }

            return null;
        }
    }
}
