namespace micro_c_app_maui.ViewModels.Reference
{
    public class ReferenceWebViewPageViewModel : BaseViewModel
    {
        private string text = "";
        public string Text { get => text; set => SetProperty(ref text, value); }
    }
}
