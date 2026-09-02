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

            Save = new Command(async () =>
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
            });

            Cancel = new Command(async () =>
            {
                if (Shell.Current != null)
                {
                    await Shell.Current.Navigation.PopAsync();
                }
            });
        }
    }
}
