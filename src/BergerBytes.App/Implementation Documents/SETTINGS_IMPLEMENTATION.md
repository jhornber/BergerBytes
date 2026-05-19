# User Settings & Adjustable Daily Goals Implementation

## Summary
Implemented persistent user settings stored in SQLite, allowing users to customize their daily nutrition goals. The Dashboard now dynamically displays progress against these personalized targets.

---

## Components Created

### 1. UserSettings Model (BergerBytes.Shared/Models)
**File:** `UserSettings.cs`

SQLite entity storing user preferences:
- `Id` (Primary Key, Auto-increment)
- `DailyCalorieGoal` (double, default: 2000)
- `DailyProteinGoal` (double, default: 150g)
- `DailyCarbsGoal` (double, default: 250g)
- `DailyFatGoal` (double, default: 67g)
- `DarkModeEnabled` (bool, default: false)
- `LastModified` (DateTime)

### 2. ISettingsRepository Interface (BergerBytes.App/Services)
**File:** `ISettingsRepository.cs`

Repository pattern abstraction:
```csharp
Task<UserSettings> GetSettingsAsync();
Task<int> SaveSettingsAsync(UserSettings settings);
```

### 3. DatabaseService Updates (BergerBytes.App/Services)
**File:** `DatabaseService.cs`

Enhanced to implement `ISettingsRepository`:
- Creates `UserSettings` table on first run
- `GetSettingsAsync()`: Returns existing settings or creates defaults
- `SaveSettingsAsync()`: Persists user preferences with timestamp
- Single-row pattern (only one settings record per user)

### 4. SettingsPageViewModel (BergerBytes.App/ViewModels)
**File:** `SettingsPageViewModel.cs`

Features:
- **Bindable Properties**: DailyCalorieGoal, DailyProteinGoal, DailyCarbsGoal, DailyFatGoal
- **SaveSettingsCommand**: Persists changes to database
- **InitializeAsync()**: Loads current settings on page appearance
- **Success/Error Alerts**: User feedback after save operations

### 5. Enhanced SettingsPage UI (BergerBytes.App/Pages)
**File:** `SettingsPage.xaml`

Complete redesign with three sections:

#### Daily Goals Section
- **Calorie Goal**: Numeric entry field (kcal)
- **Protein Goal**: Numeric entry field (grams)
- **Carbs Goal**: Numeric entry field (grams)
- **Fat Goal**: Numeric entry field (grams)
- Each field has descriptive labels and units

#### Appearance Section
- **Dark Mode Toggle**: Switch control (bound to settings)

#### Account Section
- **Sign In Button**: Placeholder for future Google OAuth integration
- Disabled with "coming soon" message

### 6. DashboardPageViewModel Updates
**File:** `DashboardPageViewModel.cs`

Dynamic target loading:
- Constructor now accepts `ISettingsRepository`
- `InitializeAsync()` loads user settings **before** calculating stats
- Target properties changed from hardcoded to reactive properties
- Dashboard automatically reflects custom goals

---

## Data Flow

### Saving Settings
1. User modifies goals in Settings page
2. Taps "Save Settings" button
3. ViewModel updates `UserSettings` object
4. Repository persists to SQLite (Insert or Update)
5. Success alert displayed to user

### Loading Settings on Dashboard
1. User navigates to Dashboard tab
2. `OnAppearing()` triggers `InitializeAsync()`
3. ViewModel loads settings from repository
4. Target properties updated (CalorieTarget, ProteinTarget, etc.)
5. Stats recalculated with new targets
6. UI updates via data binding
7. Progress bar reflects percentage against **custom** calorie goal

### Settings Persistence
- Settings stored in `UserSettings` table (SQLite)
- Single-row pattern: Only one settings record exists
- Default settings created on first app launch
- Settings persist across app restarts
- LastModified timestamp tracks changes

---

## Database Schema

### UserSettings Table
```sql
CREATE TABLE UserSettings (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    DailyCalorieGoal REAL,
    DailyProteinGoal REAL,
    DailyCarbsGoal REAL,
    DailyFatGoal REAL,
    DarkModeEnabled INTEGER,
    LastModified TEXT
);
```

---

## Dependency Injection Updates

**File:** `MauiProgram.cs`

Added registrations:
```csharp
builder.Services.AddSingleton<ISettingsRepository, DatabaseService>();
builder.Services.AddTransient<SettingsPage>();
```

Both `IMealLogRepository` and `ISettingsRepository` resolve to the same `DatabaseService` singleton, ensuring single database instance.

---

## User Experience

### Settings Page Workflow
1. Navigate to **Settings** tab
2. See current goals pre-populated in entry fields
3. Modify any goal values
4. Tap **Save Settings**
5. See success confirmation alert
6. Settings immediately available to Dashboard

