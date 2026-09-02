using System;
using System.Net.Http;

namespace MicroCLib.Models
{
    // Search.cs and Item.cs used to create-and-dispose a fresh HttpClient per request. Disposing an
    // HttpClient doesn't release its underlying socket immediately, so repeated searches/lookups
    // (pagination, batch scanning) could exhaust the connection pool - the standard fix is one shared,
    // long-lived client. Headers are identical across every microcenter.com call in this library, so
    // they're set once here instead of per-request.
    internal static class SharedHttpClient
    {
        public static readonly HttpClient Instance = Create();

        private static HttpClient Create()
        {
            var client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(15)
            };
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/112.0");
            client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8");
            client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");
            return client;
        }
    }
}
