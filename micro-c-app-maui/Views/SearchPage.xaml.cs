using micro_c_app_maui.Models;
using micro_c_app_maui.ViewModels;
using System.Windows.Input;

namespace micro_c_app_maui.Views
{
    public partial class SearchPage : ContentPage
    {
        public ICommand SearchCategoryCommand { get; }

        public SearchPage()
        {
            SearchCategoryCommand = new Command<ComponentTypeInfo>(async (category) =>
            {
                if (category != null)
                {
                    await searchView.SearchCategory(category.SearchCategory);
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
