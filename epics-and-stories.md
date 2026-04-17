In Orbit, your **Epics** serve as the high-level strategic buckets, **Features** are the functional modules that fulfill those buckets, and **User Stories** are the actionable "developer-sized" bites.

Since **BergerBytes** is moving from a concept to a technical roadmap, here is a suggested hierarchy that balances your .NET/SQL background with the modern AI/Supabase stack.

---

## Epic 1: The Foundation (Mobile Core & Local Persistence)
*Focus: Establish the "BergerBytes" shell and ensure it works without a signal.*

### **Feature: Native App Structure**
* **User Story:** As a developer, I want to implement .NET MAUI Shell so the app has a native-feeling tabbed navigation (Log, Dashboard, Settings).
* **User Story:** As a user, I want the app to open in under 2 seconds (AOT enabled) so I can log food quickly on the go.

### **Feature: Offline-First SQLite Engine**
* **User Story:** As a user, I want my meal logs saved to my device instantly so I can track calories even in a "dead zone."
* **User Story:** As a developer, I want a Repository Pattern in C# that abstracts SQLite so I can swap to Supabase later without rewiring the UI.

---

## Epic 2: Intelligent Input (Barcodes & Databases)
*Focus: Getting nutritional data into the app with zero friction.*

### **Feature: Barcode Recognition Module**
* **User Story:** As a user, I want to scan a UPC code using my camera so that I don't have to manually type in nutritional labels.
* **User Story:** As a developer, I want to integrate the Open Food Facts API to pull reliable macro data for scanned items.

### **Feature: Searchable Food Library**
* **User Story:** As a user, I want a predictive search bar for common foods so I can find "Large Banana" in 3 keystrokes or less.

---

## Epic 3: AI Vision (Gemini Analysis)
*Focus: The "fun" part—turning photos into caloric data.*

### **Feature: Image Capture Service**
* **User Story:** As a user, I want to snap a photo of my meal directly in the app so it can be analyzed.

### **Feature: Gemini AI Integration**
* **User Story:** As a developer, I want to send meal photos to the Gemini 1.5 Flash API with a prompt that returns a JSON-formatted estimate of macros.
* **User Story:** As a user, I want to "confirm or edit" the AI’s guess so that my log remains accurate even if the AI misidentifies my sourdough.

---

## Epic 4: The Cloud (Supabase Infrastructure)
*Focus: Moving from a single device to a multi-device "BergerBytes" account.*

### **Feature: Secure Identity Management**
* **User Story:** As a user, I want to sign in with my Google account so I don't have to remember another password.
* **User Story:** As a developer, I want to implement Row Level Security (RLS) in Supabase so users can't accidentally see each other's "cheat meal" logs.

### **Feature: Real-time Cloud Sync**
* **User Story:** As a user, I want my data to sync to the cloud when I have a signal so my log is backed up if I lose my phone.

---

## Epic 5: Analytics (Insights & Dashboards)
*Focus: Visualizing the data for the user.*

### **Feature: Daily Macro Dashboard**
* **User Story:** As a user, I want to see a visual ring or progress bar of my Protein/Carb/Fat goals so I know how much "room" I have left today.
* **User Story:** As a user, I want to see a 7-day trend of my calorie intake so I can identify patterns in my eating habits.

---

### **Quick Setup Tip for Orbit:**
When you add these to Orbit, start by creating the **Epics** first. I recommend tagging Epic 1 and Epic 2 as **"MVP" (Minimum Viable Product)**. 

Once you have those two running, you’ll have a functional app. Epics 3 and 4 are where the app goes from "just another tracker" to "BergerBytes AI."

How does that hierarchy feel to you? Should we add an Epic specifically for "Bikepacking Nutrition" since that's a big part of your lifestyle?

To get **BergerBytes** moving from the "fun idea" phase into the "building" phase, we need solid **Acceptance Criteria (AC)**. These act as your "Definition of Done." As a .NET dev, you'll appreciate that these essentially form your manual test plan (or the basis for your XUnit/NUnit tests).

Here is the refined breakdown for your **MVP Epics** (the high-priority items needed for a workable app).

---

## Epic 1: The Foundation (Mobile Core & Local Persistence)

