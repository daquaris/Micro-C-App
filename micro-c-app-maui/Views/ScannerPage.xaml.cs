using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

        // TEMPORARY diagnostic counters - FrameReady fires on every camera frame regardless of
        // whether ZXing found a barcode in it, unlike BarcodesDetected (which only fires on a hit).
        // That makes it the one signal that can tell "camera frames never reach the analyzer" apart
        // from "frames arrive but nothing ever decodes". Remove alongside debugLabel once fixed.
        private long frameCount;
        private DateTime lastLabelUpdate = DateTime.MinValue;

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

            scanner.FrameReady += OnFrameReady;
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

        // TEMPORARY diagnostic - fires on every camera frame regardless of detection. If the count
        // never climbs, frames aren't reaching the analyzer at all (camera/binding problem). If it
        // climbs steadily but OnBarcodesDetected never fires, frames are being analyzed but nothing
        // is ever decoding (image-format/decoder problem). Remove alongside debugLabel once fixed.
        private void OnFrameReady(object sender, ZXing.Net.Maui.CameraFrameBufferEventArgs e)
        {
            var count = Interlocked.Increment(ref frameCount);

            var now = DateTime.Now;
            if ((now - lastLabelUpdate).TotalMilliseconds < 300)
            {
                return;
            }
            lastLabelUpdate = now;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                debugLabel.Text = $"frames analyzed: {count} (last {now:T})";
            });
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
                    ? $"DETECTED: {e.Results[0].Format} -> {e.Results[0].Value}"
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
