# SQLite Offline-First Implementation

## Summary
This implementation provides offline-first meal logging with instant local persistence using SQLite.

## Components Created

### 1. Models (BergerBytes.Shared)
- **MealLog.cs**: Entity model with properties:
  - `Id` (Primary Key, Auto-increment)
  - `FoodName` (string)
  - `Calories` (double)
  - `Protein` (double)
  - `Carbs` (double)
  - `Fat` (double)
  - `Timestamp` (DateTime)

### 2. Services (BergerBytes.App)
- **IMealLogRepository.cs**: Repository interface for data access abstraction
- **DatabaseService.cs**: SQLite implementation with:
  - Lazy database initialization
  - CRUD operations (Create, Read, Update, Delete)
  - Automatic table creation
  - Database location: `FileSystem.AppDataDirectory/bergerbytes.db`

### 3. ViewModels (BergerBytes.App)
- **LogPageViewModel.cs**: MVVM pattern implementation with:
  - `ObservableCollection<MealLog>` for UI binding
  - Commands: RefreshCommand, DeleteCommand, AddMealCommand
  - Async initialization on page load

### 4. Pages (BergerBytes.App)
- **LogPage.xaml**: Updated UI with:
  - CollectionView displaying meal logs
  - Swipe-to-delete functionality
  - Pull-to-refresh
  - Add meal button
  - Real-time macro display (Protein, Carbs, Fat)
- **LogPage.xaml.cs**: Constructor injection for repository dependency

## Dependency Injection Setup
Registered in `MauiProgram.cs`:
- `IMealLogRepository` → `DatabaseService` (Singleton)
- `LogPage` (Transient)

## NuGet Packages Added
- `sqlite-net-pcl` (v1.9.172) - Both App and Shared projects

## Acceptance Criteria Status

✅ **AC 1**: MealLog table exists in SQLite with all required fields (Id, FoodName, Calories, Protein, Carbs, Fat, Timestamp)

✅ **AC 2**: Data persists across app restarts (SQLite database stored in AppDataDirectory)

✅ **AC 3**: Deleting a log entry in the UI removes it from the MealLog table immediately (Swipe-to-delete implemented)

## Testing Instructions
1. Run the app
2. Navigate to the "Log" tab
3. Tap "+ Add Sample Meal" to create test entries
4. Swipe left on any meal to reveal "Delete" button
5. Force-close the app and restart to verify persistence
6. Pull down to refresh the list

## Architecture Notes
- **Offline-First**: All writes go to SQLite immediately
- **Repository Pattern**: Abstraction enables future Supabase migration
- **MVVM**: Clean separation between UI and business logic
- **Async/Await**: All database operations are asynchronous
