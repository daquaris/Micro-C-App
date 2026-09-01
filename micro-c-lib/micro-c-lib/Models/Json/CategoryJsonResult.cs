using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MicroCLib.Models.Json
{
    internal class CategoryJsonResult
    {
        [JsonProperty(PropertyName = "itemListElement")]
        public List<CategoryInfo> Categories { get; set; }
    }
}
