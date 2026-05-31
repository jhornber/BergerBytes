using BergerBytes.App.Services;
using BergerBytes.App.ViewModels;

namespace BergerBytes.App.Pages
{
    public partial class AddMealPage : ContentPage
    {
        public AddMealPage(IMealLogRepository repository, IFoodService foodService, IRecentFoodRepository recentFoodRepository)
        {
            InitializeComponent();
            BindingContext = new AddMealPageViewModel(repository, foodService, recentFoodRepository);
        }

        protected override async void OnNavigatedTo(NavigatedToEventArgs args)
        {
            base.OnNavigatedTo(args);
            if (BindingContext is AddMealPageViewModel vm)
            {
                await vm.ProcessPendingFoodProductAsync();
                await vm.ProcessPendingBarcodeAsync();
                vm.ProcessPendingFoodEntry();
            }
        }
    }
}
