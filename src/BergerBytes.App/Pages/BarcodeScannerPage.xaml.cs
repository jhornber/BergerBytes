using ZXing.Net.Maui;

namespace BergerBytes.App.Pages
{
    public partial class BarcodeScannerPage : ContentPage
    {
        private bool _hasScanned = false;

        public BarcodeScannerPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Check camera permissions
            var status = await Permissions.CheckStatusAsync<Permissions.Camera>();

            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.Camera>();
            }

            if (status != PermissionStatus.Granted)
            {
                await DisplayAlert("Camera Permission Required", 
                    "Camera access is needed to scan barcodes. Please enable camera permissions in your device settings.", 
                    "OK");
                await Shell.Current.GoToAsync("..");
                return;
            }

            // Start detecting
            CameraView.IsDetecting = true;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            CameraView.IsDetecting = false;
        }

        private async void OnBarcodesDetected(object sender, BarcodeDetectionEventArgs e)
        {
            // Prevent multiple scans
            if (_hasScanned)
                return;

            var barcode = e.Results?.FirstOrDefault();
            if (barcode == null)
                return;

            _hasScanned = true;
            CameraView.IsDetecting = false;

            // Haptic feedback
            try
            {
#if ANDROID || IOS
                HapticFeedback.Default.Perform(HapticFeedbackType.Click);
#endif
            }
            catch
            {
                // Fallback to vibration if haptic feedback not available
                try
                {
                    Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(100));
                }
                catch
                {
                    // Ignore if vibration not available
                }
            }

            // Navigate back with the barcode
            await Shell.Current.GoToAsync("..", new Dictionary<string, object>
            {
                { "ScannedBarcode", barcode.Value }
            });
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            CameraView.IsDetecting = false;
            await Shell.Current.GoToAsync("..");
        }
    }
}
