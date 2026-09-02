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

        // TEMPORARY diagnostic counter - FrameReady fires on every camera frame regardless of
        // whether ZXing found a barcode in it, unlike BarcodesDetected (which only fires on a hit).
        // That makes it the one signal that can tell "camera frames never reach the analyzer" apart
        // from "frames arrive but nothing ever decodes". Remove alongside debugLabel once fixed.
        private long frameCount;

        // TEMPORARY - polls CrashLog on a timer (rather than only on FrameReady/BarcodesDetected)
        // so the label keeps updating even in the exact failure case we're chasing: camera reopen
        // that silently stops producing frames. Remove alongside CrashLog/debugLabel once fixed.
        private IDispatcherTimer diagnosticTimer;

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

            CrashLog.Write("ScannerPage appearing - opening camera");

            diagnosticTimer = Dispatcher.CreateTimer();
            diagnosticTimer.Interval = TimeSpan.FromSeconds(1);
            diagnosticTimer.Tick += (s, e) => RefreshDiagnosticLabel();
            diagnosticTimer.Start();
        }

        protected override void OnDisappearing()
        {
            CrashLog.Write($"ScannerPage disappearing - frames analyzed this session: {Interlocked.Read(ref frameCount)}");

            scanner.IsDetecting = false;
            scanner.IsTorchOn = false;

            diagnosticTimer?.Stop();
            diagnosticTimer = null;

            base.OnDisappearing();
        }

        private void RefreshDiagnosticLabel()
        {
            var frames = Interlocked.Read(ref frameCount);
            debugLabel.Text = $"frames analyzed: {frames}{Environment.NewLine}{CrashLog.ReadLast(4)}";
        }

        // TEMPORARY diagnostic - fires on every camera frame regardless of detection. If the count
        // never climbs, frames aren't reaching the analyzer at all (camera/binding problem). If it
        // climbs steadily but OnBarcodesDetected never fires, frames are being analyzed but nothing
        // is ever decoding (image-format/decoder problem). The label itself is refreshed by
        // diagnosticTimer, not here, so it keeps updating even if frames stop entirely.
        private void OnFrameReady(object sender, ZXing.Net.Maui.CameraFrameBufferEventArgs e)
            => Interlocked.Increment(ref frameCount);

        private void OnBarcodesDetected(object sender, BarcodeDetectionEventArgs e)
        {
            var count = e.Results?.Length ?? 0;

            if (count > 0)
            {
                CrashLog.Write($"BarcodesDetected: {e.Results[0].Format} -> {e.Results[0].Value}");
            }

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
