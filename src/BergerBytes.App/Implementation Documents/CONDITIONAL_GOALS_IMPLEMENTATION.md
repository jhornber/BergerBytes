# Conditional Goal Display Implementation

## Summary
Enhanced the Dashboard to conditionally display goal percentages and targets based on whether goals are set above zero, providing a cleaner UX when goals are not configured.

---

## Changes Made

### 1. DashboardPageViewModel Updates

#### New Percentage Properties
Added individual percentage calculations for all macros:
```csharp
public double CaloriesPercentage => CalorieTarget > 0 ? (CaloriesConsumed / CalorieTarget) * 100 : 0;
public double ProteinPercentage => ProteinTarget > 0 ? (ProteinConsumed / ProteinTarget) * 100 : 0;
public double CarbsPercentage => CarbsTarget > 0 ? (CarbsConsumed / CarbsTarget) * 100 : 0;
public double FatPercentage => FatTarget > 0 ? (FatConsumed / FatTarget) * 100 : 0;
```

#### New Goal Set Flag
```csharp
public bool IsCalorieGoalSet => CalorieTarget > 0;
```

#### Smart Display Strings
Enhanced display logic to show goals and percentages only when set:

**Calories:**
- **Goal set (> 0):** `"1200 / 2000 kcal"`
- **No goal (0 or null):** `"1200 kcal"`

**Macros (Protein, Carbs, Fat):**
- **Goal set (> 0):** `"85g / 150g (57%)"`
- **No goal (0 or null):** `"85g"`

### 2. DashboardPage.xaml Updates

#### Conditional Progress Bar
The calorie progress bar and percentage label now hide when no goal is set:
```xaml
<ProgressBar Progress="{Binding CaloriesPercentage}" 
             IsVisible="{Binding IsCalorieGoalSet}" />

<Label Text="{Binding CaloriesPercentage, StringFormat='{0:F0}% of daily goal'}" 
       IsVisible="{Binding IsCalorieGoalSet}" />
```

#### Adjusted Macro Font Size
Reduced font size from 18 to 14 to accommodate longer text with percentages:
```xaml
<Label Text="{Binding ProteinDisplay}" 
       FontSize="14" 
       HorizontalTextAlignment="Center" />
```

### 3. Property Change Notifications
Updated all target and consumed property setters to notify about percentage changes:
```csharp
public double ProteinTarget
{
    set
    {
        _proteinTarget = value;
        OnPropertyChanged();
        OnPropertyChanged(nameof(ProteinDisplay));
        OnPropertyChanged(nameof(ProteinPercentage)); // NEW
    }
}
```

---

## User Experience

### Scenario 1: All Goals Set (Default)
**Settings:**
- Calorie Goal: 2000
- Protein Goal: 150g
- Carbs Goal: 250g
- Fat Goal: 67g

**Dashboard Display:**
- Calories: `"1200 / 2000 kcal"`
- Progress bar visible
- Percentage label: `"60% of daily goal"`
- Protein: `"85g / 150g (57%)"`
- Carbs: `"120g / 250g (48%)"`
- Fat: `"45g / 67g (67%)"`

### Scenario 2: No Calorie Goal Set
**Settings:**
- Calorie Goal: 0 (or not set)
- Protein Goal: 150g

**Dashboard Display:**
- Calories: `"1200 kcal"` (no target shown)
- Progress bar **hidden**
- Percentage label **hidden**
- Protein: `"85g / 150g (57%)"` (still shows because goal is set)

### Scenario 3: Mixed Goals
**Settings:**
- Calorie Goal: 2500
- Protein Goal: 0
- Carbs Goal: 300g
- Fat Goal: 0

**Dashboard Display:**
- Calories: `"1200 / 2500 kcal"` (48% progress)
- Protein: `"85g"` (no goal shown)
- Carbs: `"120g / 300g (40%)"`
- Fat: `"45g"` (no goal shown)

### Scenario 4: No Goals Set
**Settings:**
- All goals set to 0

**Dashboard Display:**
- Calories: `"1200 kcal"`
- No progress bar
- No percentages
- Protein: `"85g"`
- Carbs: `"120g"`
- Fat: `"45g"`
- Dashboard becomes a simple tracking view without goals

---

## Technical Details

