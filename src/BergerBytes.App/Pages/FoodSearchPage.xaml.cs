using BergerBytes.App.ViewModels;

namespace BergerBytes.App.Pages
{
    public partial class FoodSearchPage : ContentPage
    {
        private readonly FoodSearchPageViewModel _viewModel;

        public FoodSearchPage(FoodSearchPageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadRecentItemsAsync();
        }
    }
}
