using BergerBytes.Shared.Models;

namespace BergerBytes.App.Services
{
    public interface ISettingsRepository
    {
        Task<UserSettings> GetSettingsAsync();
        Task<int> SaveSettingsAsync(UserSettings settings);
    }
}
