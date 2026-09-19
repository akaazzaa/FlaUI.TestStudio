using FlaUI.Core.AutomationElements;
using FlaUI.TestStudio.Core.Model;
using FlaUI.UIA3;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace FlaUI.TestStudio.Automation
{
    public sealed class ElementInspector : IDisposable
    {
        private readonly UIA3Automation _automation = new();

        public AutomationElement? FromPoint(Point point) => _automation.FromPoint(point);

        public Selector CreateSelector(AutomationElement element) => new  ()
        {
            AutomationId = element.Properties.AutomationId.ValueOrDefault,
            Name = element.Properties.Name.ValueOrDefault,
            ControlType = element.Properties.ControlType.ValueOrDefault.ToString(),
            ClassName = element.Properties.ClassName.ValueOrDefault
        };

        public void Dispose() => _automation.Dispose();
    }
}
