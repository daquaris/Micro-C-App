using MicroCLib.Models;
using System;

namespace micro_c_app_maui;

public partial class App : Application
{
	public static SearchCache SearchCache = new SearchCache(TimeSpan.FromHours(1));

	public App()
	{
		InitializeComponent();

		SettingsPage.Updated += SettingsUpdated;
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