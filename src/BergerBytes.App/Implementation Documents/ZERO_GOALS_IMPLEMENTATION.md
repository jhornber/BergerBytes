# Zero/Empty Goal Values Implementation

## Summary
Updated the Settings page to allow users to set goals to zero or leave them empty, which will be saved as zero. Negative values are automatically converted to zero for data integrity.

---

## Changes Made

### 1. SettingsPageViewModel Refactor

#### Changed from Double to String Properties
**Before:**
```csharp
private double _dailyCalorieGoal;
public double DailyCalorieGoal { get; set; }
```

**After:**
```csharp
private string _dailyCalorieGoalText = string.Empty;
public string DailyCalorieGoalText { get; set; }
```

**Reason:** String properties allow Entry fields to be truly empty, whereas double properties always have a default value.

#### Updated InitializeAsync Method
**Before:**
```csharp
DailyCalorieGoal = _settings.DailyCalorieGoal;
```

**After:**
```csharp
DailyCalorieGoalText = _settings.DailyCalorieGoal > 0 
    ? _settings.DailyCalorieGoal.ToString("F0") 
    : string.Empty;
```

**Behavior:**
- If goal is > 0: Display the number (e.g., "2000")
- If goal is 0: Display empty string (blank Entry field)

#### New ParseGoalValue Helper Method
```csharp
private double ParseGoalValue(string text)
{
    // If empty or whitespace, return 0
    if (string.IsNullOrWhiteSpace(text))
        return 0;

    // Try to parse the value
    if (double.TryParse(text, out double value))
    {
        // If negative, return 0
        return value < 0 ? 0 : value;
    }

    // If parsing fails, return 0
    return 0;
}
```

**Validation Logic:**
1. **Empty/Whitespace** → 0
2. **Valid positive number** → Use as-is
3. **Negative number** → Convert to 0
4. **Invalid text** → Default to 0

#### Updated SaveSettingsAsync Method
**Before:**
```csharp
_settings.DailyCalorieGoal = DailyCalorieGoal;
```

**After:**
```csharp
_settings.DailyCalorieGoal = ParseGoalValue(DailyCalorieGoalText);
```

All four goals now use `ParseGoalValue()` for consistent validation.

---

### 2. SettingsPage.xaml Updates

#### Changed Placeholder Text
**Before:**
```xaml
<Entry Text="{Binding DailyCalorieGoal}" 
       Placeholder="2000" />
```

**After:**
```xaml
<Entry Text="{Binding DailyCalorieGoalText}" 
       Placeholder="0 = no goal" />
```

**Benefits:**
- Clearer user guidance
- Indicates that empty/zero means "no goal"
- Applied to all four goal Entry fields

---

### 3. UserSettings Model Updates

#### Changed Default Values
**Before:**
```csharp
public double DailyCalorieGoal { get; set; } = 2000;
public double DailyProteinGoal { get; set; } = 150;
public double DailyCarbsGoal { get; set; } = 250;
public double DailyFatGoal { get; set; } = 67;
```

**After:**
```csharp
public double DailyCalorieGoal { get; set; } = 0;
public double DailyProteinGoal { get; set; } = 0;
public double DailyCarbsGoal { get; set; } = 0;
public double DailyFatGoal { get; set; } = 0;
```

**Reason:** DatabaseService already creates default settings on first run. Model defaults should be neutral (0) to avoid confusion.

---

## User Experience

### Scenario 1: Setting Goals to Zero
**Steps:**
1. Navigate to Settings
2. Clear the Calorie Goal field (leave empty)
3. Clear the Protein Goal field
4. Tap "Save Settings"

**Result:**
- Both goals saved as 0 in database
- Dashboard shows "1200 kcal" (no target)
- Dashboard shows "85g" (no target)
- No progress bars or percentages

### Scenario 2: Entering Negative Values
**Steps:**
1. Type "-500" in Calorie Goal field
2. Tap "Save Settings"

**Result:**
- Automatically converted to 0
- Saved as 0 in database
- Dashboard shows no goal

### Scenario 3: Mixed Empty and Set Goals
**Steps:**
1. Set Calorie Goal to 2500
2. Leave Protein Goal empty
3. Set Carbs Goal to 300
4. Leave Fat Goal empty
5. Save settings

**Result:**
- Calorie Goal: 2500
- Protein Goal: 0
- Carbs Goal: 300
- Fat Goal: 0
- Dashboard adapts display accordingly

### Scenario 4: Non-Numeric Input
**Steps:**
1. Type "abc" in Calorie Goal field
2. Tap "Save Settings"

**Result:**
- Parsed as 0 (invalid text defaults to zero)
- No error thrown
- Saved as 0 in database

---

## Validation Rules Summary

| Input | Parsed Value | Dashboard Display |
|-------|--------------|-------------------|
| Empty / Blank | 0 | No goal shown |
| "0" | 0 | No goal shown |
| "2000" | 2000 | "1200 / 2000 kcal" |
| "-500" | 0 | No goal shown |
| "abc" | 0 | No goal shown |
| "2500.5" | 2500.5 | "1200 / 2500 kcal" |

