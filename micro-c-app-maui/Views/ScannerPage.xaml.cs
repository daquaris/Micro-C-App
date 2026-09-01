using System;
using ZXing.Net.Maui;

namespace micro_c_app_maui.Views
{
    // Simplified port of the classic app's Views/ScannerPage.xaml.cs for the phase-1 vertical slice -
    // item-mode scanning only (the Serial-number mode used by the Quote flow isn't ported yet).
    public partial class ScannerPage : ContentPage
    {
        public event Action<string> OnScanResult;

        public ScannerPage()
        {
            InitializeComponent();
        }

        private void OnBarcodesDetected(object sender, BarcodeDetectionEventArgs e)
        {
            var value = e.Results?.Length > 0 ? e.Results[0].Value : null;
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

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
