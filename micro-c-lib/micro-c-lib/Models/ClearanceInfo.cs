using MicroCLib.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace micro_c_lib.Models
{
    public class ClearanceInfo : NotifyPropertyChangedItem
    {
        private string state;
        private float price;
        private string id;
        private string location;

        public string Id { get => id; set => SetProperty(ref id, value); }
        public string State { get => state; set => SetProperty(ref state, value); }
        public float Price { get => price; set => SetProperty(ref price, value); }
        public string Location { get => location; set => SetProperty(ref location, value); }
    }
}