### Dashboard Integration
1. Navigate to **Dashboard** tab
2. Progress bar shows percentage of **custom** calorie goal
3. Calorie display: "consumed / **YOUR_GOAL** kcal"
4. Stats reflect personalized targets
5. Changes to settings reflect immediately on next Dashboard visit

---

## Testing Instructions

### Test 1: Default Settings
1. Fresh app install (or clear app data)
2. Navigate to Settings
3. Verify defaults: 2000 kcal, 150g protein, 250g carbs, 67g fat

### Test 2: Custom Goals
1. Change calorie goal to 2500
2. Change protein goal to 180
3. Tap "Save Settings"
4. Verify success alert
5. Navigate to Dashboard
6. Verify calorie display shows "X / 2500 kcal"
7. Progress bar percentage reflects new target

### Test 3: Persistence
1. Set custom goals (e.g., 3000 kcal)
2. Save settings
3. Force-close app
4. Restart app
5. Navigate to Settings
6. Verify custom goals still displayed
7. Navigate to Dashboard
8. Verify progress reflects saved goals

### Test 4: Settings Update Dashboard
1. Add sample meals in Log tab (e.g., 1000 calories)
2. Navigate to Dashboard → see progress (e.g., 1000/2000 = 50%)
3. Navigate to Settings
4. Change calorie goal to 1000
5. Save settings
6. Return to Dashboard
7. Verify progress bar shows 100% (1000/1000)

---

## Architecture Highlights

### Repository Pattern
- `ISettingsRepository` abstracts data access
- Enables future migration to Supabase/cloud storage
- Testable business logic

### Single Responsibility
- `UserSettings`: Data model only
- `ISettingsRepository`: Data access contract
- `SettingsPageViewModel`: Business logic & UI state
- `SettingsPage`: UI presentation only

### Reactive UI
- All Entry fields bound to ViewModel properties
- Two-way data binding for real-time updates
- Command pattern for save action

### Offline-First
- Settings stored locally in SQLite
- No network dependency
- Instant read/write operations

---

## Future Enhancements (Not Implemented)

### Race Day Mode
- Toggle to triple carb goals
- Temporary override (resets next day)
- Visual indicator in Dashboard

### Macro Ratios
- Calculate recommended P/C/F based on calorie goal
- Suggested ratios (e.g., 40/40/20)
- One-tap apply presets

### Goal History
- Track changes to goals over time
- Analytics: "You increased protein goal 3 times this month"

### Dark Mode Implementation
- Hook `DarkModeEnabled` to AppTheme
- Apply theme changes dynamically
- Persist preference

### Cloud Sync
- Sync settings to Supabase
- Cross-device consistency
- Backup/restore functionality

---

## Code Quality

### Error Handling
- Try-catch in `SaveSettingsAsync()`
- User-friendly error alerts
- Graceful fallback to defaults

### Async Patterns
- All database operations async
- UI remains responsive
- Proper await usage

### Null Safety
- Nullable annotations enabled
- Safe null checks in repository
- Default object creation pattern

### Performance
- Settings loaded only on page appearance
- Single database connection (singleton)
- Minimal UI re-renders

---

## Key Files Modified

1. ✅ `BergerBytes.Shared/Models/UserSettings.cs` (NEW)
2. ✅ `BergerBytes.App/Services/ISettingsRepository.cs` (NEW)
3. ✅ `BergerBytes.App/Services/DatabaseService.cs` (UPDATED)
4. ✅ `BergerBytes.App/ViewModels/SettingsPageViewModel.cs` (NEW)
5. ✅ `BergerBytes.App/ViewModels/DashboardPageViewModel.cs` (UPDATED)
6. ✅ `BergerBytes.App/Pages/SettingsPage.xaml` (UPDATED)
7. ✅ `BergerBytes.App/Pages/SettingsPage.xaml.cs` (UPDATED)
8. ✅ `BergerBytes.App/Pages/DashboardPage.xaml.cs` (UPDATED)
9. ✅ `BergerBytes.App/MauiProgram.cs` (UPDATED)

---

## Build Status
✅ **Build Successful** - All components compile without errors

---

## Summary of Capabilities

Users can now:
- ✅ Customize daily calorie goal
- ✅ Customize daily protein goal
- ✅ Customize daily carbs goal
- ✅ Customize daily fat goal
- ✅ Save preferences to local database
- ✅ See personalized progress on Dashboard
- ✅ Settings persist across app restarts
- ✅ Toggle dark mode preference (UI ready, theming not implemented)

The Dashboard now dynamically reflects **user-defined goals** instead of hardcoded values, providing a truly personalized nutrition tracking experience! 🎯📊
