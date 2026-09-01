using System;
using System.Collections.Generic;
using System.Text;

namespace MicroCLib.Models.Reference
{
    public class PlanTier
    {
        public int Duration { get; set; }
        public float Price { get; set; }
        public string SKU { get; set; }

        public PlanTier(int duration, float price, string sku)
        {
            Duration = duration;
            Price = price;
            SKU = sku;
        }

        public PlanTier(int duration, float price)
        {
            Duration = duration;
            Price = price;
            SKU = "000000";
        }

        public Item GetItem(string name)
        {
            return new Item()
            {
                Name = name,
                Price = Price,
                OriginalPrice = Price,
                SKU = SKU,
                Brand = "Micro Center",
                ComponentType = BuildComponent.ComponentType.Plan,
                Quantity = 1,
            };
        }

        public Item GetItem(PlanReference parent)
        {
            return new Item()
            {
                Name = $"{Duration} Year {parent.Name}",
                Price = Price,
                OriginalPrice = Price,
                SKU = SKU,
                Brand = "Micro Center",
                ComponentType = BuildComponent.ComponentType.Plan,
                Quantity = 1,
            };
        }
    }
}
