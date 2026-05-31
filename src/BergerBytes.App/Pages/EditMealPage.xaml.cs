using BergerBytes.App.ViewModels;
using BergerBytes.Shared.Models;

namespace BergerBytes.App.Pages
{
    [QueryProperty(nameof(MealGroup), "mealGroup")]
    public partial class EditMealPage : ContentPage
    {
        private readonly EditMealPageViewModel _viewModel;
        private bool _initialized = false;

        public MealGroup? MealGroup { get; set; }

        public EditMealPage(EditMealPageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (!_initialized && MealGroup != null)
            {
                _viewModel.Initialize(MealGroup);
                _initialized = true;
            }
        }

        protected override async void OnNavigatedTo(NavigatedToEventArgs args)
        {
            base.OnNavigatedTo(args);
            await _viewModel.ProcessPendingFoodProductAsync();
            await _viewModel.ProcessPendingBarcodeAsync();
            await _viewModel.ProcessPendingFoodEntryAsync();
        }
    }
}
