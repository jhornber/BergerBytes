using BergerBytes.App.Services;
using BergerBytes.App.ViewModels;

namespace BergerBytes.App.Pages
{
    public partial class AddMealPage : ContentPage
    {
        public AddMealPage(IMealLogRepository repository, IFoodService foodService)
        {
            InitializeComponent();
            BindingContext = new AddMealPageViewModel(repository, foodService);
        }
    }
}
