using MicroCLib.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using static MicroCLib.Models.BuildComponent;

namespace micro_c_app_maui.ViewModels
{
    // Simplified port of the classic app's ViewModels/QuotePageViewModel.cs for this phase of the
    // MAUI migration: add items (via search/scan), adjust quantity, remove, and see subtotal/tax/total.
    // Not ported yet: serial number tracking per item, warranty plan upsells, Send Quote (the
    // dataflare.bbarrett.me sharing link), CSV/TXT export, save/load quote files, and batch scanning.
    // The quote also isn't persisted across app restarts yet (RestoreState wasn't ported).
    // Line item messages (issue #16) are ported here even though the classic app never had them -
    // BuildComponent.Message is new (see BuildComponent.cs).
    public class QuotePageViewModel : BaseViewModel
    {
        private ObservableCollection<BuildComponent> items = new ObservableCollection<BuildComponent>();
        public ObservableCollection<BuildComponent> Items { get => items; set => SetProperty(ref items, value); }

        public ICommand OnProductFound { get; }
        public ICommand OnProductError { get; }
        public ICommand IncreaseQuantity { get; }
        public ICommand DecreaseQuantity { get; }
        public ICommand RemoveItem { get; }
        public ICommand EditMessage { get; }
        public ICommand Reset { get; }

        public float Subtotal => Items.Sum(i => i.Item != null ? i.Item.Price * i.Item.Quantity : 0);
        public float TaxRate => SettingsPage.TaxRate();
        public float TaxedTotal => Subtotal * SettingsPage.TaxRateFactor();
        public int TotalUnits => Items.Sum(i => i.Item != null ? i.Item.Quantity : 0);

        public QuotePageViewModel()
        {
            Title = "Quote";

            Items.CollectionChanged += (sender, args) => UpdateProperties();

            OnProductFound = new Command<Item>((item) =>
            {
                var comp = new BuildComponent { Item = item, Type = item.ComponentType };
                comp.PropertyChanged += (sender, args) => UpdateProperties();
                Items.Add(comp);
            });

            OnProductError = new Command<string>(async (message) =>
            {
                if (Shell.Current != null)
                {
                    await Shell.Current.DisplayAlert("Error", message, "Ok");
                }
            });

            IncreaseQuantity = new Command<BuildComponent>((comp) =>
            {
                if (comp?.Item != null)
                {
                    comp.Item.Quantity++;
                    UpdateProperties();
                }
            });

            DecreaseQuantity = new Command<BuildComponent>((comp) =>
            {
                if (comp?.Item != null && comp.Item.Quantity > 1)
                {
                    comp.Item.Quantity--;
                    UpdateProperties();
                }
            });

            RemoveItem = new Command<BuildComponent>(async (comp) =>
            {
                if (comp?.Item == null || Shell.Current == null)
                {
                    return;
                }

                var confirmed = await Shell.Current.DisplayAlert("Remove", $"Are you sure you want to remove {comp.Item.Name}?", "Yes", "No");
                if (confirmed)
                {
                    Items.Remove(comp);
                }
            });

            EditMessage = new Command<BuildComponent>(async (comp) =>
            {
                if (comp?.Item == null || Shell.Current == null)
                {
                    return;
                }

                var result = await Shell.Current.DisplayPromptAsync("Line Item Message", $"Message for {comp.Item.Name}:", initialValue: comp.Message, maxLength: 200);
                if (result != null)
                {
                    comp.Message = result.Trim();
                }
            });

            Reset = new Command(async () =>
            {
                if (Shell.Current == null)
                {
                    return;
                }

                var confirmed = await Shell.Current.DisplayAlert("Reset", "Clear the entire quote?", "Yes", "No");
                if (confirmed)
                {
                    Items.Clear();
                }
            });
        }

        private void UpdateProperties()
        {
            OnPropertyChanged(nameof(Subtotal));
            OnPropertyChanged(nameof(TaxedTotal));
            OnPropertyChanged(nameof(TotalUnits));
        }
    }
}
