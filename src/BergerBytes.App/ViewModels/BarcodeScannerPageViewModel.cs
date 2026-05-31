using System.Windows.Input;

namespace BergerBytes.App.ViewModels
{
    public class BarcodeScannerPageViewModel : BindableObject
    {
        private bool _cameraEnabled;
        private bool _isReady;
        private bool _hasScanned;

        public bool CameraEnabled
        {
            get => _cameraEnabled;
            set { _cameraEnabled = value; OnPropertyChanged(); }
        }

        public ICommand CancelCommand { get; }

        public BarcodeScannerPageViewModel()
        {
            CancelCommand = new Command(async () => await CancelAsync());
        }

        public async Task InitializeAsync()
        {
            _hasScanned = false;
            _isReady = false;

            var cameraStatus = await Permissions.CheckStatusAsync<Permissions.Camera>();
            var vibrateStatus = await Permissions.CheckStatusAsync<Permissions.Vibrate>();

            if (cameraStatus != PermissionStatus.Granted)
                cameraStatus = await Permissions.RequestAsync<Permissions.Camera>();

            if (vibrateStatus != PermissionStatus.Granted)
                await Permissions.RequestAsync<Permissions.Vibrate>();

            if (cameraStatus != PermissionStatus.Granted)
            {
                await Application.Current!.MainPage!.DisplayAlertAsync(
                    "Camera Permission Required",
                    "Camera access is needed to scan barcodes. Please enable camera permissions in your device settings.",
                    "OK");
                await Shell.Current.GoToAsync("..");
                return;
            }

            CameraEnabled = true;

            // Brief delay so the user can position the camera before capture begins
            await Task.Delay(1700);
            _isReady = true;
        }

        public async Task HandleBarcodeDetectedAsync(string displayValue)
        {
            if (!_isReady || _hasScanned)
                return;

            _hasScanned = true;
            MainThread.BeginInvokeOnMainThread(() => CameraEnabled = false);

            await Shell.Current.GoToAsync("..", new Dictionary<string, object>
            {
                { "ScannedBarcode", displayValue }
            });

            try
            {
                Vibration.Vibrate(TimeSpan.FromMilliseconds(200));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Vibration error: {ex.Message}");
            }
        }

        public void OnPageDisappearing()
        {
            CameraEnabled = false;
            _isReady = false;
        }

        private async Task CancelAsync()
        {
            CameraEnabled = false;
            await Shell.Current.GoToAsync("..");
        }
    }
}
