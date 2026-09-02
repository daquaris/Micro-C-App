using System;
using System.Collections.Generic;
using System.Linq;
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

            // UPC/EAN are 1D formats and decode reliably only once the camera actually settles
            // focus - TryHarder plus a mid-resolution preview (rather than whatever oddball
            // default the device picks) makes that consistent across devices.
            scanner.Options = new BarcodeReaderOptions
            {
                Formats = BarcodeFormats.All,
                AutoRotate = true,
                TryHarder = true,
                CameraResolutionSelector = SelectCameraResolution
            };
        }

        private static CameraResolution SelectCameraResolution(IReadOnlyList<CameraResolution> availableResolutions)
        {
            if (availableResolutions.Count == 0)
            {
                return new CameraResolution(1280, 720);
            }

            return availableResolutions
                .OrderBy(resolution => Math.Abs((resolution.Width * resolution.Height) - (1280 * 720)))
                .ThenBy(resolution => Math.Abs(resolution.Width - 1280) + Math.Abs(resolution.Height - 720))
                .First();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            scanner.IsDetecting = true;
        }

        protected override void OnDisappearing()
        {
            scanner.IsDetecting = false;
            scanner.IsTorchOn = false;
            base.OnDisappearing();
        }

        private void OnBarcodesDetected(object sender, BarcodeDetectionEventArgs e)
        {
            var count = e.Results?.Length ?? 0;

            // TEMPORARY diagnostic - proves whether ZXing is decoding frames at all vs. failing
            // somewhere after. Remove this dispatch + the XAML label once scanning is confirmed
            // working on-device.
            MainThread.BeginInvokeOnMainThread(() =>
            {
                debugLabel.Text = count > 0
                    ? $"detected: {e.Results[0].Format} -> {e.Results[0].Value}"
                    : $"frame analyzed, no barcode ({DateTime.Now:T})";
            });

            if (hasDetected)
            {
                return;
            }

            var value = count > 0 ? e.Results[0].Value : null;
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
