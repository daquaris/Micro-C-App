namespace micro_c_app_maui.Views
{
    public partial class RemindersPage : ContentPage
    {
        public RemindersPage()
        {
            InitializeComponent();
        }

        // See RemindersPageViewModel.Refresh - reminders can be added from the Search tab while
        // this (session-cached) page already exists.
        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (BindingContext is ViewModels.RemindersPageViewModel vm)
            {
                vm.Refresh();
            }
        }
    }
}
