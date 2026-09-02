using micro_c_app_maui.Models;
using micro_c_app_maui.Views;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace micro_c_app_maui.ViewModels
{
    public class RemindersPageViewModel : BaseViewModel
    {
        public ObservableCollection<Reminder> Reminders { get; }
        public ICommand Edit { get; }
        public ICommand Delete { get; }
        public ICommand CheckAll { get; }

        public RemindersPageViewModel()
        {
            Title = "Reminders";

            Reminder.LoadAll();
            Reminders = new ObservableCollection<Reminder>(Reminder.AllReminders!);

            Edit = new Command<Reminder>(async (r) =>
            {
                if (Shell.Current == null)
                {
                    return;
                }
                var vm = new ReminderEditPageViewModel { Reminder = r };
                var page = new ReminderEditPage { BindingContext = vm };
                await Shell.Current.Navigation.PushAsync(page);
            });

            Delete = new Command<Reminder>((r) =>
            {
                Reminders.Remove(r);
                Reminder.AllReminders?.Remove(r);
                Reminder.SaveAll();
            });

            CheckAll = new Command(async () => await DoCheckAll());
        }

        private async System.Threading.Tasks.Task DoCheckAll()
        {
            if (Shell.Current == null)
            {
                return;
            }

            var restocked = 0;
            foreach (var reminder in Reminders.Where(r => !r.Notified).ToList())
            {
                bool inStock;
                try
                {
                    inStock = await reminder.CheckStock();
                }
                catch
                {
                    continue;
                }

                if (inStock)
                {
                    reminder.Notified = true;
                    restocked++;
                }
            }

            if (restocked > 0)
            {
                Reminder.SaveAll();
            }

            await Shell.Current.DisplayAlert("Check Stock", restocked > 0 ? $"{restocked} item(s) are back in stock!" : "No items are back in stock yet.", "Ok");
        }
    }
}