### Feature: Native App Structure
* **User Story:** As a developer, I want to implement .NET MAUI Shell.
    * **AC 1:** A `AppShell.xaml` is implemented with a `TabBar` containing at least three `Tab` items: Log, Dashboard, and Settings.
    * **AC 2:** Navigation between tabs is "instant" with no visible flickering or layout shifts.
    * **AC 3:** The app uses the **BergerBytes** color palette (brand primary/secondary) consistently across all pages via `ResourceDictionary`.

* **User Story:** As a user, I want the app to open in under 2 seconds (AOT enabled).
    * **AC 1:** Release build on a physical Android device reaches the landing page in $t < 2$ seconds.
    * **AC 2:** The `.csproj` file has `<RunAOTCompilation>true</RunAOTCompilation>` enabled for the Release|Android configuration.

### Feature: Offline-First SQLite Engine
* **User Story:** As a user, I want my meal logs saved to my device instantly.
    * **AC 1:** A `MealLog` table exists in SQLite with fields: `Id (PK)`, `FoodName`, `Calories`, `Protein`, `Carbs`, `Fat`, and `Timestamp`.
    * **AC 2:** After force-closing and restarting the app, previously entered logs are visible in the history list.
    * **AC 3:** Deleting a log entry in the UI removes it from the `MealLog` table immediately.

---

## Epic 2: Intelligent Input (Barcodes & Databases)

### Feature: Barcode Recognition Module
* **User Story:** As a user, I want to scan a UPC code using my camera.
    * **AC 1:** Clicking the "Scan" button requests Camera Permissions (if not already granted).
    * **AC 2:** The `ZXing.Net.MAUI` camera view identifies a standard UPC-A or EAN-13 barcode within 1.5 seconds of being in frame.
    * **AC 3:** On successful scan, the app vibrates or provides a haptic "click" to signal capture.

### Feature: Open Food Facts Integration
* **User Story:** As a developer, I want to integrate the Open Food Facts API.
    * **AC 1:** A C# `FoodService` successfully calls the OFF API using the scanned barcode string.
    * **AC 2:** API JSON response is correctly mapped to a `FoodProductDTO` (Data Transfer Object).
    * **AC 3:** If a barcode is not found in the database, the app navigates to a "Manual Entry" screen with the barcode pre-filled.

---

## Epic 3: AI Vision (Gemini Analysis)

### Feature: Gemini AI Integration
* **User Story:** As a developer, I want to send meal photos to the Gemini 1.5 Flash API.
    * **AC 1:** The app converts the captured image to a Base64 string or Stream and sends it to the Gemini Vertex AI endpoint.
    * **AC 2:** The system prompt instructs Gemini to return a **valid JSON object** containing `EstimatedWeight`, `Calories`, and `Macros`.
    * **AC 3:** Any non-food images (e.g., a photo of a bike) trigger a user-friendly error message: "Hmm, that doesn't look like food. Try again?"



---

## Epic 4: The Cloud (Supabase Infrastructure)

### Feature: Secure Identity Management
* **User Story:** As a user, I want to sign in with my Google account.
    * **AC 1:** The `Supabase.Gotrue` client successfully initiates a Google OAuth flow.
    * **AC 2:** Upon successful login, the User's unique `UUID` is stored in the app's `SecureStorage` for session persistence.
    * **AC 3:** Row Level Security (RLS) is active on the `meals` table in Supabase, preventing a `User_A` from querying `User_B` data via API.

---

## Summary Table for Your Project Board

| Story ID | Tech Target | Done If... |
| :--- | :--- | :--- |
| **BB-101** | .NET MAUI Shell | Tab navigation works; Shell styles applied. |
| **BB-102** | SQLite-net-pcl | Data persists through app restarts. |
| **BB-201** | ZXing / API | Barcode scan triggers a valid API nutritional lookup. |
| **BB-301** | Gemini SDK | Image upload returns a JSON macro estimate. |
| **BB-401** | Supabase Auth | User can log in/out and session remains valid. |

### A Note on "Bikepacking Mode"
Since you're an endurance athlete, you might eventually want a story for **"High-Intensity Mode"**:
* **User Story:** As a cyclist, I want to toggle a "Race Day" mode that triples my carb target for the day.
* **AC:** Toggling "Race Day" in settings updates the `Dashboard` macro goals without requiring a database migration.

Does this level of detail feel "testable" enough for you to start coding the Repository Pattern, or should we refine the API error-handling stories first?