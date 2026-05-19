using BarcodeScanning;

namespace BergerBytes.App.Pages
{
    public partial class BarcodeScannerPage : ContentPage
    {
        private bool _hasScanned;
        private bool _isReady;

        public BarcodeScannerPage()
        {
            InitializeComponent();

            //cameraView.BarcodeSymbologies = BarcodeFormats.OneDCode;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Check camera permissions
            var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
            var vibrateStatus = await Permissions.CheckStatusAsync<Permissions.Vibrate>();

            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.Camera>();
            }

            if (vibrateStatus != PermissionStatus.Granted)
            {
                vibrateStatus = await Permissions.RequestAsync<Permissions.Vibrate>();
            }

            if (status != PermissionStatus.Granted)
            {
                await DisplayAlertAsync("Camera Permission Required", 
                    "Camera access is needed to scan barcodes. Please enable camera permissions in your device settings.", 
                    "OK");
                await Shell.Current.GoToAsync("..");
                return;
            }

            cameraView.CameraEnabled = true;

            // Brief delay so the user can position the camera before capture begins
            await Task.Delay(1700);
            _isReady = true;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            cameraView.CameraEnabled = false;
            _isReady = false;
        }

        private async void OnBarcodesDetected(object sender, OnDetectionFinishedEventArg e)
        {
            // Prevent scans before ready or multiple scans
            if (!_isReady || _hasScanned)
                return;

            if(e.BarcodeResults.Count > 0)
            {
                var barcode = e.BarcodeResults?.FirstOrDefault();
                if (barcode == null)
                    return;

                _hasScanned = true;
                MainThread.BeginInvokeOnMainThread(() => cameraView.CameraEnabled = false);


                // Navigate back with the barcode
                await Shell.Current.GoToAsync("..", new Dictionary<string, object>
            {
                { "ScannedBarcode", barcode.DisplayValue }
            });

                // Vibrate on successful scan
                try
                {
                    Vibration.Vibrate(TimeSpan.FromMilliseconds(200));
                }
                catch (Exception ex)
                {
                    // Log vibration error (could use ILogger here in future)
                    System.Diagnostics.Debug.WriteLine($"Vibration error: {ex.Message}");
                }
            }

            
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            cameraView.CameraEnabled = false;
            await Shell.Current.GoToAsync("..");
        }
    }
}
