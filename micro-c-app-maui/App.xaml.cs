using MicroCLib.Models;
using System;

namespace micro_c_app_maui;

public partial class App : Application
{
	public static SearchCache SearchCache = new SearchCache(TimeSpan.FromHours(1));

	public App()
	{
		InitializeComponent();

		// StoreIDChanged, not the generic Updated - Updated fires for every preference change
		// (theme, vibrate, analytics opt-in, quicksearch categories, the hint-dismiss counter), so
		// tracking on it logged a "Store ID" event for all of those, not just an actual store change.
		SettingsPage.StoreIDChanged += SettingsUpdated;
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		var window = new Window(new AppShell());

		window.Created += (_, _) =>
		{
			AnalyticsService.Track("Store ID", SettingsPage.StoreID());

			// Best-effort refresh of the store list from microcenter.com; Stores.AllStores keeps its
			// hardcoded fallback values if this fails, so a bad network or a markup change can't break
			// store selection.
			_ = Stores.RefreshFromWeb();
		};

		return window;
	}

	private void SettingsUpdated()
	{
		AnalyticsService.Track("Store ID", SettingsPage.StoreID());
	}
}