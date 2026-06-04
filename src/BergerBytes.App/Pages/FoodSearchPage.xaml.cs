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

        private async void OnScrolled(object sender, ScrolledEventArgs e)
        {
            var scrollView = (ScrollView)sender;
            var scrollingSpace = scrollView.ContentSize.Height - scrollView.Height;
            if (scrollingSpace > 0 && e.ScrollY >= scrollingSpace - 300)
            {
                await _viewModel.LoadMoreRecentItemsAsync();
            }
        }
    }
}