---

## Technical Details

### String to Double Conversion
Uses `double.TryParse()` for safe parsing:
- Returns `true` if successful, `false` if invalid
- Out parameter `value` contains parsed number
- No exceptions thrown on invalid input

### Empty Field Handling
- MAUI Entry with empty Text binds as `string.Empty` or `null`
- `string.IsNullOrWhiteSpace()` catches both cases
- Converts to 0 for database storage

### Negative Value Protection
```csharp
return value < 0 ? 0 : value;
```
- Simple ternary check after successful parse
- Ensures no negative goals stored
- User-friendly (no error message, just corrects)

### Display Formatting
```csharp
_settings.DailyCalorieGoal.ToString("F0")
```
- `"F0"` = Fixed-point format with 0 decimal places
- Displays as "2000" instead of "2000.0"
- Clean integer display for whole numbers

---

## Integration with Existing Features

### Dashboard Conditional Display
The existing Dashboard conditional logic (`IsCalorieGoalSet => CalorieTarget > 0`) now works perfectly:
- User clears goal → Saved as 0
- Dashboard checks if CalorieTarget > 0 → False
- Progress bar and percentage hidden
- Display shows "1200 kcal" only

### Settings Page Loading
- On page load, InitializeAsync reads from database
- If goal is 0 → Entry field is empty (blank)
- If goal is > 0 → Entry field shows number
- User sees clean slate for unset goals

---

## Edge Cases Handled

### 1. First Launch
- DatabaseService creates default UserSettings with 0 values
- Settings page shows all Entry fields empty
- Placeholder text guides user: "0 = no goal"

### 2. Decimal Values
- User can enter "2500.5"
- Parsed as 2500.5 (valid double)
- Saved and displayed correctly
- Dashboard calculations work with decimals

### 3. Very Large Numbers
- User enters "999999"
- Parsed successfully
- No upper limit validation (user responsibility)
- Dashboard displays correctly

### 4. Whitespace
- User enters "  " (spaces only)
- Detected by `IsNullOrWhiteSpace()`
- Converted to 0
- Saved as no goal

### 5. Partial Clearing
- User deletes part of "2000" leaving "20"
- On save, parsed as 20
- Valid goal, saved correctly
- Dashboard shows "X / 20 kcal"

---

## Benefits

### 1. Flexible Goal Setting
- Users can opt in/out of goals individually
- Mix and match (e.g., only track calories, ignore macros)
- No forced targets

### 2. Clean First-Run Experience
- No intimidating default goals
- Blank slate for new users
- Add goals when ready

### 3. Intuitive Editing
- Empty field = no goal (obvious)
- Clear a field to remove goal (intuitive)
- No special "delete goal" button needed

### 4. Data Integrity
- Negative values prevented
- Invalid input gracefully handled
- No crashes or errors

### 5. Dashboard Integration
- Seamless with conditional display logic
- Progress bars hide automatically
- Percentages disappear when appropriate

---

## Testing Instructions

### Test 1: Empty Field to Zero
1. Navigate to Settings
2. Clear all goal fields (leave blank)
3. Tap "Save Settings"
4. Verify success alert
5. Navigate to Dashboard
6. ✅ Verify: "1200 kcal" (no targets shown)

### Test 2: Negative to Zero
1. Settings → Enter "-1000" in Calorie Goal
2. Save settings
3. Reopen Settings page
4. ✅ Verify: Field is empty (saved as 0)

### Test 3: Mixed Goals
1. Set Calories to 2500, leave Protein blank
2. Set Carbs to 300, leave Fat blank
3. Save and go to Dashboard
4. ✅ Verify: 
   - Calories show target
   - Protein shows "Xg" only
   - Carbs show target
   - Fat shows "Xg" only

### Test 4: Invalid Input
1. Type "hello" in Calorie Goal
2. Save settings
3. Reopen Settings
4. ✅ Verify: Field is empty (saved as 0)

### Test 5: Persistence
1. Set Calorie Goal to empty
2. Save and force-close app
3. Restart app
4. Navigate to Settings
5. ✅ Verify: Calorie Goal field is empty
6. Navigate to Dashboard
7. ✅ Verify: No calorie target shown

---

## Build Status
✅ **Build Successful** - All changes compile without errors

---

## Summary of Changes

**Files Modified:**
1. ✅ `BergerBytes.App/ViewModels/SettingsPageViewModel.cs`
   - Changed to string properties
   - Added ParseGoalValue validation method
   - Updated InitializeAsync and SaveSettingsAsync

2. ✅ `BergerBytes.App/Pages/SettingsPage.xaml`
   - Updated bindings to Text properties
   - Changed placeholders to "0 = no goal"

3. ✅ `BergerBytes.Shared/Models/UserSettings.cs`
   - Changed default values from hardcoded to 0

**Validation Rules:**
- Empty → 0
- Zero → 0
- Negative → 0
- Invalid → 0
- Positive number → Use as-is

**User Experience:**
- Empty fields allowed
- No forced goals
- Clean first-run
- Dashboard adapts automatically

Users now have complete control over their goal-setting experience! 🎯✅
