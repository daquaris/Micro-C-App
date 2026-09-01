using micro_c_app_maui.ViewModels;

namespace micro_c_app_maui.Views
{
    public partial class SearchPage : ContentPage
    {
        public SearchPage()
        {
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
