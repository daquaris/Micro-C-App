using MicroCLib.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MicroCLib.Models.Build
{
    public class DependencyResult
    {
        public Item Primary { get; set; }
        public string Text { get; set; }

        public DependencyResult(Item primary, string text)
        {
            Primary = primary;
            Text = text;
        }
    }
}
