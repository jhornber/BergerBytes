using BergerBytes.Shared.Models;

namespace BergerBytes.App.Services
{
    public interface IMealLogRepository
    {
        Task<List<MealLog>> GetAllAsync();
        Task<MealLog?> GetByIdAsync(int id);
        Task<int> SaveAsync(MealLog mealLog);
        Task<int> DeleteAsync(int id);
    }
}
