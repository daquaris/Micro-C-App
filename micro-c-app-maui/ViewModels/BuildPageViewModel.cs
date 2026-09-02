using micro_c_app_maui.Views;
using MicroCLib.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using static MicroCLib.Models.BuildComponent;

namespace micro_c_app_maui.ViewModels
{
    // Simplified port of the classic app's ViewModels/BuildPageViewModel.cs for this phase of the
    // MAUI migration: a fixed list of component slots (CPU, Motherboard, ...), each filled by
    // browsing/picking from that category. Not ported yet: compatibility checking between components
    // (the FieldComparisonDependency/etc. system - shows ErrorText/HintText per slot in the classic
    // app), warranty plan upsells, MCOL export/BuildURL, save/load build files, serial tracking,
    // batch scanning, and cross-restart persistence (RestoreState).
    public class BuildPageViewModel : BaseViewModel
    {
        public ObservableCollection<BuildComponent> Components { get; } = new ObservableCollection<BuildComponent>();

        public ICommand ComponentSelectClicked { get; }
        public ICommand Reset { get; }

        public float Subtotal => Components.Sum(c => c.Item?.Price ?? 0f);
        public float TaxedTotal => Subtotal * SettingsPage.TaxRateFactor();

        public BuildPageViewModel()
        {
            Title = "PC Build";

            SetupDefaultComponents();

            ComponentSelectClicked = new Command<BuildComponent>(async (comp) =>
            {
                if (comp == null || Shell.Current == null)
                {
                    return;
                }

                SearchResults results;
                try
                {
                    results = await Search.LoadQuery(null, SettingsPage.StoreID(), comp.CategoryFilter, Search.OrderByMode.match, 1);
                }
                catch
                {
                    await Shell.Current.DisplayAlert("Error", "Search failed - check your connection and try again.", "Ok");
                    return;
                }

                if (results?.Items == null || results.Items.Count == 0)
                {
                    await Shell.Current.DisplayAlert("Error", $"No {comp.Type} products found.", "Ok");
                    return;
                }

                var resultsPage = new SearchResultsPage();
                if (resultsPage.BindingContext is SearchResultsPageViewModel vm)
                {
                    vm.Title = comp.Type.ToString();
                    vm.ParseResults(results);
                    vm.ItemSelected += async (selected) =>
                    {
                        var full = await Item.FromUrl(selected.URL, SettingsPage.StoreID());
                        comp.Item = full;
                        UpdateProperties();
                        await Shell.Current.Navigation.PopAsync();
                    };
                }

                await Shell.Current.Navigation.PushAsync(resultsPage);
            });

            Reset = new Command(async () =>
            {
                if (Shell.Current == null)
                {
                    return;
                }

                var confirmed = await Shell.Current.DisplayAlert("Reset", "Are you sure you want to reset the build?", "Yes", "No");
                if (confirmed)
                {
                    Components.Clear();
                    SetupDefaultComponents();
                    UpdateProperties();
                }
            });
        }

        private void SetupDefaultComponents()
        {
            foreach (var category in SettingsPage.PresetBYO())
            {
                if (MaxNumberPerType(category.Type) > 0)
                {
                    var comp = new BuildComponent { Type = category.Type };
                    comp.PropertyChanged += (sender, args) => UpdateProperties();
                    Components.Add(comp);
                }
            }
        }

        private void UpdateProperties()
        {
            OnPropertyChanged(nameof(Subtotal));
            OnPropertyChanged(nameof(TaxedTotal));
        }
    }
}
