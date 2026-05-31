using BarcodeScanning;
using BergerBytes.App.ViewModels;

namespace BergerBytes.App.Pages
{
    public partial class BarcodeScannerPage : ContentPage
    {
        private BarcodeScannerPageViewModel Vm => (BarcodeScannerPageViewModel)BindingContext;

        public BarcodeScannerPage()
        {
            InitializeComponent();
            BindingContext = new BarcodeScannerPageViewModel();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await Vm.InitializeAsync();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Vm.OnPageDisappearing();
        }

        private async void OnBarcodesDetected(object sender, OnDetectionFinishedEventArg e)
        {
            var barcode = e.BarcodeResults?.FirstOrDefault();
            if (barcode != null)
                await Vm.HandleBarcodeDetectedAsync(barcode.DisplayValue);
        }
    }
}

