using BergerBytes.App.Services;
using BergerBytes.App.ViewModels;

namespace BergerBytes.App.Pages
{
    public partial class LogPage : ContentPage
    {
        private readonly LogPageViewModel _viewModel;

        public LogPage(IMealLogRepository repository)
        {
            InitializeComponent();
            _viewModel = new LogPageViewModel(repository);
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.InitializeAsync();
        }
    }
}
