using BergerBytes.App.Services;
using BergerBytes.App.ViewModels;

namespace BergerBytes.App.Pages
{
    public partial class ExerciseLogPage : ContentPage
    {
        private readonly ExerciseLogPageViewModel _viewModel;

        public ExerciseLogPage(IExerciseLogRepository repository, IWeightLogRepository weightLogRepository)
        {
            InitializeComponent();
            _viewModel = new ExerciseLogPageViewModel(repository, weightLogRepository);
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.InitializeAsync();
        }
    }
}
