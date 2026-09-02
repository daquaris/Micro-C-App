using MicroCLib.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace micro_c_app_maui.Models
{
    // Simplified port of the classic app's Models/Reminder.cs: saved items that can be manually
    // re-checked for stock. Not ported yet: automatic background checking + push notifications
    // (INotificationManager, a per-platform DependencyService this migration hasn't ported) - use
    // the "Check All" button on the Reminders page instead of relying on a background check.
    public class Reminder : NotifyPropertyChangedItem
    {
        public static List<Reminder>? AllReminders { get; set; }

        private string name = "";
        private string sku = "000000";
        private string url = "";
        private string message = "";
        private bool notified = false;
        private string pictureURL = "";

        public string Name { get => name; set => SetProperty(ref name, value); }
        public string SKU { get => sku; set => SetProperty(ref sku, value); }
        public string PictureURL { get => pictureURL; set => SetProperty(ref pictureURL, value); }
        public string URL { get => url; set => SetProperty(ref url, value); }
        public string Message { get => message ?? ""; set => SetProperty(ref message, value); }
        public bool Notified { get => notified; set => SetProperty(ref notified, value); }

        public const string FILENAME = "Reminders.json";
        static string Path => System.IO.Path.Combine(FileSystem.AppDataDirectory, FILENAME);

        public Reminder()
        {
        }

        public Reminder(Item item)
        {
            Name = item.Name;
            SKU = item.SKU;
            URL = item.URL;
            PictureURL = item.PictureUrls?.FirstOrDefault() ?? "";
        }

        public async Task<bool> CheckStock()
        {
            var item = await Item.FromUrl(URL, SettingsPage.StoreID());
            return item?.Stock is not null && item.Stock != "Sold Out" && item.Stock != "0";
        }

        public static void Add(Reminder reminder)
        {
            // LoadAll() is what initializes AllReminders - without this, adding a reminder before
            // ever opening the Reminders tab (e.g. straight from a Search result) silently no-ops.
            LoadAll().Add(reminder);
        }

        public static List<Reminder> LoadAll()
        {
            if (AllReminders != null)
            {
                return AllReminders;
            }

            try
            {
                if (File.Exists(Path))
                {
                    var text = File.ReadAllText(Path);
                    AllReminders = JsonSerializer.Deserialize<List<Reminder>>(text) ?? new List<Reminder>();
                }
                else
                {
                    AllReminders = new List<Reminder>();
                }
            }
            catch
            {
                AllReminders = new List<Reminder>();
            }

            return AllReminders;
        }

        public static void SaveAll()
        {
            if (AllReminders == null)
            {
                return;
            }

            try
            {
                var text = JsonSerializer.Serialize(AllReminders);
                // Write-then-move, same as SavedBuild.Save() - a kill mid-write can't leave a
                // truncated/corrupt Reminders.json.
                var tempPath = Path + ".tmp";
                File.WriteAllText(tempPath, text);
                File.Move(tempPath, Path, overwrite: true);
            }
            catch
            {
                // Best-effort persistence - if this fails there's nowhere useful to surface it from a static method.
            }
        }
    }
}
