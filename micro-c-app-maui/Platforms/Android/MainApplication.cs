using Android.App;
using Android.Runtime;

namespace micro_c_app_maui;

[Application]
public class MainApplication : MauiApplication
{
	public MainApplication(IntPtr handle, JniHandleOwnership ownership)
		: base(handle, ownership)
	{
		// TEMPORARY - hunting the scanner "works once, fails on reopen" bug. A CameraX rebind
		// failure (e.g. a lifecycle-state exception from ProcessCameraProvider.BindToLifecycle)
		// runs inside a native Runnable callback and would otherwise crash the process before
		// anything gets logged. Marking Handled = true keeps the app alive so the log is readable
		// on-screen instead of just seeing a crash/restart. Remove alongside CrashLog once fixed.
		AndroidEnvironment.UnhandledExceptionRaiser += (sender, e) =>
		{
			CrashLog.Write($"AndroidEnvironment unhandled: {e.Exception}");
			e.Handled = true;
		};
	}

	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
