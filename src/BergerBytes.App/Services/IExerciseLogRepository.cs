using BergerBytes.Shared.Models;

namespace BergerBytes.App.Services
{
    public interface IExerciseLogRepository
    {
        Task<List<ExerciseLog>> GetExerciseLogsAsync();
        Task<int> SaveExerciseLogAsync(ExerciseLog entry);
        Task<int> DeleteExerciseLogAsync(int id);
    }
}
