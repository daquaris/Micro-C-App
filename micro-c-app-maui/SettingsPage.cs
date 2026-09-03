using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;
using micro_c_app_maui.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using static MicroCLib.Models.BuildComponent;
using static MicroCLib.Models.BuildComponent.ComponentType;

namespace micro_c_app_maui
{
    // Static preference helpers only, ported from the classic app's Views/SettingsPage.xaml.cs.
    // The visual settings page itself isn't part of this vertical slice yet.
    public static class SettingsPage
    {
        public const string PREF_SELECTED_STORE = "selected_store";
        public const string PREF_TAX_RATE = "tax_rate";
        public const string PREF_THEME = "theme";
        public const string PREF_VIBRATE = "vibrate";
        public const string PREF_ANALYTICS_ENABLED = "analytics_enabled";
        public const string PREF_QUICKSEARCH_CATEGORIES = "quicksearch_categories";
        public const string PREF_HELPMESSAGE_INDEX = "helpmessage_index";
        public const string PREF_ENHANCED_SEARCH = "enhanced_search";

        public const string SETTINGS_UPDATED_MESSAGE = "updated";

        public static string StoreID() => Preferences.Get(PREF_SELECTED_STORE, "141");
        public static float TaxRate() => Preferences.Get(PREF_TAX_RATE, 7.5f);
        public static float TaxRateFactor() => (TaxRate() * .01f) + 1;
        public static AppTheme Theme() => (AppTheme)Preferences.Get(PREF_THEME, 0);
        public static bool Vibrate() => Preferences.Get(PREF_VIBRATE, true);
        public static bool AnalyticsEnabled() => Preferences.Get(PREF_ANALYTICS_ENABLED, false);
        public static bool UseEnhancedSearch() => Preferences.Get(PREF_ENHANCED_SEARCH, true);
        public static int HelpMessageIndex() => Preferences.Get(PREF_HELPMESSAGE_INDEX, 0);

        public static List<ComponentTypeInfo> QuicksearchCategories()
        {
            var json = Preferences.Get(PREF_QUICKSEARCH_CATEGORIES, null);
            if (json == null)
            {
                return PresetBYO().ToList();
            }

            try
            {
                return JsonConvert.DeserializeObject<List<ComponentTypeInfo>>(json) ?? PresetBYO().ToList();
            }
            catch (JsonException)
            {
                // A corrupted preference value or a future ComponentTypeInfo shape change shouldn't
                // hard-lock the app out of its quick-search categories - fall back to the defaults.
                return PresetBYO().ToList();
            }
        }

        public static void StoreID(string id) { Preferences.Set(PREF_SELECTED_STORE, id); StoreIDChanged?.Invoke(); SendSettingsUpdated(); }
        public static void Theme(AppTheme theme) { Preferences.Set(PREF_THEME, (int)theme); SendSettingsUpdated(); }
        public static void Vibrate(bool vibrate) { Preferences.Set(PREF_VIBRATE, vibrate); SendSettingsUpdated(); }
        public static void AnalyticsEnabled(bool value) { Preferences.Set(PREF_ANALYTICS_ENABLED, value); SendSettingsUpdated(); }
        public static void UseEnhancedSearch(bool value) { Preferences.Set(PREF_ENHANCED_SEARCH, value); SendSettingsUpdated(); }

        public static void QuicksearchCategories(List<ComponentTypeInfo> categories)
        {
            Preferences.Set(PREF_QUICKSEARCH_CATEGORIES, JsonConvert.SerializeObject(categories));
            SendSettingsUpdated();
        }

        public static void HelpMessageIndex(int index) { Preferences.Set(PREF_HELPMESSAGE_INDEX, index); SendSettingsUpdated(); }
        public static void IncrementHelpMessageIndex() => HelpMessageIndex(HelpMessageIndex() + 1);

        // MessagingCenter was removed from MAUI (Xamarin.Forms legacy) - a plain static event covers
        // our one use case (notifying the app that a setting changed) without pulling in a messenger package.
        public static event Action? Updated;

        // Separate from Updated, which fires for every preference (theme, vibrate, analytics opt-in,
        // quicksearch categories, even the help-hint dismiss counter) - App.xaml.cs's analytics
        // handler used to listen on Updated and log a "Store ID" event on every one of those, not
        // just an actual store change.
        public static event Action? StoreIDChanged;

        private static void SendSettingsUpdated()
        {
            Updated?.Invoke();
        }

        public static IEnumerable<ComponentTypeInfo> PresetBYO()
        {
            yield return new ComponentTypeInfo(BuildService, "");
            yield return new ComponentTypeInfo(ComponentType.OperatingSystem, "");
            yield return new ComponentTypeInfo(CPU, "");
            yield return new ComponentTypeInfo(Motherboard, "");
            yield return new ComponentTypeInfo(RAM, "");
            yield return new ComponentTypeInfo(Case, "");
            yield return new ComponentTypeInfo(PowerSupply, "");
            yield return new ComponentTypeInfo(GPU, "");
            yield return new ComponentTypeInfo(SSD, "");
            yield return new ComponentTypeInfo(HDD, "");
            yield return new ComponentTypeInfo(CPUCooler, "");
            yield return new ComponentTypeInfo(WaterCoolingKit, "");
            yield return new ComponentTypeInfo(CaseFan, "");
        }
    }
}
