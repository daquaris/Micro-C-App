using Microsoft.Maui.ApplicationModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace micro_c_app_maui.ViewModels
{
    // Simplified port of the classic app's ViewModels/SettingsPageViewModel.cs for this phase of the
    // MAUI migration: store selection, vibration, analytics opt-in, and theme. Not ported yet: sales
    // ID/tax rate editing, CSV-with-quote toggle, and quick-search category list management (add/
    // remove/reorder/presets).
    //
    // This page is a TabBar tab rather than a pushed page (the classic app opened it from a menu),
    // so Save persists in place and Cancel just discards unsaved edits - there's no page to pop back to.
    public class SettingsPageViewModel : BaseViewModel
    {
        public static Dictionary<string, string> Stores { get; private set; }
        public List<string> StoreNames { get; set; }
        public string SelectedStoreName { get => selectedStoreName; set => SetProperty(ref selectedStoreName, value); }

        public List<AppTheme> ThemeOptions { get; } = Enum.GetValues(typeof(AppTheme)).Cast<AppTheme>().ToList();
        public AppTheme Theme { get => theme; set => SetProperty(ref theme, value); }

        public bool Vibrate { get => vibrate; set => SetProperty(ref vibrate, value); }
        public bool AnalyticsEnabled { get => analyticsEnabled; set => SetProperty(ref analyticsEnabled, value); }

        public ICommand Save { get; }
        public ICommand Cancel { get; }

        private string selectedStoreName;
        private AppTheme theme;
        private bool vibrate;
        private bool analyticsEnabled;

        public SettingsPageViewModel()
        {
            Title = "Settings";

            Stores = MicroCLib.Models.Stores.AllStores;
            StoreNames = Stores.Keys.ToList();

            Save = new Command(DoSave);
            Cancel = new Command(Reload);

            Reload();
        }

        private void Reload()
        {
            var storeId = SettingsPage.StoreID();
            SelectedStoreName = Stores.FirstOrDefault(kvp => kvp.Value == storeId).Key ?? StoreNames.FirstOrDefault();

            Theme = SettingsPage.Theme();
            Vibrate = SettingsPage.Vibrate();
            AnalyticsEnabled = SettingsPage.AnalyticsEnabled();
        }

        private void DoSave()
        {
            if (SelectedStoreName != null && Stores.TryGetValue(SelectedStoreName, out var storeId))
            {
                SettingsPage.StoreID(storeId);
            }

            if (Application.Current != null)
            {
                Application.Current.UserAppTheme = Theme;
            }

            SettingsPage.Theme(Theme);
            SettingsPage.Vibrate(Vibrate);
            SettingsPage.AnalyticsEnabled(AnalyticsEnabled);
        }
    }
}
