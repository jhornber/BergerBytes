using BergerBytes.Shared.Models;

namespace BergerBytes.App.Services
{
    public interface IWeightLogRepository
    {
        Task<List<WeightLog>> GetWeightLogsAsync();
        Task<int> SaveWeightLogAsync(WeightLog entry);
        Task<int> DeleteWeightLogAsync(int id);
    }
}
