using System;
using System.Collections.Generic;
using System.Text;

namespace micro_c_lib.Models.Inventory
{
    public class InventoryLocation
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Store { get; set; }
        public string Aisle { get; set; }
        public string Section { get; set; }
        public string Identifier { get; set; }
        public DateTime LastFullScan { get; set; }
        public ICollection<InventoryEntry> Entries { get; set; }
    }
}
