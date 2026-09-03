using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace MicroCLib.Models
{
    public class CategoryInfo
    {
        public string Name { get; set; }
        [JsonProperty(PropertyName = "item")]
        public string Url { get; set; }
        private string filter;
        // Distinct from "filter is null/empty" - without this, a Url that's null or just doesn't
        // match FilterRegex left `filter` unset forever, so every single Filter access (this is read
        // per-category in a loop - see Item.GetPrimaryType) re-ran the regex from scratch instead of
        // caching the "no match" result once.
        private bool filterComputed;
        public string Filter
        {
            get
            {
                if (!filterComputed)
                {
                    filterComputed = true;
                    if (!string.IsNullOrWhiteSpace(Url))
                    {
                        var match = Regex.Match(Url, FilterRegex);
                        filter = match.Success ? match.Groups[1].Value : null;
                    }
                }

                return filter;
            }
        }
        public const string FilterRegex = "category\\/(.*?)\\/";
    }
}
