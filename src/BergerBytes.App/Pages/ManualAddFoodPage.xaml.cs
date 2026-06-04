using BergerBytes.App.ViewModels;

namespace BergerBytes.App.Pages
{
    public partial class ManualAddFoodPage : ContentPage
    {
        public ManualAddFoodPage()
        {
            InitializeComponent();
            BindingContext = new ManualAddFoodViewModel();
        }
    }
}
