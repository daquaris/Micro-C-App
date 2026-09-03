using Markdig;
using micro_c_app_maui.ViewModels.Reference;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace micro_c_app_maui.Views.Reference
{
    // Simplified port of the classic app's Views/Reference/ReferenceWebViewPage.xaml.cs: renders
    // Markdown to HTML with Markdig (C#-side) instead of the classic app's client-side marked.js in
    // a bundled reference.html, which sidesteps needing to port that whole local-asset WebView setup.
    // The special `[Text](search=X)` / `(reference=/path)` / `(plan=Name)` links still work via
    // WebView.Navigating interception; `(#footer)` signature links are not ported yet.
    public partial class ReferenceWebViewPage : ContentPage
    {
        // UseAdvancedExtensions enables GFM pipe tables, autolinks, task lists, etc. - without it
        // Markdig only does CommonMark, and reference entries here rely on pipe tables.
        private static readonly MarkdownPipeline MarkdownPipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();

        public ReferenceWebViewPage()
        {
            InitializeComponent();

            if (BindingContext is ReferenceWebViewPageViewModel vm)
            {
                vm.PropertyChanged += (_, e) =>
                {
                    if (e.PropertyName == nameof(ReferenceWebViewPageViewModel.Text))
                    {
                        UpdateWebView(vm.Text);
                    }
                };

                if (!string.IsNullOrWhiteSpace(vm.Text))
                {
                    UpdateWebView(vm.Text);
                }
            }

            webView.Navigating += WebView_Navigating;
        }

        private void UpdateWebView(string markdown)
        {
            var body = Markdown.ToHtml(markdown ?? "", MarkdownPipeline);
            var isDark = Application.Current?.RequestedTheme == AppTheme.Dark;
            var bg = isDark ? "#121212" : "#ffffff";
            var fg = isDark ? "#e8e8e8" : "#111111";
            var linkColor = isDark ? "#8ab4f8" : "#0b5ed7";

            var html = $@"<html><head><meta name='viewport' content='width=device-width,initial-scale=1'>
<style>
body {{ background:{bg}; color:{fg}; font-family:sans-serif; padding:12px; }}
img {{ max-width:100%; }}
table {{ width:100%; border-collapse:collapse; margin:8px 0; }}
th, td {{ border:1px solid gray; padding:6px; text-align:left; }}
a {{ color:{linkColor}; }}
blockquote {{ border-left:3px solid gray; margin:8px 0; padding:4px 12px; opacity:0.85; }}
</style></head><body>{body}</body></html>";

            webView.Source = new HtmlWebViewSource { Html = html };
        }

        private async void WebView_Navigating(object? sender, WebNavigatingEventArgs e)
        {
            // Must run before the custom-link regex below: (search|reference|plan)=(.*)$ is
            // unanchored at the start, so a genuine external link that merely *ends* with a matching
            // query string (e.g. https://microcenter.com/category?search=monitors) would false-match
            // and get cancelled/rerouted through internal Shell navigation instead of opening in the
            // browser. The classic app's equivalent regex avoided this by requiring a "file:" scheme
            // prefix (its internal links are file:// URLs); this MAUI port's internal links render as
            // bare "search=X" hrefs with no scheme, so ordering is what keeps the two apart here.
            if (e.Url.StartsWith("http://") || e.Url.StartsWith("https://"))
            {
                e.Cancel = true;
                await Launcher.OpenAsync(e.Url);
                return;
            }

            var match = Regex.Match(e.Url, "(search|reference|plan)=(.*)$");
            if (match.Success)
            {
                e.Cancel = true;
                var command = match.Groups[1].Value.ToLower();
                var argument = Uri.UnescapeDataString(match.Groups[2].Value);

                switch (command)
                {
                    case "search":
                        if (Shell.Current != null)
                        {
                            await Shell.Current.GoToAsync($"//SearchPage?search={argument}");
                        }
                        break;
                    case "reference":
                        await ReferenceIndexPage.NavigateTo(argument);
                        break;
                    case "plan":
                        // The warranty Plans reference tree isn't ported yet.
                        Debug.WriteLine($"Reference plan link not supported yet: {argument}");
                        break;
                }
            }
        }
    }
}
