using micro_c_app_maui.Models;
using MicroCLib.Models;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using static MicroCLib.Models.BuildComponent;

namespace micro_c_app_maui.ViewModels
{
    // Ported from the classic app's ViewModels/SearchViewModel.cs, trimmed for the phase-1 vertical
    // slice: Reminders integration, the location-tracker OnProductLocationFound path, quick-search
    // category browsing (needs SearchResultsPage, not ported yet), and the long-press action sheet
    // are all out of scope until those features get ported too.
    public class SearchViewModel : BaseViewModel
    {
        private Item item;
        private Stack<Item> itemQueue;
        private List<ComponentTypeInfo> categories;
        private string hintText;
        private bool hintVisible;
        private bool fastSearch;

        public Stack<Item> ItemQueue { get => itemQueue; set => SetProperty(ref itemQueue, value); }
        public ICommand PopItem { get; }
        public ICommand PopAll { get; }
        public ICommand OnProductFound { get; }
        public ICommand OnProductFastFound { get; }
        public ICommand OnProductError { get; }
        public INavigation Navigation { get; internal set; }
        public Item Item { get => item; set { SetProperty(ref item, value); SetupHint(); } }
        public bool FastSearch { get => fastSearch; set => SetProperty(ref fastSearch, value); }

        public ICommand GoToWebpage { get; }
        public ICommand DismissHint { get; }
        public string HintText { get => hintText; set => SetProperty(ref hintText, value); }
        public bool HintVisible { get => hintVisible; set => SetProperty(ref hintVisible, value); }

        public List<ComponentTypeInfo> Categories { get => categories; set => SetProperty(ref categories, value); }

        public SearchViewModel()
        {
            Title = "Search";
            ItemQueue = new Stack<Item>();

            SetupHint();

            Categories = SettingsPage.QuicksearchCategories();

            OnProductFound = new Command<Item>((Item item) =>
            {
                if (Item != null && !FastSearch)
                {
                    ItemQueue.Push(Item);
                }
                FastSearch = false;

                Item = item;
            });

            OnProductFastFound = new Command<Item>((Item item) =>
            {
                FastSearch = true;
                if (Item != null)
                {
                    ItemQueue.Push(Item);
                }

                Item = item;
            });

            OnProductError = new Command<string>(async (string message) =>
            {
                if (Shell.Current != null)
                {
                    await Shell.Current.DisplayAlert("Error", message, "Ok");
                }
            });

            GoToWebpage = new Command(async () =>
            {
                if (Item != null)
                {
                    await Launcher.OpenAsync($"https://microcenter.com{Item.URL}");
                }
            });

            PopItem = new Command(() =>
            {
                DoPopItem();
            });

            PopAll = new Command(() =>
            {
                DoPopAll();
            });

            DismissHint = new Command(() =>
            {
                HintVisible = false;
                SettingsPage.IncrementHelpMessageIndex();
            });

            SettingsPage.Updated += UpdateProperties;
        }

        private void SetupHint()
        {
            HintText = HelpMessages.GetNextMessage() ?? "";
            HintVisible = !string.IsNullOrWhiteSpace(HintText) && Item == null;
        }

        private void UpdateProperties()
        {
            Categories = SettingsPage.QuicksearchCategories();
        }

        public bool DoPopItem()
        {
            if (ItemQueue.Count > 0)
            {
                Item = ItemQueue.Pop();
                return true;
            }
            Item = null;
            return false;
        }

        public void DoPopAll()
        {
            ItemQueue.Clear();
            Item = null;
        }
    }
}
