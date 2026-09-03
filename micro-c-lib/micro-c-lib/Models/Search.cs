using micro_c_lib.Models;
using MicroCLib.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MicroCLib.Models
{
    public static class Search
    {
        public const int RESULTS_PER_PAGE = 96;
        public enum OrderByMode
        {
            match,
            rating,
            numreviews,
            pricelow,
            pricehigh
        }

        // Same defensive bound as Item.cs's RegexTimeout: a body-wide pattern that stops matching
        // MicroCenter's markup can still backtrack across the whole page rather than failing fast.
        // ParseBody hasn't been observed hanging like Item.ParsePlans did, but the search results
        // page is the same kind of ~500KB+ HTML this failure mode needs to happen on.
        private static readonly TimeSpan RegexTimeout = TimeSpan.FromSeconds(2);

        
        public static string GetSearchUrl(string query, string storeId, string categoryFilter, OrderByMode orderBy, int resultsPerPage, int page)
        {
            return $"https://www.microcenter.com/search/search_results.aspx?Ntt={query}&storeid={storeId}&myStore=false&Ntk=all&N={categoryFilter}&sortby={orderBy}&rpp={resultsPerPage}&page={page}";
        }

        public static async Task<SearchResults> LoadAll(string searchQuery, string storeID, string categoryFilter, OrderByMode orderBy, CancellationToken? token = null)
        {
            int page = 1;
            var result = new SearchResults() { TotalResults = 1 };

            while(result.Items.Count < result.TotalResults)
            {
                // Was returning a plain empty SearchResults on cancellation - indistinguishable from a
                // genuine zero-match query, unlike the ThrowIfCancellationRequested() at the bottom of
                // this same method, which callers can tell apart from "no results" via
                // OperationCanceledException. Throw here too instead of swallowing the cancellation.
                token?.ThrowIfCancellationRequested();

                var addResult = await LoadQuery(searchQuery, storeID, categoryFilter, orderBy, page, token);
                if(addResult != null && addResult.Items.Count == 0)
                {
                    result.TotalResults = result.Items.Count;
                    break;
                }
                result.Items.AddRange(addResult.Items);
                // A parsed TotalResults lower than what's actually been collected is never
                // trustworthy (e.g. ParseBody's "of <tag>N</tag>" summary regex failing to match a
                // markup change) - force at least one more page rather than silently truncating
                // pagination. The "page came back empty" check above is still what actually stops
                // the loop once results are genuinely exhausted.
                result.TotalResults = System.Math.Max(addResult.TotalResults, result.Items.Count + 1);
                page++;
            }

            token?.ThrowIfCancellationRequested();
            return result;

        }

        public static async Task<SearchResults> LoadQuery(string searchQuery, string storeID, string categoryFilter, OrderByMode orderBy, int page, CancellationToken? token = null, IProgress<ProgressInfo> progress = null)
        {
            var client = SharedHttpClient.Instance;

            progress?.Report(new ProgressInfo($"Loading query {searchQuery}", .3));

            var url = GetSearchUrl(searchQuery, storeID, categoryFilter, orderBy, RESULTS_PER_PAGE, page);
            var response = await (token != null ? client.GetAsync(url, token.Value) :  client.GetAsync(url));
            token?.ThrowIfCancellationRequested();
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                progress?.Report(new ProgressInfo($"Parsing query {searchQuery}", .5));

                var body = await response.Content.ReadAsStringAsync();

                var result = await ParseBody(body, token);
                result.Page = page;
                token?.ThrowIfCancellationRequested();
                return result;
            }

            return new SearchResults();
        }

        public static async Task<SearchResults> LoadEnhanced(string searchQuery, string storeID, string categoryFilter, CancellationToken? token = null)
        {
            return await LoadAll(searchQuery, storeID, categoryFilter, OrderByMode.match, token);
        }

        public static async Task<Item> LoadFast(string search, string storeId, CancellationToken? token = null)
        {
            // MicroCenter's search endpoint is 1-indexed - page=0 silently returns zero results for every
            // query (SKU, UPC, or text), which made every fast lookup fail regardless of parsing.
            var res = await LoadQuery(search, storeId, null, OrderByMode.match, 1, token);
            if (res == null || res.TotalResults == 0 || res.Items.Count == 0)
            {
                return null;
            }

            var item = await Item.FromUrl(res.Items[0].URL, storeId, token);

            // A query with zero *real* matches still renders MicroCenter's "you might like" filler
            // products using the same markup as a genuine result, and (like a real single-match hit)
            // that page skips the "Showing X of Y" summary ParseBody otherwise relies on - so
            // TotalResults alone can't tell a real SKU/UPC match from filler. Confirmed by scanning a
            // barcode MicroCenter doesn't carry at all (a grocery item) and getting back an unrelated
            // in-catalog product. Only trust this result if it actually matches what was searched.
            //
            // Item.FromUrl never actually returns null - a non-200 response comes back as a
            // NotFound placeholder Item whose SKU is "000000" (see Item.NotFound). Without this check,
            // searching the literal string "000000" would pass the SKU comparison below against that
            // placeholder and hand back a fake "found" result for a product page that failed to load.
            if (item == null || item.NotFound)
            {
                return null;
            }

            if (Regex.IsMatch(search, "^\\d{6}$") && item.SKU == search)
            {
                return item;
            }

            if (Regex.IsMatch(search, "^\\d{12}$")
                && item.Specs != null
                && item.Specs.TryGetValue("UPC", out var upc)
                && Regex.Replace(upc ?? "", "\\D", "") == search)
            {
                return item;
            }

            return null;
        }

        public static async Task<SearchResults> LoadCategoryFast(BuildComponent.ComponentType type, CancellationToken? token = null)
        {
            var client = SharedHttpClient.Instance;

            var url = $"https://microc.bbarrett.me/MicroCenterProxy/getCachedCategory/{(int)type}";
            var response = await (token != null ? client.GetAsync(url, token.Value) : client.GetAsync(url));
            if (response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<SearchResults>(body);
                return result;
            }

            return new SearchResults();
        }

        public static async Task<SearchResults> LoadMultipleFast(List<string> skus, CancellationToken? token = null)
        {
            var client = SharedHttpClient.Instance;

            var url = $"https://microc.bbarrett.me/MicroCenterProxy/getCachedSkus/{string.Join(",", skus)}";
            var response = await (token != null ? client.GetAsync(url, token.Value) : client.GetAsync(url));
            if (response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<SearchResults>(body);
                return result;
            }

            return new SearchResults();
        }

        public static async Task<SearchResults> ParseBody(string body, CancellationToken? token = null)
        {
            var result = new SearchResults();

            MatchCollection shortMatches;
            try
            {
                //
                // MicroCenter's search result markup no longer emits data-name/data-id/price/data-brand/href/src
                // in a fixed order on a single line, so we can't match them as one sequential regex anymore.
                // Instead grab each product anchor's attribute blob + inner content, then pull fields out of
                // that blob independently of attribute order.
                //
                shortMatches = Regex.Matches(body, "<a id=\"hypProduct_\\d+\" class=\"image2 productClickItemV2\"([^>]*)>(.*?)</a>", RegexOptions.Singleline, RegexTimeout);
                var stockMatches = Regex.Matches(body, "<div class=\"stock\">(.+?)<\\/div>", RegexOptions.Singleline, RegexTimeout);
                var skuMatches = Regex.Matches(body, "<p class=\"sku\">SKU: (\\d{6})</p>", RegexOptions.None, RegexTimeout);
                //
                // Anchored on class="clearance"> rather than the bare string "clearance": the search
                // page's header category dropdown carries data-eventlabel="clearance" nine times
                // before the product grid even starts, and those junk matches shifted the whole
                // collection - ParseItems pairs clearanceMatches[i] with shortMatches[i] by index, so
                // every product was reading some other product's clearance block (or nav markup).
                // Verified against a live 96-result page: this yields exactly 96 blocks, one per
                // product anchor (empty for products with no open-box listing), which is what the
                // by-index pairing needs. The stock/SKU collections were already 1:1 at 96 each.
                //
                var clearanceMatches = Regex.Matches(body, "class=\"clearance\">.*?<\\/div>", RegexOptions.Singleline, RegexTimeout);
                var newItems = new List<Item>();

                //
                // The "Showing X-Y of Z" summary is how we know a real total, including the Z=0 case MicroCenter
                // uses for a genuine no-match query (which still renders a handful of unrelated "you might like"
                // filler products using this same productClickItemV2 markup - so Items.Count alone can't tell
                // a real match from filler). But a query that matches exactly one product (e.g. a SKU/UPC fast
                // lookup) skips the summary block entirely, so its absence isn't itself a signal of zero results.
                //
                // Any single wrapping tag, not specifically <strong> - MicroCenter swapping the tag
                // used to highlight the count (e.g. to <b> or <span>) shouldn't drop this to the
                // filler-query fallback below. (LoadAll no longer trusts a too-low TotalResults
                // blindly either way, but this keeps the value itself accurate for anything that
                // reads TotalResults directly - the tests, a "Showing X of Y" UI, etc.)
                // A count over 999 renders with a thousands separator ("of 1,234 results"), which
                // \d+ can't match past the comma - the summary regex simply failed to match at all
                // for any popular category, falling through to the filler-query heuristic below and
                // under-reporting shortMatches.Count as the total instead of the real count.
                var match = Regex.Match(body, "of\\s*<[^>]+>\\s*([\\d,]+)\\s*</[^>]+>", RegexOptions.None, RegexTimeout);
                if (match.Success)
                {
                    if (int.TryParse(match.Groups[1].Value.Replace(",", ""), NumberStyles.Integer, CultureInfo.InvariantCulture, out int totalResults))
                    {
                        result.TotalResults = totalResults;
                    }
                }
                else if (!Regex.IsMatch(body, "st-search-summary", RegexOptions.None, RegexTimeout))
                {
                    result.TotalResults = shortMatches.Count;
                }

                return ParseItems(body, result, shortMatches, stockMatches, skuMatches, clearanceMatches, token);
            }
            catch (RegexMatchTimeoutException)
            {
                // See Item.ParsePlans for the same reasoning - a body-wide pattern that no longer
                // matches MicroCenter's current markup shouldn't be able to hang search entirely.
                return result;
            }
        }

        private static SearchResults ParseItems(string body, SearchResults result, MatchCollection shortMatches, MatchCollection stockMatches, MatchCollection skuMatches, MatchCollection clearanceMatches, CancellationToken? token)
        {
            for (int i = 0; i < shortMatches.Count; i++)
            {

                token?.ThrowIfCancellationRequested();

                bool comingSoon = false;
                Match m = shortMatches[i];
                var attrs = m.Groups[1].Value;
                var inner = m.Groups[2].Value;

                string GetAttr(string attrName)
                {
                    var attrMatch = Regex.Match(attrs, attrName + "=\"(.*?)\"");
                    return attrMatch.Success ? attrMatch.Groups[1].Value : "";
                }

                string stock = "0";
                if (i < stockMatches.Count)
                {
                    Match stockMatch = stockMatches[i];
                    var stockHtml = string.IsNullOrWhiteSpace(stockMatch.Groups[1].Value) ? "0" : stockMatch.Groups[1].Value;
                    var stockRegex = new Regex("<span class=\"inventoryCnt\">(.*?) <").Match(stockHtml);
                    stock = stockRegex.Success ? stockRegex.Groups[1].Value : "0";
                }

                string sku = "000000";
                if (i < skuMatches.Count)
                {
                    var skuMatch = skuMatches[i];
                    sku = skuMatch.Groups[1].Value ?? "000000";
                }

                var url = GetAttr("href");
                string id = "000000";
                Match m_id = Regex.Match(url, "/product/(\\d+)/");
                if (m_id.Success)
                {
                    id = m_id.Groups[1].Value;
                }
                else
                {
                    Debug.WriteLine("ID NOT FOUND FOR SEARCH RESULT");
                    Debug.WriteLine(m.Value);
                }

                var clearanceBody = i < clearanceMatches.Count ? clearanceMatches[i].Value : "";
                var m_clearanceQty = Regex.Match(clearanceBody, "clearance\">[^\\<\\>]*?(\\d+)", RegexOptions.Singleline);
                //
                // The price sits inside a nested <strong> ("2 open box from <span><strong>$2,159.96
                // </strong></span>"), so the old "<span>\$" - which required the dollar sign to
                // follow <span> immediately - never matched and no search result ever came back with
                // clearance data at all. Matching on the "$" itself covers either shape, and the
                // digit class has to allow the thousands separator or a $2,159.96 open-box listing
                // parses as $2.00.
                //
                var m_clearancePrice = Regex.Match(clearanceBody, "clearance\">.*?\\$([\\d,.]+)", RegexOptions.Singleline);
                int clearanceQty = 0;
                float clearancePrice = 0f;
                if(m_clearanceQty.Success && m_clearancePrice.Success)
                {
                    int.TryParse(m_clearanceQty.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out clearanceQty);
                    float.TryParse(m_clearancePrice.Groups[1].Value.Replace(",", ""), NumberStyles.Float, CultureInfo.InvariantCulture, out clearancePrice);
                }

                var clearanceInfo = new List<ClearanceInfo>();
                for(int c_i = 0; c_i < clearanceQty; c_i++)
                {
                    clearanceInfo.Add(new ClearanceInfo() { Price = clearancePrice });
                }

                var srcMatch = Regex.Match(inner, "src=\"(.*?)\"");

                float.TryParse(GetAttr("data-price"), NumberStyles.Float, CultureInfo.InvariantCulture, out float price);
                var item = new Item()
                {
                    Name = Item.HttpDecode(GetAttr("data-name")),
                    ID = id,
                    Price = price,
                    OriginalPrice = price,
                    Brand = GetAttr("data-brand"),
                    URL = url,
                    PictureUrls = new List<string>() { srcMatch.Success ? srcMatch.Groups[1].Value : "" },
                    Stock = stock,
                    SKU = sku,
                    ComingSoon = comingSoon,
                    ClearanceItems = clearanceInfo
                };

                result.Items.Add(item);
            }
            token?.ThrowIfCancellationRequested();
            return result;
        }
    }

    public class SearchResults
    {
        public int TotalResults { get; set; }
        public int Page { get; set; }
        public List<Item> Items { get; set; }

        public SearchResults()
        {
            Items = new List<Item>();
        }
    }

}
