using BergerBytes.App.Pages;
using MauiIcons.Core;

namespace BergerBytes.App
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            // Workaround for dotnet/maui#7503 - required for URL-style xmlns namespaces
            _ = new MauiIcon();

            // Register routes for navigation
            Routing.RegisterRoute("addmeal", typeof(AddMealPage));
            Routing.RegisterRoute("editmeal", typeof(EditMealPage));
            Routing.RegisterRoute("BarcodeScannerPage", typeof(BarcodeScannerPage));
            Routing.RegisterRoute("ServingSizePromptPage", typeof(ServingSizePromptPage));
            Routing.RegisterRoute("FoodSearchPage", typeof(FoodSearchPage));
            Routing.RegisterRoute("WeightLogPage", typeof(WeightLogPage));
            Routing.RegisterRoute("ExerciseLogPage", typeof(ExerciseLogPage));
            Routing.RegisterRoute("ManualAddFoodPage", typeof(ManualAddFoodPage));
        }
    }
}
