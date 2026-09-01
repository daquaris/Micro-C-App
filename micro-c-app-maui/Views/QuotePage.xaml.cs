using micro_c_app_maui.ViewModels;

namespace micro_c_app_maui.Views
{
    public partial class QuotePage : ContentPage
    {
        public QuotePage()
        {
            InitializeComponent();

            searchView.ProductFound += (item) =>
            {
                if (BindingContext is QuotePageViewModel vm)
                {
                    vm.OnProductFound.Execute(item);
                }
            };

            searchView.Error += (message) =>
            {
                if (BindingContext is QuotePageViewModel vm)
                {
                    vm.OnProductError.Execute(message);
                }
            };
        }
    }
}
