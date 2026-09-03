using MicroCLib.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace micro_c_app_maui
{
    public class SearchCache
    {
        public List<(DateTime created, Item item)> Items = new List<(DateTime created, Item item)>();
        public TimeSpan CacheLength { get; private set; }

        public SearchCache(TimeSpan cacheLength)
        {
            CacheLength = cacheLength;
        }

        public void SetCacheLength(TimeSpan length)
        {
            CacheLength = length;
        }

        private void Purge()
        {
            Items.RemoveAll(i => DateTime.UtcNow - i.created > CacheLength);
        }

        public Item? Get(string search)
        {
            Purge();
            return Items.Select(i => i.item).FirstOrDefault(i => i.SKU == search || (i.Specs?.ContainsKey("UPC") == true && i.Specs["UPC"] == search))?.CloneAndResetQuantity();
        }

        // Purge() only runs from Get(), on a TTL basis - a long session that searches many distinct
        // SKUs without ever calling Get() on a stale one (e.g. cataloging inventory by hand) grows
        // Items unbounded until the hour-long TTL happens to clear it. Cap it independently of time.
        private const int MAX_ITEMS = 100;

        public void Add(Item item)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.SKU))
            {
                return;
            }
            Items.RemoveAll(i => i.item.SKU == item.SKU);
            Items.Add((DateTime.UtcNow, item));

            if (Items.Count > MAX_ITEMS)
            {
                Items.RemoveRange(0, Items.Count - MAX_ITEMS);
            }
        }
    }
}