### Display Logic
All display properties use ternary operators to conditionally format:
```csharp
public string ProteinDisplay => ProteinTarget > 0
    ? $"{ProteinConsumed:F0}g / {ProteinTarget:F0}g ({ProteinPercentage:F0}%)"
    : $"{ProteinConsumed:F0}g";
```

### Percentage Calculation Safety
Prevents division by zero:
```csharp
public double ProteinPercentage => ProteinTarget > 0 
    ? (ProteinConsumed / ProteinTarget) * 100 
    : 0;
```

### UI Visibility Binding
XAML `IsVisible` property reacts to ViewModel flag:
```xaml
IsVisible="{Binding IsCalorieGoalSet}"
```

### Reactive Updates
All property changes cascade to dependent display properties:
- Setting `CalorieTarget` → updates `CaloriesDisplay`, `CaloriesPercentage`, `IsCalorieGoalSet`
- Setting `CaloriesConsumed` → updates `CaloriesDisplay`, `CaloriesPercentage`

---

## Benefits

### 1. Cleaner UI
- No confusing "X / 0 kcal" displays
- No "Infinity%" or "NaN%" edge cases
- Progress bar doesn't show when meaningless

### 2. Flexible Tracking Modes
Users can now:
- Track intake without setting goals (simple logging)
- Set goals for some macros but not others
- Gradually add goals as they become comfortable

### 3. Better First-Run Experience
- New users see clean numbers without intimidating goals
- Can start tracking immediately
- Add goals when ready

### 4. Race Day / Special Scenarios
- Temporarily remove calorie goal for intuitive eating days
- Focus on specific macros (e.g., only track protein)
- Dashboard adapts automatically

---

## Testing Scenarios

### Test 1: Default State
1. Fresh install with default goals (2000 kcal, etc.)
2. Add sample meal (500 calories)
3. Navigate to Dashboard
4. ✅ Verify: `"500 / 2000 kcal"`, progress bar at 25%

### Test 2: Remove Calorie Goal
1. Navigate to Settings
2. Change calorie goal to 0
3. Save settings
4. Return to Dashboard
5. ✅ Verify: `"500 kcal"` (no target), no progress bar, no percentage

### Test 3: Mixed Goals
1. Set: Calories = 2500, Protein = 0, Carbs = 250, Fat = 0
2. Add meal with 30g protein
3. Navigate to Dashboard
4. ✅ Verify: 
   - Calories show target and %
   - Protein shows `"30g"` only
   - Carbs show target and %
   - Fat shows `"0g"` only

### Test 4: Re-add Goals
1. Start with all goals at 0
2. Dashboard shows simple numbers
3. Go to Settings, set calorie goal to 2000
4. Save and return to Dashboard
5. ✅ Verify: Progress bar appears, percentage shown

---

## Code Quality

### Null Safety
All calculations check for zero/null before dividing:
```csharp
CalorieTarget > 0 ? ... : 0
```

### MVVM Compliance
- All logic in ViewModel
- XAML only binds and displays
- No code-behind business logic

### Performance
- Computed properties (no caching needed for simple formatting)
- Minimal UI redraws (only affected properties notify)

### Maintainability
- Consistent pattern across all four macros
- Easy to extend for future goals (e.g., fiber, sugar)

---

## Future Enhancements (Not Implemented)

### Goal Presets
- "Cutting" (calorie deficit)
- "Bulking" (calorie surplus)
- "Maintenance" (balanced)
- One-tap apply

### Visual Indicators
- Green progress bar when under goal
- Yellow when approaching goal
- Red when over goal

### Contextual Messages
- "No calorie goal set. Tap here to set one."
- "Great job! You've hit 75% of your protein goal!"

### Goal History
- Track goal changes over time
- "You've increased your protein goal 3 times this month"

---

## Build Status
✅ **Build Successful** - All changes compile without errors

---

## Summary of User-Facing Changes

**Before:**
- Always showed "X / Y kcal" even when Y was 0
- Progress bar always visible
- Macros showed only consumed amount

**After:**
- Calories: Shows target **only if set** (> 0)
- Progress bar: Visible **only if goal set**
- Percentage: Shown **only if goal set**
- Macros: Show "consumed / target (percentage)" **if goal > 0**, else just "consumed"
- Clean, adaptive UI that works with or without goals

Users now have full control over what they track and what goals they set! 🎯📊
