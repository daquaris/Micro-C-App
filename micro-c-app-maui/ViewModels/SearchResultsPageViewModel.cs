using MicroCLib.Models;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace micro_c_app_maui.ViewModels
{
    // Simplified port of the classic app's ViewModels/SearchResultsPageViewModel.cs for the phase-1
    // vertical slice: shows the flat result list from a single query. Sorting, spec-based filtering,
    // and "load more" pagination (enhanced search) are not ported yet.
    public class SearchResultsPageViewModel : BaseViewModel
    {
        private ObservableCollection<Item> items = new ObservableCollection<Item>();
        public ObservableCollection<Item> Items { get => items; set => SetProperty(ref items, value); }

        public string SearchQuery { get; set; }
        public string StoreID { get; set; }

        public ICommand SelectItem { get; }
        public event Action<Item> ItemSelected;

        public SearchResultsPageViewModel()
        {
            Title = "Search Results";
            SelectItem = new Command<Item>((item) => ItemSelected?.Invoke(item));
        }

        public void ParseResults(SearchResults results)
        {
            Items = new ObservableCollection<Item>(results?.Items ?? new System.Collections.Generic.List<Item>());
        }
    }
}
