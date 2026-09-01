using micro_c_lib.Models;
using MicroCLib.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        static HttpClient newClient() {
            var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(15);
            return client;
        }

        
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
                if(token != null && token.Value.IsCancellationRequested)
                {
                    return new SearchResults() { };
                }

                var addResult = await LoadQuery(searchQuery, storeID, categoryFilter, orderBy, page, token);
                if(addResult != null && addResult.Items.Count == 0)
                {
                    result.TotalResults = result.Items.Count;
                    break;
                }
                result.Items.AddRange(addResult.Items);
                result.TotalResults = addResult.TotalResults;
                page++;
            }

            token?.ThrowIfCancellationRequested();
            return result;

        }

        public static async Task<SearchResults> LoadQuery(string searchQuery, string storeID, string categoryFilter, OrderByMode orderBy, int page, CancellationToken? token = null, IProgress<ProgressInfo> progress = null)
        {
            var client = newClient();
            token?.Register(() =>
            {
                client?.CancelPendingRequests();
            });

            progress?.Report(new ProgressInfo($"Loading query {searchQuery}", .3));

            var url = GetSearchUrl(searchQuery, storeID, categoryFilter, orderBy, RESULTS_PER_PAGE, page);
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/112.0");
            client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8");
            client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");
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

            Dictionary<string, string> query = new Dictionary<string, string>()
            {
                {"query", searchQuery},
                {"storeId", storeID },
                {"categoryFilter", categoryFilter },
                {"orderBy", "0"},
                {"page", "0" }
            };

            var client = newClient();

            string queryString = string.Join("&", query.Select((x) => x.Key + "=" + x.Value?.ToString()));
            var url = $"https://microc.bbarrett.me/MicroCenterProxy/searchAll?{queryString}";
            var response = await (token != null ? client.GetAsync(url, token.Value) : client.GetAsync(url));
            if (response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<SearchResults>(body);
                return result;
            }

            return new SearchResults();
        }

        public static async Task<Item> LoadFast(string search, CancellationToken? token = null)
        {
            // MicroCenter's search endpoint is 1-indexed - page=0 silently returns zero results for every
            // query (SKU, UPC, or text), which made every fast lookup fail regardless of parsing.
            var res = await LoadQuery(search, "141", null, OrderByMode.match, 1, token);
            if(res != null && res.TotalResults > 0)
            {
                return await Item.FromUrl(res.Items[0].URL, "141", token);
            }
            else
            {
                return null;
            }

            var client = newClient();

            var url = $"https://microc.bbarrett.me/MicroCenterProxy/getCached/{search}";
            var response = await (token != null ? client.GetAsync(url, token.Value) : client.GetAsync(url));
            if (response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<Item>(body);
                return result;
            }

            return null;
        }

        public static async Task<SearchResults> LoadCategoryFast(BuildComponent.ComponentType type, CancellationToken? token = null)
        {
            var client = newClient();

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
            var client = newClient();

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

            //
            // MicroCenter's search result markup no longer emits data-name/data-id/price/data-brand/href/src
            // in a fixed order on a single line, so we can't match them as one sequential regex anymore.
            // Instead grab each product anchor's attribute blob + inner content, then pull fields out of
            // that blob independently of attribute order.
            //
            var shortMatches = Regex.Matches(body, "<a id=\"hypProduct_\\d+\" class=\"image2 productClickItemV2\"([^>]*)>(.*?)</a>", RegexOptions.Singleline);
            var stockMatches = Regex.Matches(body, "<div class=\"stock\">(.+?)<\\/div>", RegexOptions.Singleline);
            var skuMatches = Regex.Matches(body, "<p class=\"sku\">SKU: (\\d{6})</p>");
            var clearanceMatches = Regex.Matches(body, "\"clearance\".*?<\\/div>", RegexOptions.Singleline);
            var newItems = new List<Item>();

            //
            // The "Showing X-Y of Z" summary is how we know a real total, including the Z=0 case MicroCenter
            // uses for a genuine no-match query (which still renders a handful of unrelated "you might like"
            // filler products using this same productClickItemV2 markup - so Items.Count alone can't tell
            // a real match from filler). But a query that matches exactly one product (e.g. a SKU/UPC fast
            // lookup) skips the summary block entirely, so its absence isn't itself a signal of zero results.
            //
            var match = Regex.Match(body, "of\\s*<strong>\\s*(\\d+)\\s*</strong>");
            if (match.Success)
            {
                if (int.TryParse(match.Groups[1].Value, out int totalResults))
                {
                    result.TotalResults = totalResults;
                }
            }
            else if (!Regex.IsMatch(body, "st-search-summary"))
            {
                result.TotalResults = shortMatches.Count;
            }

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
                var m_clearancePrice = Regex.Match(clearanceBody, "clearance\">.*?<span>\\$([\\d.]+)", RegexOptions.Singleline);
                int clearanceQty = 0;
                float clearancePrice = 0f;
                if(m_clearanceQty.Success && m_clearancePrice.Success)
                {
                    int.TryParse(m_clearanceQty.Groups[1].Value, out clearanceQty);
                    float.TryParse(m_clearancePrice.Groups[1].Value, out clearancePrice);
                }

                var clearanceInfo = new List<ClearanceInfo>();
                for(int c_i = 0; c_i < clearanceQty; c_i++)
                {
                    clearanceInfo.Add(new ClearanceInfo() { Price = clearancePrice });
                }

                var srcMatch = Regex.Match(inner, "src=\"(.*?)\"");

                float.TryParse(GetAttr("data-price"), out float price);
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
