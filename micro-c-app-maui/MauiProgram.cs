using Microsoft.Extensions.Logging;
using Sentry.Maui;
using ZXing.Net.Maui;

namespace micro_c_app_maui;

public static class MauiProgram
{
	// No DSN configured yet - Sentry's SDK no-ops safely (doesn't throw) when Dsn is empty.
	// Set this to a real project DSN from https://sentry.io to actually start receiving events.
	public const string SentryDsn = "";

	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseBarcodeReader()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
				fonts.AddFont("FontAwesome6Solid.otf", "FontAwesomeSolid");
			});

		if (!string.IsNullOrWhiteSpace(SentryDsn))
		{
			builder.UseSentry(options =>
			{
				options.Dsn = SentryDsn;
			});
		}

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
