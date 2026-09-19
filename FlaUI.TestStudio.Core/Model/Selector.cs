using System;
using System.Collections.Generic;
using System.Text;

namespace FlaUI.TestStudio.Core.Model
{
    public sealed class Selector
    {
        public string? AutomationId { get; set; }
        public string? Name { get; set; }
        public string? ControlType { get; set; }
        public string? ClassName { get; set; }

        public override string ToString() =>
            !string.IsNullOrWhiteSpace(AutomationId) ? $"AutomationId={AutomationId}" :
            !string.IsNullOrWhiteSpace(Name) ? $"Name={Name}" : "Element";
    }
}
