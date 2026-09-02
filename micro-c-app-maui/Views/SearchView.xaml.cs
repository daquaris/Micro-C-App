using micro_c_app_maui.ViewModels;
using MicroCLib.Models;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace micro_c_app_maui.Views
{
    // Simplified port of the classic app's Views/SearchView.xaml.cs for the phase-1 vertical slice:
    // covers the text/SKU/UPC search + scan path that's actually used day to day, plus category
    // browsing (via SearchResultsPage). Batch scanning and the enhanced-search toggle are not
    // ported yet.
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

            // A 6-digit SKU or 12-digit UPC goes straight to the single matching item (this is the
            // scan/fast-lookup path); anything else is a text query that can return many results.
            if (Regex.IsMatch(searchValue, "^\\d{6}$|^\\d{12}$"))
            {
                Item item;
                try
                {
                    item = await Search.LoadFast(searchValue, SettingsPage.StoreID());
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
                return;
            }

            await SearchAndShowResults(searchValue, null);
        }

        // Used by the quick-search category grid to browse a whole category rather than a specific query.
        public async Task SearchCategory(string categoryFilter)
        {
            await SearchAndShowResults(null, categoryFilter);
        }

        private async Task SearchAndShowResults(string searchValue, string categoryFilter)
        {
            SearchResults results;
            try
            {
                results = await Search.LoadQuery(searchValue, SettingsPage.StoreID(), categoryFilter, Search.OrderByMode.match, 1);
            }
            catch (Exception ex)
            {
                Error?.Invoke($"Search failed: {ex.Message}");
                return;
            }

            if (results?.Items == null || results.Items.Count == 0)
            {
                Error?.Invoke($"Failed to find product with query {searchValue}");
                return;
            }

            if (results.Items.Count == 1)
            {
                var item = await Item.FromUrl(results.Items[0].URL, SettingsPage.StoreID());
                App.SearchCache?.Add(item);
                ProductFound?.Invoke(item);
                return;
            }

            var resultsPage = new SearchResultsPage();
            if (resultsPage.BindingContext is SearchResultsPageViewModel vm)
            {
                vm.SearchQuery = searchValue;
                vm.ParseResults(results);
                vm.ItemSelected += async (selected) =>
                {
                    var full = await Item.FromUrl(selected.URL, SettingsPage.StoreID());
                    App.SearchCache?.Add(full);
                    await Application.Current.MainPage.Navigation.PopAsync();
                    ProductFound?.Invoke(full);
                };
            }

            await Application.Current.MainPage.Navigation.PushAsync(resultsPage);
        }
    }
}
