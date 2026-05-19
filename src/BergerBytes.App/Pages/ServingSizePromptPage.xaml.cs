using BergerBytes.App.ViewModels;

namespace BergerBytes.App.Pages
{
    public partial class ServingSizePromptPage : ContentPage
    {
        public ServingSizePromptPage()
        {
            InitializeComponent();
            BindingContext = new ServingSizePromptPageViewModel();
        }
    }
}
