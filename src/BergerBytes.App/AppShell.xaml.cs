using BergerBytes.App.Pages;

namespace BergerBytes.App
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Register routes for navigation
            Routing.RegisterRoute("addmeal", typeof(AddMealPage));
            Routing.RegisterRoute("editmeal", typeof(EditMealPage));
            Routing.RegisterRoute("BarcodeScannerPage", typeof(BarcodeScannerPage));
            Routing.RegisterRoute("ServingSizePromptPage", typeof(ServingSizePromptPage));
        }
    }
}
