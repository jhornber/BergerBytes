using Microsoft.Extensions.Logging;
using BergerBytes.App.Services;
using BergerBytes.App.Pages;
using BergerBytes.App.ViewModels;
using BarcodeScanning;
using MauiIcons.Material;

namespace BergerBytes.App
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseBarcodeScanning()
                .UseMaterialMauiIcons()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            // Register services
            builder.Services.AddSingleton<IMealLogRepository, DatabaseService>();
            builder.Services.AddSingleton<ISettingsRepository, DatabaseService>();
            builder.Services.AddSingleton<IWeightLogRepository, DatabaseService>();
            builder.Services.AddSingleton<IExerciseLogRepository, DatabaseService>();
            builder.Services.AddSingleton<IRecentFoodRepository, DatabaseService>();
            builder.Services.AddSingleton<HttpClient>();
            builder.Services.AddSingleton<IFoodService, FoodService>();
            builder.Services.AddSingleton<App>();

            // Register pages
            builder.Services.AddTransient<LogPage>();
            builder.Services.AddTransient<DashboardPage>();
            builder.Services.AddTransient<SettingsPage>();
            builder.Services.AddTransient<AddMealPage>();
            builder.Services.AddTransient<EditMealPage>();
            builder.Services.AddTransient<BarcodeScannerPage>();
            builder.Services.AddTransient<ServingSizePromptPage>();
            builder.Services.AddTransient<FoodSearchPage>();
            builder.Services.AddTransient<WeightLogPage>();
            builder.Services.AddTransient<ExerciseLogPage>();

            // Register ViewModels
            builder.Services.AddTransient<EditMealPageViewModel>();
            builder.Services.AddTransient<FoodSearchPageViewModel>();

            return builder.Build();
        }
    }
}
