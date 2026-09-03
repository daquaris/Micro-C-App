using micro_c_app_maui.Models.Reference;
using micro_c_app_maui.ViewModels.Reference;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace micro_c_app_maui.Views.Reference
{
    // Simplified port of the classic app's Views/Reference/ReferenceIndexPage.xaml.cs: builds the
    // reference tree from the bundled Markdown files (Assets/Pages/**/*.md, embedded resources) and
    // navigates a folder-like hierarchy. The classic app's huge hardcoded warranty Plans subtree
    // (ReferencePlanData/PlanType) is not ported yet - it's Micro Center-internal warranty lookup
    // data, not general reference content.
    public partial class ReferenceIndexPage : ContentPage
    {
        private static bool initialized = false;
        public static ReferenceTree Tree { get; private set; } = new ReferenceTree("Root");

        public ReferenceIndexPage()
        {
            InitializeComponent();

            if (!initialized)
            {
                initialized = true;
                AddPageItems();
                Tree.SortNodes();
            }

            if (BindingContext is ReferenceIndexPageViewModel vm && vm.Nodes.Count == 0)
            {
                Title = "References";
                vm.Nodes = Tree.Nodes;
            }
        }

        public static async Task NavigateTo(string path)
        {
            // The only current caller (ReferenceWebViewPage's reference= link handler) can't pass
            // null, but this is a public entry point driven by markdown-authored link text - a
            // malformed link, or any future caller, shouldn't NRE on path.Split.
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            var parts = path.Split('/').Skip(1);
            var node = Tree.GetNode(parts);
            await NavigateTo(node);
        }

        public static async Task NavigateTo(IReferenceItem node)
        {
            if (Shell.Current == null)
            {
                return;
            }

            if (node is ReferenceTree tree && tree.Nodes.Count > 0)
            {
                var page = new ReferenceIndexPage { Title = tree.Name };
                if (page.BindingContext is ReferenceIndexPageViewModel vm)
                {
                    vm.Nodes = tree.Nodes;
                }
                await Shell.Current.Navigation.PushAsync(page);
            }
            else if (node is ReferenceEntry entry)
            {
                var page = new ReferenceWebViewPage { Title = node.Name };
                if (page.BindingContext is ReferenceWebViewPageViewModel vm)
                {
                    vm.Text = entry.Data;
                }
                await Shell.Current.Navigation.PushAsync(page);
            }
        }

        private static void AddPageItems()
        {
            var assembly = Assembly.GetExecutingAssembly();
            foreach (var res in assembly.GetManifestResourceNames())
            {
                var match = Regex.Match(res, @"micro_c_app_maui\.Assets\.Pages\.(.*?)\.md");
                if (!match.Success)
                {
                    continue;
                }

                var name = match.Groups[1].Value.Replace('_', ' ');
                using var stream = assembly.GetManifestResourceStream(res);
                if (stream == null)
                {
                    continue;
                }
                using var reader = new StreamReader(stream);
                var text = reader.ReadToEnd();

                var path = name.Split('.');
                var parent = Tree.CreateRoute(path);
                parent.Nodes.Add(new ReferenceEntry
                {
                    Name = path.Last(),
                    Data = text
                });
            }
        }
    }
}
