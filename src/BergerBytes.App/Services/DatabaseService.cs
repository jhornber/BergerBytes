using SQLite;
using BergerBytes.Shared.Models;

namespace BergerBytes.App.Services
{
    public class DatabaseService : IMealLogRepository, ISettingsRepository
    {
        private SQLiteAsyncConnection? _database;
        private readonly object _initLock = new();
        private bool _isInitialized;

        private async Task InitAsync()
        {
            if (_isInitialized)
                return;

            bool shouldInitialize = false;
            lock (_initLock)
            {
                if (!_isInitialized)
                {
                    shouldInitialize = true;
                }
            }

            if (shouldInitialize)
            {
                var databasePath = Path.Combine(FileSystem.AppDataDirectory, "bergerbytes.db");
                _database = new SQLiteAsyncConnection(databasePath);
                await _database.CreateTableAsync<MealLog>();
                await _database.CreateTableAsync<UserSettings>();

                lock (_initLock)
                {
                    _isInitialized = true;
                }
            }
        }

        public async Task<List<MealLog>> GetAllAsync()
        {
            await InitAsync();
            return await _database!.Table<MealLog>()
                .OrderByDescending(m => m.Timestamp)
                .ToListAsync();
        }

        public async Task<MealLog?> GetByIdAsync(int id)
        {
            await InitAsync();
            return await _database!.Table<MealLog>()
                .Where(m => m.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<int> SaveAsync(MealLog mealLog)
        {
            await InitAsync();

            if (mealLog.Id != 0)
            {
                return await _database!.UpdateAsync(mealLog);
            }
            else
            {
                return await _database!.InsertAsync(mealLog);
            }
        }

        public async Task<int> DeleteAsync(int id)
        {
            await InitAsync();
            return await _database!.DeleteAsync<MealLog>(id);
        }

        // Settings Repository Implementation
        public async Task<UserSettings> GetSettingsAsync()
        {
            await InitAsync();
            var settings = await _database!.Table<UserSettings>().FirstOrDefaultAsync();

            if (settings == null)
            {
                // Create default settings if none exist
                settings = new UserSettings
                {
                    DailyCalorieGoal = 2000,
                    DailyProteinGoal = 150,
                    DailyCarbsGoal = 250,
                    DailyFatGoal = 67,
                    DarkModeEnabled = false,
                    LastModified = DateTime.Now
                };
                await _database.InsertAsync(settings);
            }

            return settings;
        }

        public async Task<int> SaveSettingsAsync(UserSettings settings)
        {
            await InitAsync();
            settings.LastModified = DateTime.Now;

            if (settings.Id != 0)
            {
                return await _database!.UpdateAsync(settings);
            }
            else
            {
                return await _database!.InsertAsync(settings);
            }
        }
    }
}
