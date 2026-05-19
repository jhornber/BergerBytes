# Add Meal Feature Implementation

## Summary
Implemented a comprehensive meal logging system where users can categorize meals (Breakfast, Lunch, Dinner, Other) and add multiple food items per meal session with detailed nutrition information.

---

## New Components Created

### 1. MealType Enum (BergerBytes.Shared/Models)
**File:** `MealType.cs`

```csharp
public enum MealType
{
    Breakfast = 0,
    Lunch = 1,
    Dinner = 2,
    Other = 3
}
```

Purpose: Categorizes meals for better organization and future analytics.

### 2. FoodEntry Model (BergerBytes.Shared/Models)
**File:** `FoodEntry.cs`

Temporary model for building up a meal before saving:
- **Properties:** Name, CaloriesText, ProteinText, CarbsText, FatText
- **Validation:** `IsValid` checks Name and Calories are provided
- **Parsing:** Safe double parsing with negative value protection
- **Required:** Name + Calories
- **Optional:** Protein, Carbs, Fat

### 3. Updated MealLog Model
**File:** `MealLog.cs` (Modified)

Added:
- `MealType MealType` property (stored as integer in SQLite)
- `MealTypeDisplay` computed property for UI display with emojis:
  - 🌅 Breakfast
  - ☀️ Lunch
  - 🌙 Dinner
  - 🍽️ Other

### 4. AddMealPage (BergerBytes.App/Pages)
**Files:** `AddMealPage.xaml`, `AddMealPage.xaml.cs`

Comprehensive meal entry form with:

#### Meal Type Selection
- Picker dropdown with 4 meal categories
- Defaults to "Other"
- Selected once per session (all items share the category)

#### Current Food Entry Form
- **Food Name** (required): Text entry
- **Calories** (required): Numeric entry
- **Protein** (optional): Numeric entry, defaults to 0
- **Carbs** (optional): Numeric entry, defaults to 0
- **Fat** (optional): Numeric entry, defaults to 0
- **Add This Item** button

#### Added Items List
- CollectionView showing all added items
- Swipe-to-remove functionality
- Displays: Name, Calories, P/C/F breakdown
- Real-time count display

#### Meal Total Summary
- Dynamic calculation of totals across all items
- Shows total calories and macros
- Only visible when items exist
- Styled with brand color background

#### Action Buttons
- **Cancel**: Confirms discard if items added
- **Save Meal**: Saves all items to database

### 5. AddMealPageViewModel (BergerBytes.App/ViewModels)
**File:** `AddMealPageViewModel.cs`

Features:
- **ObservableCollection<FoodEntry>**: Holds items before saving
- **Current Entry Properties**: Two-way bound to form fields
- **Validation**: Checks Name + Calories required
- **Add Item Logic**: Validates, creates FoodEntry, clears form
- **Remove Item**: Swipe-to-delete support
- **Totals Calculation**: Real-time sum of all items
- **Save Logic**: Creates MealLog for each item with shared timestamp/type
- **Cancel Logic**: Confirms if unsaved changes exist

---

## User Flow

### Step 1: Navigate to Add Meal
1. Tap "**+ Add Meal**" on Log page
2. Navigates to AddMealPage

### Step 2: Select Meal Type
1. Tap Meal Type picker
2. Choose: Breakfast, Lunch, Dinner, or Other
3. Selection applies to all items in this session

### Step 3: Add First Food Item
1. Enter food name (e.g., "Chicken Breast")
2. Enter calories (e.g., "250")
3. Optionally enter macros (P: 45, C: 0, F: 5)
4. Tap "**+ Add This Item**"
5. Item appears in list below
6. Form clears for next item

### Step 4: Add More Items (Optional)
1. Repeat Step 3 for additional foods
2. Build up your complete meal
3. See running total at bottom

### Step 5: Save or Cancel
**Save:**
- Tap "**Save Meal**"
- All items saved to database
- Each gets same timestamp and meal type
- Success confirmation
- Returns to Log page

**Cancel:**
- Tap "**Cancel**"
- If items added, confirm discard
- Returns to Log page without saving

---

## Data Flow

### Adding Items
```
User enters data → Validation → FoodEntry created → Added to ObservableCollection → UI updates
```

### Saving Meal
```
Tap Save → For each FoodEntry:
  Create MealLog with:
    - FoodName = entry.Name
    - Calories/Protein/Carbs/Fat = parsed values
    - MealType = selected type
    - Timestamp = DateTime.Now (same for all items)
  Save to SQLite via repository
→ Success alert → Navigate back
```

### Meal Display
```
Log page loads → Repository GetAllAsync() → OrderBy Timestamp descending → Display with badges
```

---

## UI Features

