using BergerBytes.App.Services;
using BergerBytes.App.ViewModels;

namespace BergerBytes.App.Pages
{
    public partial class LogPage : ContentPage
    {
        private readonly LogPageViewModel _viewModel;

        public LogPage(IMealLogRepository repository, IExerciseLogRepository exerciseRepository)
        {
            InitializeComponent();
            _viewModel = new LogPageViewModel(repository, exerciseRepository);
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.InitializeAsync();
        }
    }
}
