# 🍔 BergerBytes

**BergerBytes** is an AI-powered calorie/nutrition tracking application built to turn "burgers into bytes." By leveraging computer vision and massive nutritional databases, it simplifies the tedious process of food logging into a seamless, "snap-and-go" experience.

Developed as a "just for fun" project to sharpen .NET MAUI and Cloud-Native skills, this app focuses on performance, offline-first reliability, and clever AI integration.

---

## 🛠 Tech Stack

*   **Frontend:** [.NET MAUI](https://learn.microsoft.com/en-us/dotnet/maui/) (C# / XAML) targeting Android.
*   **Local Database:** [SQLite](https://www.sqlite.org/index.html) (via `sqlite-net-pcl`) for offline-first logging.
*   **Cloud Backend:** [Supabase](https://supabase.com/) (PostgreSQL, Auth, and Edge Functions).
*   **AI Vision:** [Google Gemini 1.5 Flash](https://aistudio.google.com/) for meal identification.
*   **Barcode API:** [Open Food Facts](https://world.openfoodfacts.org/data) (Open Source nutritional data).

---

## 🚀 Key Features

*   **Smart Scan:** Take a photo of your plate and let Gemini AI estimate the macros and calories.
*   **Barcode Lookup:** Scan any product barcode to instantly retrieve nutritional data.
*   **Offline First:** Log your meals even in a basement gym or a remote cabin; the app syncs to Supabase when you're back online.
*   **Fluid UI:** Built with .NET MAUI Shell for native-feeling transitions and 60 FPS performance.

---

## 📂 Project Structure (Monorepo)

This repository follows a monorepo pattern to keep shared logic and backend scripts in one place.

```text
/berger-bytes
  ├── /src
  │    ├── BergerBytes.App        # The .NET MAUI Android project
  │    ├── BergerBytes.Shared     # Shared C# Models (FoodItem, UserProfile, etc.)
  │    └── BergerBytes.Functions  # Future Supabase Edge Functions / AI logic
  ├── /supabase
  │    └── migrations             # SQL scripts for PostgreSQL schema
  ├── .gitignore                  # Standard .NET gitignore
  └── readme.md
```

---

## 🏁 Getting Started

### Prerequisites
*   **Visual Studio 2022 (Community)** with the **.NET MAUI workload** installed.
*   **Android SDK API 34+** (Manage via Android SDK Manager).
*   A **Supabase** free-tier project (for Phase 2).

### Installation
1.  Clone this repository:
    ```bash
    git clone https://github.com/your-username/berger-bytes.git
    ```
2.  Open `BergerBytes.sln` in Visual Studio.
3.  Ensure your Android Emulator is running.
4.  Press **F5** to build and deploy.

---

## 🗺 Roadmap

- [ ] **Phase 1:** Core UI Shell and Local SQLite storage.
- [ ] **Phase 2:** Barcode integration with Open Food Facts.
- [ ] **Phase 3:** AI Vision integration with Gemini 1.5 Flash.
- [ ] **Phase 4:** Cloud sync and Authentication via Supabase.

---

> *“Tracking your nutrition shouldn’t be a chore, it should be a Byte.”* — **The BergerBytes Team**