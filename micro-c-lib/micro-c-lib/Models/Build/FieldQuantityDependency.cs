using MicroCLib.Models.Build;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MicroCLib.Models
{
    public class FieldQuantityDependency : BuildComponentDependency
    {
        public BuildComponent.ComponentType subType { get; set; }
        public BuildComponent.ComponentType containerType { get; set; }

        public string? subTypeFieldName { get; set; }
        public string? containerFieldName { get; set; }

        public FieldQuantityDependency(string name, BuildComponent.ComponentType subItem, BuildComponent.ComponentType containerItem, string? containerField = null, string? subField = null)
            : base(name)
        {
            subType = subItem;
            containerType = containerItem;
            containerFieldName = containerField;
            subTypeFieldName = subField;
        }

        public override string HintText(List<Item> items, BuildComponent.ComponentType type)
        {
            if(type == subType)
            {
                var containerItems = items.Where(i => i.ComponentType == containerType);
                if(containerItems.Count() == 0)
                {
                    return null;
                }
                int containerSlots;
                if (!string.IsNullOrWhiteSpace(containerFieldName))
                {
                    containerSlots = containerItems.Sum(i => GetValue(i, containerFieldName) ?? 0);
                }
                else
                {
                    containerSlots = containerItems.Count();
                }

                return $"{containerType} has {containerSlots} available for {subType}";
            }
            if(type == containerType)
            {
                var subItems = items.Where(i => i.ComponentType == subType);
                if(subItems.Count() == 0)
                {
                    return null;
                }

                int subItemCount;

                if (!string.IsNullOrWhiteSpace(subTypeFieldName))
                {
                    subItemCount = subItems.Sum(i => GetValue(i, subTypeFieldName) ?? 0);
                }
                else
                {
                    subItemCount = subItems.Count();
                }

                return $"{containerType} must have support for {subItemCount} {subType}";
            }

            return null;
        }

        public override List<DependencyResult> HasErrors(List<Item> items)
        {
            // Was filtering on GetValue(i, containerFieldName).HasValue unconditionally, even when
            // containerFieldName is null (e.g. the "Has OS" dependency, which omits both field names
            // and just compares item counts) - GetValue(item, null) always returns null, so
            // containerItems was always empty and HasErrors returned early with zero errors on every
            // call, silently disabling the whole dependency. subItems below already branches on
            // subTypeFieldName being blank for the same reason; this mirrors that.
            IEnumerable<Item> containerItems;
            if (string.IsNullOrWhiteSpace(containerFieldName))
            {
                containerItems = items.Where(i => i.ComponentType == containerType);
            }
            else
            {
                containerItems = items.Where(i => i.ComponentType == containerType && GetValue(i, containerFieldName).HasValue);
            }

            if (containerItems.Count() == 0)
            {
                return new List<DependencyResult>();
            }

            IEnumerable<Item> subItems;
            if (string.IsNullOrWhiteSpace(subTypeFieldName))
            {
                subItems = items.Where(i => i.ComponentType == subType);
            }
            else
            {
                subItems = items.Where(i => i.ComponentType == subType && GetValue(i, subTypeFieldName).HasValue);
            }

            if(subItems.Count() == 0)
            {
                return new List<DependencyResult>();
            }

            int containerSlots;
            if (!string.IsNullOrWhiteSpace(containerFieldName))
            {
                containerSlots = containerItems.Sum(i => GetValue(i, containerFieldName) ?? 0);
            }
            else
            {
                containerSlots = containerItems.Count();
            }

            int subItemCount;
            if(!string.IsNullOrWhiteSpace(subTypeFieldName))
            {
                subItemCount = subItems.Sum(i => GetValue(i, subTypeFieldName) ?? 0);
            }
            else
            {
                subItemCount = subItems.Count();
            }


            if(subItemCount > containerSlots)
            {
                return containerItems.Concat(subItems)
                    .Select(i =>
                        new DependencyResult(i, $"{containerType} must have at least {subItemCount} {subType}")
                    ).ToList();
            }


            return new List<DependencyResult>();
        }

        private static int? GetValue(Item item, string field)
        {
            if(item == null || item.Specs == null || field == null || !item.Specs.ContainsKey(field))
            {
                return null;
            }

            var spec = item.Specs[field];
            var specNumber = Regex.Match(spec, "([\\d\\.]+)").Groups[1].Value;
            if (int.TryParse(specNumber, out int val))
            {
                return val;
            }

            return null;
        }

        public override bool Applicable(BuildComponent.ComponentType type)
        {
            return type == containerType || type == subType;
        }
    }
}