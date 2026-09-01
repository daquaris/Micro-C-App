using MicroCLib.Models;
using System;
using System.Collections.Generic;
using System.Text;
using static MicroCLib.Models.BuildComponent;

namespace MicroCLib.Models.Build
{
    public interface IBuildComponentDependency
    {
        List<DependencyResult> HasErrors(List<Item> items);
        string? HintText(List<Item> items, ComponentType type);
        string Name { get; }
    }
}
