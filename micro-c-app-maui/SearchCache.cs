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

        public void Add(Item item)
        {
            if (string.IsNullOrWhiteSpace(item.SKU))
            {
                return;
            }
            Items.RemoveAll(i => i.item.SKU == item.SKU);
            Items.Add((DateTime.UtcNow, item));
        }
    }
}
