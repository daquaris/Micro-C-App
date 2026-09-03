using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MicroCLib.Models
{
    public static class Stores
    {
        // Hardcoded fallback - kept current as of 2026-09, but will drift again as MicroCenter opens new
        // stores. Call RefreshFromWeb() to pull the live list; this dictionary is only ever used if that
        // hasn't run yet or fails, so store selection never breaks even when the site markup changes again.
        public static Dictionary<string, string> AllStores = new Dictionary<string, string>()
        {
            {"AZ - Phoenix", "205"},
            {"CA - Tustin", "101"},
            {"CA - Santa Clara", "195"},
            {"CO - Denver", "181"},
            {"FL - Miami", "185"},
            {"GA - Duluth", "065"},
            {"GA - Marietta", "041"},
            {"IL - Chicago", "151"},
            {"IL - Westmont", "025"},
            {"IN - Indianapolis", "165" },
            {"KS - Overland Park", "191"},
            {"MA - Cambridge", "121"},
            {"MD - Rockville", "085"},
            {"MD - Parkville", "125"},
            {"MI - Madison Heights", "055"},
            {"MN - St. Louis Park", "045"},
            {"MO - Brentwood", "095"},
            {"NC - Charlotte", "175"},
            {"NJ - North Jersey", "075"},
            {"NY - Westbury", "171"},
            {"NY - Brooklyn", "115"},
            {"NY - Flushing", "145"},
            {"NY - Yonkers", "105"},
            {"OH - Columbus", "141"},
            {"OH  - Mayfield Heights", "051"},
            {"OH - Sharonville", "071"},
            {"PA - St. Davids", "061"},
            {"TX - Houston", "155"},
            {"TX - Dallas", "131"},
            {"TX - Austin", "215"},
            {"VA - Fairfax", "081"},
            {"Micro Center Web Store", "029"}
        };

        private static readonly Regex StoreDropdownEntry = new Regex(
            "store_\\d{3}\"><a class=\"dropdown-item\" href=\"[^\"]*storeid=(\\d{3})\"><span class=\"storeState\">([^<]*)</span><span class=\"dash\">[^<]*</span><span class=\"storeName\">([^<]*)</span>");

        // Scrapes the store-selector dropdown that appears in the footer of every microcenter.com page.
        // Overwrites AllStores only on success - a failed fetch or a markup change leaves the existing
        // (possibly stale, but non-empty) list in place rather than breaking store selection.
        public static async Task<bool> RefreshFromWeb(CancellationToken? token = null)
        {
            try
            {
                // Was creating and disposing its own HttpClient per call instead of reusing
                // SharedHttpClient.Instance like Item.cs/Search.cs - the exact anti-pattern
                // SharedHttpClient's own comment warns about, and it duplicated just the User-Agent
                // header rather than the full header set every other microcenter.com request sends.
                var body = await SharedHttpClient.Instance.GetStringAsync("https://www.microcenter.com/");
                token?.ThrowIfCancellationRequested();

                var matches = StoreDropdownEntry.Matches(body);
                if (matches.Count == 0)
                {
                    return false;
                }

                var updated = new Dictionary<string, string>();
                foreach (Match m in matches)
                {
                    var state = m.Groups[2].Value.Trim();
                    var city = m.Groups[3].Value.Trim();
                    updated[$"{state} - {city}"] = m.Groups[1].Value;
                }

                // The ship-to-store / web option doesn't appear in the physical-store dropdown.
                updated["Micro Center Web Store"] = "029";

                AllStores = updated;
                return true;
            }
            catch (Exception e)
            {
                // Best-effort refresh - AllStores keeps its previous (fallback or last-successful)
                // value on failure, so this is never fatal. Logged only so a markup change here isn't
                // as invisible as the ParsePlans hang was before someone happened to investigate it.
                System.Diagnostics.Debug.WriteLine($"Stores.RefreshFromWeb failed: {e.Message}");
                return false;
            }
        }
    }
}
