using MicroCLib.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using static MicroCLib.Models.BuildComponent;
using System.Linq;

namespace MicroCLib.Models.Build
{
    public class MemorySpeedDependency : FieldContainsDependency
    {
        public MemorySpeedDependency(string name, ComponentType first, string firstField, ComponentType second, string secondField)
            : base(name, first, firstField, second, secondField)
        {

        }

        protected override bool Compatible(string firstValue, string secondValue)
        {
            var firstSpeeds = ProcessSpeedString(firstValue);
            var secondSpeeds = ProcessSpeedString(secondValue);

            return firstSpeeds.Any(s1 => secondSpeeds.Any(s2 => s2 == s1));
        }

        private IEnumerable<string> ProcessSpeedString(string value)
        {
            // (\d+) matched every digit run, including the DDR generation number - "DDR4-3200" and
            // "DDR4-2133" both contain a bare "4", so Compatible's cross-product ("does any speed in
            // A match any speed in B") reported them compatible on that shared "4" alone regardless of
            // the actual 3200 vs 2133 mismatch. DDR speeds are always >= 3 digits (down to DDR-200);
            // the generation number is always exactly 1, so requiring 3+ digits excludes it.
            var matches = Regex.Matches(value, "\\d{3,}");
            return matches.OfType<Match>().Select(m => m.Value);
        }
    }
}