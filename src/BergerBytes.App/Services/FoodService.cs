using System.Net.Http.Json;
using System.Text.RegularExpressions;
using BergerBytes.Shared.DTOs;

namespace BergerBytes.App.Services
{
    public class FoodService : IFoodService
    {
        private readonly HttpClient _httpClient;
        private const string OpenFoodFactsBaseUrl = "https://world.openfoodfacts.org/api/v2/product";
        private const string OpenFoodFactsSearchUrl = "https://world.openfoodfacts.org/cgi/search.pl";
        private const string SearchFields = "code,product_name,product_name_en,brands,serving_size,serving_quantity,serving_quantity_unit,nutriments,image_front_url";

        public FoodService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.Timeout = TimeSpan.FromSeconds(10);
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("BergerBytes/1.0 (https://github.com/berger-bytes)");
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

        public async Task<IList<FoodProductDTO>> SearchFoodAsync(string query, CancellationToken ct = default)
        {
            try
            {
                var encodedQuery = Uri.EscapeDataString(query);
                var url = $"{OpenFoodFactsSearchUrl}?search_terms={encodedQuery}&search_simple=1&action=process&json=1&page_size=20&fields={SearchFields}";

                var response = await _httpClient.GetAsync(url, ct);
                if (!response.IsSuccessStatusCode)
                    return Array.Empty<FoodProductDTO>();

                var searchResponse = await response.Content.ReadFromJsonAsync<OpenFoodFactsSearchResponse>(cancellationToken: ct);
                if (searchResponse?.Products == null)
                    return Array.Empty<FoodProductDTO>();

                return searchResponse.Products
                    .Where(p => !string.IsNullOrWhiteSpace(p.ProductName) || !string.IsNullOrWhiteSpace(p.ProductNameEn))
                    .Select(p => MapProductToDTO(p, p.Code ?? string.Empty))
                    .ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error searching food: {ex.Message}");
                return Array.Empty<FoodProductDTO>();
            }
        }

        private FoodProductDTO MapToDTO(OpenFoodFactsResponse response, string barcode)
            => MapProductToDTO(response.Product!, barcode);

        private FoodProductDTO MapProductToDTO(Product product, string barcode)
        {
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

            var densityGPerMl = ComputeDensityGPerMl(product.ServingSize, servingSizeGrams);

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
                DensityGPerMl = densityGPerMl,
                IsFound = true
            };
        }

        private double? ParseServingSizeToGrams(string? servingSize, double? servingQuantity)
        {
            // serving_quantity from the API is authoritative and already in grams.
            // Only skip it if the API explicitly reports mL (some liquid products).
            if (servingQuantity.HasValue && servingQuantity.Value > 0)
                return servingQuantity.Value;

            if (string.IsNullOrWhiteSpace(servingSize))
                return null;

            // Fall back: extract grams from serving size text, e.g. "100g", "3.5 oz (100g)"
            var match = Regex.Match(servingSize, @"(\d+(?:\.\d+)?)\s*g(?:rams?)?", RegexOptions.IgnoreCase);
            if (match.Success && double.TryParse(match.Groups[1].Value, out var grams))
                return grams;

            return null;
        }

        // Derive g/mL density when serving_size text contains an explicit volume measurement.
        // Returns null when density cannot be determined (caller should assume 1.0 g/mL).
        private double? ComputeDensityGPerMl(string? servingSize, double? servingQuantityGrams)
        {
            if (string.IsNullOrWhiteSpace(servingSize) || !servingQuantityGrams.HasValue || servingQuantityGrams.Value <= 0)
                return null;

            // "360 mL", "355 ml"
            var mlMatch = Regex.Match(servingSize, @"(\d+(?:\.\d+)?)\s*ml", RegexOptions.IgnoreCase);
            if (mlMatch.Success && double.TryParse(mlMatch.Groups[1].Value, out var ml) && ml > 0)
                return servingQuantityGrams.Value / ml;

            // "12 fl oz", "12 fl. oz"
            var flOzMatch = Regex.Match(servingSize, @"(\d+(?:\.\d+)?)\s*fl\.?\s*oz", RegexOptions.IgnoreCase);
            if (flOzMatch.Success && double.TryParse(flOzMatch.Groups[1].Value, out var flOz) && flOz > 0)
                return servingQuantityGrams.Value / (flOz * 29.5735);

            // "0.5 cups", "1 cup"  — e.g. "0.5 cups (45 g)" for oatmeal
            var cupsMatch = Regex.Match(servingSize, @"(\d+(?:\.\d+)?)\s*cups?", RegexOptions.IgnoreCase);
            if (cupsMatch.Success && double.TryParse(cupsMatch.Groups[1].Value, out var cups) && cups > 0)
                return servingQuantityGrams.Value / (cups * 240.0);

            // "2 tbsp", "2 tablespoons"
            var tbspMatch = Regex.Match(servingSize, @"(\d+(?:\.\d+)?)\s*(?:tbsp|tablespoons?)", RegexOptions.IgnoreCase);
            if (tbspMatch.Success && double.TryParse(tbspMatch.Groups[1].Value, out var tbsp) && tbsp > 0)
                return servingQuantityGrams.Value / (tbsp * 15.0);

            // "1 tsp", "1 teaspoon"
            var tspMatch = Regex.Match(servingSize, @"(\d+(?:\.\d+)?)\s*(?:tsp|teaspoons?)", RegexOptions.IgnoreCase);
            if (tspMatch.Success && double.TryParse(tspMatch.Groups[1].Value, out var tsp) && tsp > 0)
                return servingQuantityGrams.Value / (tsp * 5.0);

            return null;
        }
    }
}
