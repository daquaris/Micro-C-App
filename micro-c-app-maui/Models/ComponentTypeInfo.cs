using System;
using static MicroCLib.Models.BuildComponent;

namespace micro_c_app_maui.Models
{
    public class ComponentTypeInfo
    {
        public ComponentType Type { get; set; }
        public string Name { get; set; }
        public string SearchCategory { get; set; }
        public string Icon { get; set; }

        public ComponentTypeInfo()
        {
        }

        public ComponentTypeInfo(ComponentType type)
        {
            Type = type;
            Name = Enum.GetName(typeof(ComponentType), type);
            SearchCategory = CategoryFilterForType(type);
            Icon = "";
        }

        public ComponentTypeInfo(ComponentType type, string icon)
        {
            Type = type;
            Name = Enum.GetName(typeof(ComponentType), type);
            SearchCategory = CategoryFilterForType(type);
            Icon = icon;
        }
    }
}