### Validation
- **Visual Feedback**: Red border on invalid fields (future enhancement)
- **Alert Dialogs**: Clear error messages
- **Required Field Indicators**: Asterisk (*) on Name and Calories
- **Optional Label**: "Macros (Optional)" section header

### Smart Defaults
- Empty macro fields default to "0" when saving
- Negative values converted to 0
- Invalid numbers default to 0
- Meal type defaults to "Other"

### Real-Time Feedback
- Item count updates as you add/remove
- Total calculations update instantly
- Save button always enabled (validation on tap)

### Swipe Actions
- Swipe left on added item → "Remove" button
- Tap Remove → Item deleted from list
- No confirmation (easy to re-add)

### Meal Type Badges (Log Page)
- Each log entry shows colored badge
- Emojis for visual recognition:
  - 🌅 Breakfast (morning)
  - ☀️ Lunch (midday)
  - 🌙 Dinner (evening)
  - 🍽️ Other (anytime)

---

## Database Schema Changes

### MealLog Table (Updated)
```sql
ALTER TABLE MealLog ADD COLUMN MealType INTEGER DEFAULT 3;
```

SQLite stores enum as integer:
- 0 = Breakfast
- 1 = Lunch
- 2 = Dinner
- 3 = Other

**Migration:** Existing records will get `MealType = 3 (Other)` by default.

---

## Navigation Setup

### AppShell Route Registration
```csharp
Routing.RegisterRoute("addmeal", typeof(AddMealPage));
```

### Navigation Commands
```csharp
// Navigate to AddMealPage
await Shell.Current.GoToAsync("addmeal");

// Navigate back
await Shell.Current.GoToAsync("..");
```

---

## Validation Rules

| Field | Required | Validation | Default on Empty/Invalid |
|-------|----------|------------|--------------------------|
| Food Name | ✅ Yes | Non-empty string | Alert shown |
| Calories | ✅ Yes | Numeric, >= 0 | Alert shown |
| Protein | ❌ No | Numeric, >= 0 | 0 |
| Carbs | ❌ No | Numeric, >= 0 | 0 |
| Fat | ❌ No | Numeric, >= 0 | 0 |

### Validation Messages
- **Missing Name**: "Please enter a food name."
- **Missing Calories**: "Please enter calories."
- **No Items on Save**: "Please add at least one food item."

---

## Example User Scenarios

### Scenario 1: Simple Breakfast
1. Tap "+ Add Meal"
2. Select "🌅 Breakfast"
3. Add "Oatmeal": 300 cal, P: 10g, C: 54g, F: 6g
4. Tap "Save Meal"
5. Log shows: **[🌅 Breakfast] Oatmeal - 300 calories**

### Scenario 2: Multi-Item Lunch
1. Tap "+ Add Meal"
2. Select "☀️ Lunch"
3. Add "Grilled Chicken": 250 cal, P: 45g, C: 0g, F: 5g
4. Add "Brown Rice": 215 cal, P: 5g, C: 45g, F: 2g
5. Add "Broccoli": 55 cal, P: 4g, C: 11g, F: 0.5g
6. **Total shows: 520 calories | P: 54g  C: 56g  F: 7.5g**
7. Tap "Save Meal"
8. Log shows **3 separate entries**, all timestamped the same, all tagged "☀️ Lunch"

### Scenario 3: Quick Snack (Minimal Info)
1. Tap "+ Add Meal"
2. Select "🍽️ Other"
3. Add "Protein Bar": 200 cal (leave macros empty)
4. Tap "Save Meal"
5. Log shows: **[🍽️ Other] Protein Bar - 200 calories | P: 0g  C: 0g  F: 0g**

### Scenario 4: Cancel with Unsaved Changes
1. Tap "+ Add Meal"
2. Add 2 items
3. Tap "Cancel"
4. Alert: "You have unsaved items. Are you sure you want to cancel?"
5. Tap "Yes" → Returns to Log page, nothing saved
6. Tap "No" → Stays on Add Meal page

---

## Integration with Existing Features

### Dashboard
- Dashboard continues to sum all meal logs
- Meal type not displayed on Dashboard (just totals)
- Works seamlessly with updated MealLog schema

### Settings
- No impact on Settings page
- Goals apply to daily totals regardless of meal type

### Database Service
- DatabaseService creates MealLog table with new column
- Existing repositories work without changes (backward compatible)

---

## Future Enhancements (Not Yet Implemented)

Per user request, these are planned but not implemented:

### Recent Foods
- Show list of recently logged items
- Quick re-log with one tap
- Edit quantities before re-adding

### Favorites/Common Foods
- Save frequently eaten foods
- Quick access library
- Searchable by name

### Copy Meal
- Duplicate entire meal with one tap
- Edit before saving
- Useful for repetitive eating patterns

