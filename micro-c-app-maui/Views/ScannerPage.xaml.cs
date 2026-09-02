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

        // CameraBarcodeReaderView never sets a continuous autofocus mode on Android (confirmed by
        // reading ZXing.Net.Maui's CameraManager.android.cs - Preview/ImageAnalysis are built with
        // only a resolution selector, no AF mode, and nothing in this page ever called AutoFocus()).
        // A single focus-metering scan locks the lens and disables continuous AF for ~5s, so most
        // frames are simply out of focus - the screenshot that surfaced this showed a visibly blurry
        // barcode despite a clean preview. Re-triggering AutoFocus() periodically keeps the lens
        // hunting instead of sitting locked wherever it happened to land.
        private IDispatcherTimer autoFocusTimer;

        public ScannerPage()
        {
            InitializeComponent();

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

            // AutoFocus() is a no-op until the camera is actually bound (CanFocus() checks for a
            // live camera + sized preview), so starting immediately is harmless - it just won't do
            // anything on the first tick or two.
            autoFocusTimer = Dispatcher.CreateTimer();
            autoFocusTimer.Interval = TimeSpan.FromSeconds(2);
            autoFocusTimer.Tick += (s, e) => scanner.AutoFocus();
            autoFocusTimer.Start();
        }

        protected override void OnDisappearing()
        {
            autoFocusTimer?.Stop();
            autoFocusTimer = null;

            scanner.IsDetecting = false;
            scanner.IsTorchOn = false;
            base.OnDisappearing();
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
