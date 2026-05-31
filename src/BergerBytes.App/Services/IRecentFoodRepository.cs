using BergerBytes.Shared.Models;

namespace BergerBytes.App.Services
{
    public interface IRecentFoodRepository
    {
        Task<List<RecentFoodItem>> GetRecentFoodsAsync(int maxCount = 20);
        Task RecordFoodUsageAsync(string foodName, double calories, double protein, double carbs, double fat, string barcode = "", double quantity = 1.0, string unit = "serving");
        Task ClearRecentFoodsAsync();
    }
}