### Notes Field
- Optional text field per item
- Examples: "Restaurant: Chipotle", "Homemade", "Meal prep batch #3"
- Display in log view

### Portion Sizes
- Multiplier field (e.g., "2x")
- Auto-calculate nutrition values
- Useful for servings > 1

### Meal Grouping (Display)
- Group items by timestamp + meal type
- Show meal-level totals
- Collapse/expand items

---

## Testing Instructions

### Test 1: Basic Meal Entry
1. Navigate to Log tab
2. Tap "+ Add Meal"
3. Select "Breakfast"
4. Add "Toast": 100 cal, P: 3g, C: 18g, F: 1g
5. Tap "Save Meal"
6. ✅ Verify: Log shows item with 🌅 Breakfast badge

### Test 2: Multi-Item Meal
1. Tap "+ Add Meal"
2. Select "Lunch"
3. Add 3 items with different values
4. ✅ Verify: Meal Total updates after each add
5. Tap "Save Meal"
6. ✅ Verify: Log shows 3 separate entries, all with ☀️ Lunch badge

### Test 3: Validation
1. Tap "+ Add Meal"
2. Leave Name empty, tap "Add This Item"
3. ✅ Verify: Alert shown
4. Enter Name only, leave Calories empty
5. ✅ Verify: Alert shown
6. Enter both Name and Calories
7. ✅ Verify: Item added successfully

### Test 4: Optional Macros
1. Add item with only Name + Calories (no macros)
2. Save meal
3. Navigate to Dashboard
4. ✅ Verify: Calories counted, macros show as 0

### Test 5: Cancel Confirmation
1. Tap "+ Add Meal"
2. Add 2 items
3. Tap "Cancel"
4. ✅ Verify: Confirmation dialog shown
5. Tap "No" → stays on page
6. Tap "Cancel" again → "Yes" → returns without saving
7. ✅ Verify: Items not in log

### Test 6: Swipe to Remove
1. Tap "+ Add Meal"
2. Add 3 items
3. Swipe left on middle item
4. Tap "Remove"
5. ✅ Verify: Item removed, total updates

### Test 7: Meal Type Badges
1. Add meals for each type (Breakfast, Lunch, Dinner, Other)
2. Navigate to Log tab
3. ✅ Verify: Each shows correct emoji and label

---

## Code Quality

### Separation of Concerns
- **FoodEntry**: Temporary UI model (not persisted)
- **MealLog**: Persistent domain model
- **ViewModel**: Business logic and validation
- **Page**: UI presentation only

### Validation Pattern
```csharp
// Two-stage validation:
1. FoodEntry.IsValid (model-level)
2. ViewModel checks before adding to list
3. Alert feedback to user
```

### Safe Parsing
```csharp
public double GetCalories() => 
    double.TryParse(CaloriesText, out var val) && val >= 0 
        ? val 
        : 0;
```
- No exceptions thrown
- Negative values rejected
- Defaults to 0 on failure

### Observable Pattern
```csharp
FoodEntries.CollectionChanged += (s, e) =>
{
    OnPropertyChanged(nameof(TotalCalories));
    // etc.
};
```
- Real-time UI updates
- Automatic total recalculation

---

## Build Status
✅ **Build Successful** - All components compile without errors

---

## Files Modified/Created

### Created
1. ✅ `BergerBytes.Shared/Models/MealType.cs`
2. ✅ `BergerBytes.Shared/Models/FoodEntry.cs`
3. ✅ `BergerBytes.App/Pages/AddMealPage.xaml`
4. ✅ `BergerBytes.App/Pages/AddMealPage.xaml.cs`
5. ✅ `BergerBytes.App/ViewModels/AddMealPageViewModel.cs`

### Modified
1. ✅ `BergerBytes.Shared/Models/MealLog.cs` - Added MealType property
2. ✅ `BergerBytes.App/ViewModels/LogPageViewModel.cs` - Changed to navigation command
3. ✅ `BergerBytes.App/Pages/LogPage.xaml` - Added meal type badges, changed button text
4. ✅ `BergerBytes.App/AppShell.xaml.cs` - Registered "addmeal" route
5. ✅ `BergerBytes.App/MauiProgram.cs` - Registered AddMealPage

---

## Summary of User-Facing Features

Users can now:
- ✅ Categorize meals (Breakfast, Lunch, Dinner, Other)
- ✅ Add multiple food items in one session
- ✅ Enter Name (required) + Calories (required)
- ✅ Optionally enter Protein, Carbs, Fat
- ✅ See real-time meal totals before saving
- ✅ Remove items with swipe gesture
- ✅ Cancel with unsaved changes confirmation
- ✅ View meal type badges in log
- ✅ All items in a session share timestamp + meal type

The meal logging experience is now comprehensive and user-friendly! 🍔✨
