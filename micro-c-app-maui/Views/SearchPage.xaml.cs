using micro_c_app_maui.Models;
using micro_c_app_maui.ViewModels;
using System.Windows.Input;

namespace micro_c_app_maui.Views
{
    public partial class SearchPage : ContentPage
    {
        public ICommand SearchCategoryCommand { get; }
        public ICommand AddReminderCommand { get; }

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
    }
}
