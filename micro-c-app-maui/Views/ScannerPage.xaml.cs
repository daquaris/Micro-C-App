using System;
using ZXing.Net.Maui;

namespace micro_c_app_maui.Views
{
    // Simplified port of the classic app's Views/ScannerPage.xaml.cs for the phase-1 vertical slice -
    // item-mode scanning only (the Serial-number mode used by the Quote flow isn't ported yet).
    public partial class ScannerPage : ContentPage
    {
        public event Action<string> OnScanResult;

        // ZXing keeps firing OnBarcodesDetected on every processed frame, including ones already
        // queued for the main thread when the first valid hit comes in - without this, a single scan
        // can invoke OnScanResult (and so pop/submit) more than once.
        private bool hasDetected;

        public ScannerPage()
        {
            InitializeComponent();
        }

        private void OnBarcodesDetected(object sender, BarcodeDetectionEventArgs e)
        {
            if (hasDetected)
            {
                return;
            }

            var value = e.Results?.Length > 0 ? e.Results[0].Value : null;
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            hasDetected = true;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                OnScanResult?.Invoke(value);
            });
        }

        private void TorchClicked(object sender, EventArgs e)
        {
            scanner.IsTorchOn = !scanner.IsTorchOn;
        }
    }
}
