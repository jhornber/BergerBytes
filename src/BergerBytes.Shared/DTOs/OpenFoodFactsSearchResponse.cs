using System.Text.Json.Serialization;

namespace BergerBytes.Shared.DTOs
{
    public class OpenFoodFactsSearchResponse
    {
        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("page")]
        public int Page { get; set; }

        [JsonPropertyName("page_size")]
        public int PageSize { get; set; }

        [JsonPropertyName("products")]
        public List<Product>? Products { get; set; }
    }
}
