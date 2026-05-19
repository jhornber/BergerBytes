using BergerBytes.App.ViewModels;
using BergerBytes.Shared.Models;

namespace BergerBytes.App.Pages
{
    [QueryProperty(nameof(MealGroup), "mealGroup")]
    public partial class EditMealPage : ContentPage
    {
        private readonly EditMealPageViewModel _viewModel;

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

            if (MealGroup != null)
            {
                _viewModel.Initialize(MealGroup);
            }
        }
    }
}
