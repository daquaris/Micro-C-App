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

        // Guards both entry points below against rapid double-taps - without it, tapping Search
        // twice fires concurrent lookups, and tapping Scan twice pushes a second scanner page on top
        // of the first. Spans the *whole* async flow, not just the synchronous part of each handler:
        // for Scan specifically, it's only cleared once a scanned result finishes its own Submit(), or
        // (via ScannerPage.Disappearing) once the user backs out without scanning anything - clearing
        // it right after PushModalAsync returns would only debounce the push itself, not the scan.
        private bool isBusy;

        public SearchView()
        {
            InitializeComponent();
        }

        private async void OnSubmitClicked(object sender, EventArgs e)
        {
            if (isBusy)
            {
                return;
            }

            isBusy = true;
            try
            {
                await Submit(searchEntry.Text);
            }
            finally
            {
                isBusy = false;
            }
        }

        private async void OnScanClicked(object sender, EventArgs e)
        {
            if (isBusy)
            {
                return;
            }

            isBusy = true;

            var scanPage = new ScannerPage();
            var gotResult = false;

            // A named local function (rather than an anonymous lambda) so it can unsubscribe itself
            // once used.
            async void HandleScan(string result)
            {
                gotResult = true;
                scanPage.OnScanResult -= HandleScan;

                if (SettingsPage.Vibrate())
                {
                    Vibration.Vibrate();
                }

                await (Application.Current?.MainPage?.Navigation?.PopModalAsync() ?? Task.CompletedTask);
                try
                {
                    await Submit(result);
                }
                finally
                {
                    isBusy = false;
                }
            }

            scanPage.OnScanResult += HandleScan;
            // Disappearing fires for every dismissal path. HandleScan already clears isBusy for the
            // scan-and-submit path (after Submit finishes); this only needs to cover backing out of
            // the scanner (e.g. the hardware back button) without ever scanning anything.
            scanPage.Disappearing += (s, args) =>
            {
                if (!gotResult)
                {
                    isBusy = false;
                }
            };

            await (Application.Current?.MainPage?.Navigation?.PushModalAsync(scanPage) ?? Task.CompletedTask);
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
            if (isBusy)
            {
                return;
            }

            isBusy = true;
            try
            {
                await SearchAndShowResults(null, categoryFilter);
            }
            finally
            {
                isBusy = false;
            }
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
                Item item;
                try
                {
                    item = await Item.FromUrl(results.Items[0].URL, SettingsPage.StoreID());
                }
                catch (Exception ex)
                {
                    Error?.Invoke($"Search failed: {ex.Message}");
                    return;
                }

                // Item.FromUrl doesn't throw on a non-200 response - it returns a NotFound placeholder
                // (see Item.NotFound), so a delisted product or a transient 5xx was silently surfacing
                // as a "found" result reading "Product not found" instead of the error path below.
                if (item.NotFound)
                {
                    Error?.Invoke($"Failed to find product with query {searchValue}");
                    return;
                }

                App.SearchCache?.Add(item);
                ProductFound?.Invoke(item);
                return;
            }

            var resultsPage = new SearchResultsPage();
            if (resultsPage.BindingContext is SearchResultsPageViewModel vm)
            {
                vm.SearchQuery = searchValue;
                vm.ParseResults(results);
                // The results list has no debounce (SelectionMode="None" + a per-row
                // TapGestureRecognizer firing on every tap), so a fast double-tap fires ItemSelected
                // twice - without this guard, two concurrent fetches would each race to pop the page,
                // and the second PopAsync() (on a page the first already popped) throws.
                var itemSelectionInFlight = false;
                vm.ItemSelected += async (selected) =>
                {
                    if (itemSelectionInFlight)
                    {
                        return;
                    }
                    itemSelectionInFlight = true;
                    try
                    {
                        Item full;
                        try
                        {
                            full = await Item.FromUrl(selected.URL, SettingsPage.StoreID());
                        }
                        catch (Exception ex)
                        {
                            Error?.Invoke($"Search failed: {ex.Message}");
                            return;
                        }

                        // See the single-result NotFound check above - the same placeholder trap applies
                        // to picking an item off the results list.
                        if (full.NotFound)
                        {
                            Error?.Invoke($"Failed to load {selected.Name}");
                            return;
                        }

                        App.SearchCache?.Add(full);
                        await (Application.Current?.MainPage?.Navigation?.PopAsync() ?? Task.CompletedTask);
                        ProductFound?.Invoke(full);
                    }
                    finally
                    {
                        itemSelectionInFlight = false;
                    }
                };
            }

            await (Application.Current?.MainPage?.Navigation?.PushAsync(resultsPage) ?? Task.CompletedTask);
        }
    }
}
