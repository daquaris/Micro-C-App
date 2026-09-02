using micro_c_app_maui.Models;
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
    // app), warranty plan upsells, MCOL export/BuildURL, serial tracking, batch scanning, and
    // cross-restart persistence (RestoreState). Save/Load (issue #19) is ported, but as a local
    // named-slot file store (Models/SavedBuild.cs) rather than the classic app's
    // dataflare.bbarrett.me share-link flow.
    public class BuildPageViewModel : BaseViewModel
    {
        public ObservableCollection<BuildComponent> Components { get; } = new ObservableCollection<BuildComponent>();

        private string? buildName;
        public string? BuildName { get => buildName; set { SetProperty(ref buildName, value); UpdateTitle(); } }

        public ICommand ComponentSelectClicked { get; }
        public ICommand Reset { get; }
        public ICommand Save { get; }
        public ICommand Load { get; }

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
                    BuildName = null;
                    Components.Clear();
                    SetupDefaultComponents();
                    UpdateProperties();
                }
            });

            Save = new Command(async () => await DoSave());
            Load = new Command(async () => await DoLoad());
        }

        private async System.Threading.Tasks.Task DoSave()
        {
            if (Shell.Current == null)
            {
                return;
            }

            var filled = Components.Where(c => c.Item != null).ToList();
            if (filled.Count == 0)
            {
                await Shell.Current.DisplayAlert("Save Build", "Add at least one component before saving.", "Ok");
                return;
            }

            var name = await Shell.Current.DisplayPromptAsync("Save Build", "Name this build:", initialValue: BuildName ?? "");
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }

            if (SavedBuild.Exists(name) && !SavedBuild.IsSameSave(name, BuildName))
            {
                var overwrite = await Shell.Current.DisplayAlert("Save Build", $"A build named \"{name}\" already exists. Overwrite it?", "Yes", "No");
                if (!overwrite)
                {
                    return;
                }
            }

            SavedBuild.Save(name, filled);
            BuildName = name;
            await Shell.Current.DisplayAlert("Save Build", $"Saved as \"{name}\".", "Ok");
        }

        private async System.Threading.Tasks.Task DoLoad()
        {
            if (Shell.Current == null)
            {
                return;
            }

            var names = SavedBuild.ListSavedNames();
            if (names.Count == 0)
            {
                await Shell.Current.DisplayAlert("Load Build", "No saved builds found.", "Ok");
                return;
            }

            var choice = await Shell.Current.DisplayActionSheet("Load Build", "Cancel", null, names.ToArray());
            if (string.IsNullOrEmpty(choice) || choice == "Cancel")
            {
                return;
            }

            var action = await Shell.Current.DisplayActionSheet(choice, "Cancel", "Delete", "Load");
            if (action == "Delete")
            {
                var confirmed = await Shell.Current.DisplayAlert("Delete Build", $"Delete saved build \"{choice}\"?", "Yes", "No");
                if (confirmed)
                {
                    SavedBuild.Delete(choice);
                    if (BuildName == choice)
                    {
                        BuildName = null;
                    }
                }
                return;
            }

            if (action != "Load")
            {
                return;
            }

            var saved = SavedBuild.Load(choice);
            if (saved == null)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to load \"{choice}\".", "Ok");
                return;
            }

            Components.Clear();
            SetupDefaultComponents();
            foreach (var comp in saved.Components)
            {
                comp.PropertyChanged += (sender, args) => UpdateProperties();
                ReplaceOrAdd(comp);
            }
            BuildName = saved.Name;
            UpdateProperties();
        }

        private void ReplaceOrAdd(BuildComponent component)
        {
            if (component.Item == null)
            {
                return;
            }

            var existing = Components.FirstOrDefault(c => c.Item == null && (c.Type == component.Type || component.Item.ComponentType == c.Type));
            if (existing != null)
            {
                existing.Item = component.Item;
            }
            else
            {
                var index = Components.ToList().FindLastIndex(c => c.Type == component.Type);
                if (index >= 0)
                {
                    Components.Insert(index, component);
                }
                else
                {
                    Components.Add(component);
                }
            }
        }

        private void UpdateTitle()
        {
            Title = string.IsNullOrWhiteSpace(BuildName) ? "PC Build" : $"PC Build - {BuildName}";
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
