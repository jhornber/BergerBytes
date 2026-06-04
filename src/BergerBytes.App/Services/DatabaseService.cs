using SQLite;
using BergerBytes.Shared.Models;

namespace BergerBytes.App.Services
{
    public class DatabaseService : IMealLogRepository, ISettingsRepository, IWeightLogRepository, IExerciseLogRepository, IRecentFoodRepository
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
                await _database.CreateTableAsync<WeightLog>();
                await _database.CreateTableAsync<ExerciseLog>();
                await _database.CreateTableAsync<RecentFoodItem>();

                // Migrate existing tables: add Quantity/Unit columns if they don't exist yet
                try { await _database.ExecuteAsync("ALTER TABLE MealLog ADD COLUMN Quantity REAL NOT NULL DEFAULT 1.0"); } catch { }
                try { await _database.ExecuteAsync("ALTER TABLE MealLog ADD COLUMN Unit TEXT NOT NULL DEFAULT 'serving'"); } catch { }
                try { await _database.ExecuteAsync("ALTER TABLE RecentFoodItem ADD COLUMN Quantity REAL NOT NULL DEFAULT 1.0"); } catch { }
                try { await _database.ExecuteAsync("ALTER TABLE RecentFoodItem ADD COLUMN Unit TEXT NOT NULL DEFAULT 'serving'"); } catch { }
                try { await _database.ExecuteAsync("ALTER TABLE UserSettings ADD COLUMN HeightCm REAL NOT NULL DEFAULT 0"); } catch { }
                try { await _database.ExecuteAsync("ALTER TABLE UserSettings ADD COLUMN AgeYears INTEGER NOT NULL DEFAULT 0"); } catch { }
                try { await _database.ExecuteAsync("ALTER TABLE UserSettings ADD COLUMN Sex TEXT NOT NULL DEFAULT ''"); } catch { }
                try { await _database.ExecuteAsync("ALTER TABLE UserSettings ADD COLUMN ActivityLevel TEXT NOT NULL DEFAULT 'Sedentary'"); } catch { }
                try { await _database.ExecuteAsync("ALTER TABLE UserSettings ADD COLUMN WeightGoalPaceKgPerWeek REAL NOT NULL DEFAULT 0"); } catch { }
                try { await _database.ExecuteAsync("ALTER TABLE UserSettings ADD COLUMN OnboardingCompleted INTEGER NOT NULL DEFAULT 0"); } catch { }
                try { await _database.ExecuteAsync("ALTER TABLE MealLog ADD COLUMN BrandName TEXT NOT NULL DEFAULT ''"); } catch { }

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

        // Weight Log Repository Implementation
        public async Task<List<WeightLog>> GetWeightLogsAsync()
        {
            await InitAsync();
            return await _database!.Table<WeightLog>()
                .OrderByDescending(w => w.LoggedAt)
                .ToListAsync();
        }

        public async Task<int> SaveWeightLogAsync(WeightLog entry)
        {
            await InitAsync();
            if (entry.Id != 0)
                return await _database!.UpdateAsync(entry);
            return await _database!.InsertAsync(entry);
        }

        public async Task<int> DeleteWeightLogAsync(int id)
        {
            await InitAsync();
            return await _database!.DeleteAsync<WeightLog>(id);
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

        // Exercise Log Repository Implementation
        public async Task<List<ExerciseLog>> GetExerciseLogsAsync()
        {
            await InitAsync();
            return await _database!.Table<ExerciseLog>()
                .OrderByDescending(e => e.LoggedAt)
                .ToListAsync();
        }

        public async Task<int> SaveExerciseLogAsync(ExerciseLog entry)
        {
            await InitAsync();
            if (entry.Id != 0)
                return await _database!.UpdateAsync(entry);
            return await _database!.InsertAsync(entry);
        }

        public async Task<int> DeleteExerciseLogAsync(int id)
        {
            await InitAsync();
            return await _database!.DeleteAsync<ExerciseLog>(id);
        }

        // Recent Food Repository Implementation
        public async Task<List<RecentFoodItem>> GetRecentFoodsAsync(int maxCount = 20)
        {
            await InitAsync();
            return await _database!.Table<RecentFoodItem>()
                .OrderByDescending(r => r.LastUsedAt)
                .Take(maxCount)
                .ToListAsync();
        }

        public async Task<List<RecentFoodItem>> GetRecentFoodsAsync(int maxCount, int offset)
        {
            await InitAsync();
            return await _database!.Table<RecentFoodItem>()
                .OrderByDescending(r => r.LastUsedAt)
                .Skip(offset)
                .Take(maxCount)
                .ToListAsync();
        }

        public async Task<List<RecentFoodItem>> SearchRecentFoodsAsync(string query)
        {
            await InitAsync();
            var pattern = $"%{query.Trim().Replace("%", "\\%").Replace("_", "\\_")}%";
            return await _database!.QueryAsync<RecentFoodItem>(
                "SELECT * FROM RecentFoodItem WHERE FoodName LIKE ? ORDER BY LastUsedAt DESC",
                pattern);
        }

        public async Task RecordFoodUsageAsync(string foodName, double calories, double protein, double carbs, double fat, string barcode = "", double quantity = 1.0, string unit = "serving")
        {
            if (string.IsNullOrWhiteSpace(foodName))
                return;

            await InitAsync();

            var normalizedName = foodName.Trim();
            var existing = await _database!.Table<RecentFoodItem>()
                .Where(r => r.FoodName == normalizedName)
                .FirstOrDefaultAsync();

            if (existing != null)
            {
                existing.Calories = calories;
                existing.Protein = protein;
                existing.Carbs = carbs;
                existing.Fat = fat;
                existing.Barcode = barcode;
                existing.Quantity = quantity;
                existing.Unit = unit;
                existing.LastUsedAt = DateTime.Now;
                await _database.UpdateAsync(existing);
            }
            else
            {
                await _database.InsertAsync(new RecentFoodItem
                {
                    FoodName = normalizedName,
                    Calories = calories,
                    Protein = protein,
                    Carbs = carbs,
                    Fat = fat,
                    Barcode = barcode,
                    Quantity = quantity,
                    Unit = unit,
                    LastUsedAt = DateTime.Now
                });

                // Trim to 200 most recent
                var all = await _database.Table<RecentFoodItem>()
                    .OrderByDescending(r => r.LastUsedAt)
                    .ToListAsync();
                if (all.Count > 200)
                {
                    foreach (var old in all.Skip(200))
                        await _database.DeleteAsync(old);
                }
            }
        }

        public async Task ClearRecentFoodsAsync()
        {
            await InitAsync();
            await _database!.DeleteAllAsync<RecentFoodItem>();
        }
    }
}
