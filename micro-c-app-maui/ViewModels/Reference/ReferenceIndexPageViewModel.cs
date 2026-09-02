using micro_c_app_maui.Models.Reference;
using micro_c_app_maui.Views.Reference;
using System.Collections.Generic;
using System.Windows.Input;

namespace micro_c_app_maui.ViewModels.Reference
{
    public class ReferenceIndexPageViewModel : BaseViewModel
    {
        private List<IReferenceItem> nodes = new List<IReferenceItem>();
        public List<IReferenceItem> Nodes { get => nodes; set => SetProperty(ref nodes, value); }
        public ICommand SelectedCommand { get; }

        public ReferenceIndexPageViewModel()
        {
            SelectedCommand = new Command<IReferenceItem>(async (node) => await ReferenceIndexPage.NavigateTo(node));
        }
    }
}
