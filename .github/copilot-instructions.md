# BergerBytes Copilot Instructions

## 🛠 Tech Stack

### Frontend
- **Framework:** .NET MAUI (C#/XAML)
- **Target Platform:** Android (primary), with support for iOS, macOS Catalyst, and Windows
- **Navigation:** .NET MAUI Shell with TabBar
- **Build Optimization:** AOT Compilation enabled for Release builds

### Local Database
- **Technology:** SQLite via `sqlite-net-pcl`
- **Architecture:** Offline-first storage with Repository Pattern abstraction
- **Persistence:** Instant local saves, cloud sync on connectivity

### Cloud Backend
- **Platform:** Supabase (PostgreSQL, Auth, Edge Functions)
- **Authentication:** Google OAuth via Supabase.Gotrue
- **Security:** Row Level Security (RLS) on database tables

### AI & External Integrations
- **Computer Vision:** Google Gemini 1.5 Flash (meal photo analysis)
- **Barcode API:** Open Food Facts (nutritional database)
- **Barcode Recognition:** ZXing.Net.MAUI

### Project Structure
```
/berger-bytes
  ├── /src
  │    ├── BergerBytes.App        # .NET MAUI Android app
  │    ├── BergerBytes.Shared     # Shared C# models (FoodItem, UserProfile, etc.)
  │    └── BergerBytes.Functions  # Supabase Edge Functions / AI logic
  ├── /supabase
  │    └── migrations             # PostgreSQL schema scripts
```

---

## 📋 Project Management Style

### Hierarchy
- **Epics:** High-level strategic buckets (e.g., "The Foundation," "Intelligent Input")
- **Features:** Functional modules that fulfill epic goals (e.g., "Native App Structure," "Barcode Recognition")
- **User Stories:** Developer-sized tasks with Acceptance Criteria (AC)

### MVP Roadmap (Priority Order)
1. **Epic 1: The Foundation** – MAUI Shell, offline SQLite, local persistence
2. **Epic 2: Intelligent Input** – Barcode scanning, Open Food Facts integration
3. **Epic 3: AI Vision** – Gemini photo analysis
4. **Epic 4: Cloud Infrastructure** – Supabase sync, Google OAuth, RLS
5. **Epic 5: Analytics** – Dashboards, macro tracking, trend visualization

### Acceptance Criteria Standards
Each user story must include:
- **Functional Requirements:** What the feature does
- **Integration Points:** How it connects to other components
- **Success Metrics:** Time targets (e.g., app launch < 2 seconds), data integrity
- **Test Coverage:** Manual test scenarios

### Example Story Format
```
User Story: [Role] wants [action] so that [benefit]

AC 1: [Specific, measurable outcome]
AC 2: [Specific, measurable outcome]
AC 3: [Specific, measurable outcome]
```

---

## 🏗 Architecture Patterns

### Repository Pattern
- Abstract SQLite queries behind a Repository interface
- Enable future migration to Supabase without UI rewiring
- Use DTOs (Data Transfer Objects) for API responses

### Offline-First Design
- All meal logs written to SQLite immediately
- Sync to Supabase when network is available
- Graceful error handling for connectivity loss

### MVVM with .NET MAUI
- ViewModels for business logic and state management
- Code-behind minimal; logic in ViewModels
- Resource dictionaries for theming and consistent styling

---

## 💡 Development Practices

### Naming Conventions
- C# classes: PascalCase (e.g., `MealLogRepository`, `FoodService`)
- Methods: PascalCase (e.g., `GetMealLogs()`, `SaveMealAsync()`)
- Variables: camelCase (e.g., `mealList`, `isLoading`)
- XAML Resources: camelCase (e.g., `primaryColor`, `fontSizeBody`)

### Code Quality
- Async/await for all I/O operations
- Null-safety checks and graceful error handling
- Secure storage for sensitive data (API keys, auth tokens)
- Unit tests for business logic (Repository, Services)

### Branch Strategy
- `main` → stable release builds
- `develop` → integration branch for features
- Feature branches: `feature/BB-###-description` (e.g., `feature/BB-101-maui-shell`)

### Performance Targets
- App launch: < 2 seconds (Release build)
- Barcode scan: < 1.5 seconds recognition
- Local database queries: < 100ms
- Gemini API response: < 5 seconds (acceptable for AI)

---

## 🎯 When Coding

### Before Starting
- Check the Epic/Story acceptance criteria
- Verify no conflicting local changes in shared models
- Ensure tests are defined in the story

### During Development
- Keep commits atomic and linked to story ID (e.g., `[BB-102] Add SQLite meal table`)
- Write defensive code—assume network will fail
- Test on physical Android device for auth flows

### Code Review Focus
- AC completion (all three met?)
- Error handling for network/API failures
- Data integrity and RLS compliance
- Performance metrics met?

---

## 🚀 Special Contexts

### "Just for Fun" Mindset
- Prioritize learning .NET MAUI and Supabase
- Don't over-engineer; ship incrementally
- Bikepacking use case is the north star (endurance athlete context)

### High-Intensity / Race Day Mode (Future)
- Plan for a toggleable "Race Day" mode (tripled carb targets)
- Keep macro calculations flexible for special scenarios

---

## 📚 References

- BergerBytes README: [Design and high-level overview](../readme.md)
- Epics & Stories: [Detailed AC and roadmap](../epics-and-stories.md)
- Microsoft MAUI Docs: https://learn.microsoft.com/en-us/dotnet/maui/
- Supabase Docs: https://supabase.com/docs
- Google Gemini API: https://aistudio.google.com/
