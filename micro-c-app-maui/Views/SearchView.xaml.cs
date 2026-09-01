using MicroCLib.Models;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace micro_c_app_maui.Views
{
    // Simplified port of the classic app's Views/SearchView.xaml.cs for the phase-1 vertical slice:
    // covers the text/SKU/UPC search + scan path that's actually used day to day. Batch scanning,
    // quick-search category filters, and the enhanced-search toggle are not ported yet.
    public partial class SearchView : ContentView
    {
        public event Action<Item> ProductFound;
        public event Action<string> Error;

        public SearchView()
        {
            InitializeComponent();
        }

        private async void OnSubmitClicked(object sender, EventArgs e)
        {
            await Submit(searchEntry.Text);
        }

        private async void OnScanClicked(object sender, EventArgs e)
        {
            var scanPage = new ScannerPage();
            scanPage.OnScanResult += async (result) =>
            {
                if (SettingsPage.Vibrate())
                {
                    Vibration.Vibrate();
                }
                await Application.Current.MainPage.Navigation.PopModalAsync();
                await Submit(result);
            };
            await Application.Current.MainPage.Navigation.PushModalAsync(scanPage);
        }

        public async Task Submit(string searchValue)
        {
            if (string.IsNullOrWhiteSpace(searchValue))
            {
                return;
            }

            var cached = App.SearchCache?.Get(searchValue);
            if (cached != null)
            {
                ProductFound?.Invoke(cached);
                return;
            }

            Item item = null;
            try
            {
                if (Regex.IsMatch(searchValue, "^\\d{6}$|^\\d{12}$"))
                {
                    item = await Search.LoadFast(searchValue);
                }
                else
                {
                    var results = await Search.LoadQuery(searchValue, SettingsPage.StoreID(), null, Search.OrderByMode.match, 1);
                    item = results?.Items?.Count > 0
                        ? await Item.FromUrl(results.Items[0].URL, SettingsPage.StoreID())
                        : null;
                }
            }
            catch (Exception ex)
            {
                Error?.Invoke($"Search failed: {ex.Message}");
                return;
            }

            if (item == null || string.IsNullOrWhiteSpace(item.SKU) || item.SKU == "000000")
            {
                Error?.Invoke($"Failed to find product with query {searchValue}");
                return;
            }

            App.SearchCache?.Add(item);
            ProductFound?.Invoke(item);
        }
    }
}
