using micro_c_app_maui.Models;
using System.Windows.Input;

namespace micro_c_app_maui.ViewModels
{
    public class ReminderEditPageViewModel : BaseViewModel
    {
        private Reminder reminder = new Reminder();
        private string message = "";

        public Reminder Reminder { get => reminder; set { SetProperty(ref reminder, value); Message = reminder.Message; } }
        public string Message { get => message; set => SetProperty(ref message, value); }
        public bool NewItem { get; set; }

        public ICommand Save { get; }
        public ICommand Cancel { get; }

        public ReminderEditPageViewModel()
        {
            Title = "Edit Reminder";

            // Save and Cancel are plain Buttons with no built-in debounce - a fast double-tap (on
            // either one, or one of each) fired a second PopAsync() on a page the first tap had
            // already popped. IsBusy (BaseViewModel's re-entrancy flag) guards both commands together
            // so only one navigation can be in flight at a time.
            Save = new Command(async () =>
            {
                if (IsBusy)
                {
                    return;
                }
                IsBusy = true;
                try
                {
                    Reminder.Message = Message;
                    if (NewItem)
                    {
                        Reminder.Add(Reminder);
                    }
                    Reminder.SaveAll();
                    if (Shell.Current != null)
                    {
                        await Shell.Current.Navigation.PopAsync();
                    }
                }
                finally
                {
                    IsBusy = false;
                }
            });

            Cancel = new Command(async () =>
            {
                if (IsBusy)
                {
                    return;
                }
                IsBusy = true;
                try
                {
                    if (Shell.Current != null)
                    {
                        await Shell.Current.Navigation.PopAsync();
                    }
                }
                finally
                {
                    IsBusy = false;
                }
            });
        }
    }
}
