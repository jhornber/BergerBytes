🍔 BergerBytes
BergerBytes is an AI-powered calorie tracking application built to turn "burgers into bytes." By leveraging computer vision and massive nutritional databases, it simplifies the tedious process of food logging into a seamless, "snap-and-go" experience.

Developed as a "just for fun" project to sharpen .NET MAUI and Cloud-Native skills, this app focuses on performance, offline-first reliability, AI integration.

🛠 Tech Stack
Frontend: .NET MAUI (C# / XAML) targeting Android.

Local Database: SQLite (via sqlite-net-pcl) for offline-first logging.

Cloud Backend: Supabase (PostgreSQL, Auth, and Edge Functions).

AI Vision: Google Gemini 1.5 Flash for meal identification.

Barcode API: Open Food Facts (Open Source nutritional data).

🚀 Key Features
Smart Scan: Take a photo of your plate and let Gemini AI estimate the macros and calories.

Barcode Lookup: Scan any product barcode to instantly retrieve nutritional data.

Offline First: Log your meals even in a basement gym or a remote cabin; the app syncs to Supabase when you're back online.

Fluid UI: Built with .NET MAUI Shell.

📂 Project Structure (Monorepo)
This repository follows a monorepo pattern to keep shared logic and backend scripts in one place.

Plaintext
/berger-bytes
  ├── /src
  │    ├── BergerBytes.App        # The .NET MAUI Android project
  │    ├── BergerBytes.Shared     # Shared C# Models (FoodItem, UserProfile, etc.)
  │    └── BergerBytes.Functions  # Future Supabase Edge Functions / AI logic
  ├── /supabase
  │    └── migrations             # SQL scripts for PostgreSQL schema
  ├── .gitignore                  # Standard .NET gitignore
  └── README.md
🏁 Getting Started
Prerequisites
Visual Studio 2026 (Community) with the .NET MAUI workload installed.

Android SDK API 34+ (Manage via Android SDK Manager).

A Supabase free-tier project (for Phase 2).

Installation
Clone this repository:

Bash
git clone https://github.com/jhornber/BergerBytes.git
Open BergerBytes.slnx in Visual Studio.

Ensure your Android Emulator is running.

Press F5 to build and deploy.

🗺 Roadmap
[ ] Phase 1: Core UI Shell and Local SQLite storage.

[ ] Phase 2: Barcode integration with Open Food Facts.

[ ] Phase 3: AI Vision integration with Gemini 1.5 Flash.

[ ] Phase 4: Cloud sync and Authentication via Supabase.

“Tracking your nutrition shouldn’t be a chore, it should be a Byte.” — The BergerBytes Team