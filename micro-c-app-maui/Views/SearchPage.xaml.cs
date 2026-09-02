using micro_c_app_maui.Models;
using micro_c_app_maui.ViewModels;
using System.Windows.Input;

namespace micro_c_app_maui.Views
{
    public partial class SearchPage : ContentPage, IQueryAttributable
    {
        public ICommand SearchCategoryCommand { get; }
        public ICommand AddReminderCommand { get; }

        // Shell re-applies query attributes to this cached page instance on every "//SearchPage"
        // navigation, including a plain tab switch back to Search with no new link tapped - without
        // tracking what was last consumed, that would silently re-run the reference-link search and
        // clobber whatever item the user was actually looking at.
        private string? lastAppliedSearch;

        public SearchPage()
        {
            SearchCategoryCommand = new Command<ComponentTypeInfo>(async (category) =>
            {
                if (category != null)
                {
                    await searchView.SearchCategory(category.SearchCategory);
                }
            });

            AddReminderCommand = new Command(async () =>
            {
                if (BindingContext is SearchViewModel vm && vm.Item != null && Shell.Current != null)
                {
                    var reminderVm = new ReminderEditPageViewModel
                    {
                        Reminder = new Reminder(vm.Item),
                        NewItem = true
                    };
                    var page = new ReminderEditPage { BindingContext = reminderVm };
                    await Shell.Current.Navigation.PushAsync(page);
                }
            });

            InitializeComponent();

            searchView.ProductFound += (item) =>
            {
                if (BindingContext is SearchViewModel vm)
                {
                    vm.OnProductFound.Execute(item);
                }
            };

            searchView.Error += (message) =>
            {
                if (BindingContext is SearchViewModel vm)
                {
                    vm.OnProductError.Execute(message);
                }
            };
        }

        // Reference pages link to specific SKUs via `[Text](search=X)`, which
        // ReferenceWebViewPage.WebView_Navigating turns into a "//SearchPage?search=X" Shell
        // navigation. Without this, that query parameter was never read - the link just switched to
        // the Search tab without actually searching for anything.
        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("search", out var value) && value is string search
                && !string.IsNullOrWhiteSpace(search) && search != lastAppliedSearch)
            {
                lastAppliedSearch = search;
                _ = searchView.Submit(search);
            }
        }
    }
}
