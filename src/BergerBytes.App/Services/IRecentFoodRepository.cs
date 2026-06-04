using BergerBytes.Shared.Models;

namespace BergerBytes.App.Services
{
    public interface IRecentFoodRepository
    {
        Task<List<RecentFoodItem>> GetRecentFoodsAsync(int maxCount = 20);
        Task<List<RecentFoodItem>> GetRecentFoodsAsync(int maxCount, int offset);
        Task<List<RecentFoodItem>> SearchRecentFoodsAsync(string query);
        Task RecordFoodUsageAsync(string foodName, double calories, double protein, double carbs, double fat, string barcode = "", double quantity = 1.0, string unit = "serving");
        Task ClearRecentFoodsAsync();
    }
}
