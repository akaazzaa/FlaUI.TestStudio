using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.TestStudio.Core.Model;
using FlaUI.UIA3;
using System;
using System.Collections.Generic;
using System.Text;

namespace FlaUI.TestStudio.Automation
{
    public sealed class FlaUIAutomation : IDisposable
    {
        private readonly UIA3Automation _automation = new();
        private Application? _application;
        private Window? _window;

        public void AttachToProcess(int processId)
        {
            _application = Application.Attach(processId);
            _window = _application.GetMainWindow(_automation);
        }

        public void Launch(string path)
        {
            _application = Application.Launch(path);
            _window = _application.GetMainWindow(_automation);
        }

        public AutomationElement? Find(Selector selector)
        {
            if (_window is null) throw new InvalidOperationException("Keine Anwendung verbunden.");
            if (!string.IsNullOrWhiteSpace(selector.AutomationId))
            {
                var e = _window.FindFirstDescendant(cf => cf.ByAutomationId(selector.AutomationId));
                if (e != null) return e;
            }
            if (!string.IsNullOrWhiteSpace(selector.Name) && !string.IsNullOrWhiteSpace(selector.ControlType))
            {
                var e = _window.FindFirstDescendant(cf =>
                    cf.ByName(selector.Name).And(cf.ByControlType(ParseControlType(selector.ControlType))));
                if (e != null) return e;
            }
            if (!string.IsNullOrWhiteSpace(selector.Name))
                return _window.FindFirstDescendant(cf => cf.ByName(selector.Name));
            return null;
        }

        public void Click(Selector selector)
        {
            var e = Find(selector) ?? throw new InvalidOperationException($"Element nicht gefunden: {selector}");
            if (e.Patterns.Invoke.IsSupported) e.Patterns.Invoke.Pattern.Invoke();
            else e.Click();
        }

        public void SetText(Selector selector, string text)
        {
            var e = Find(selector) ?? throw new InvalidOperationException($"Element nicht gefunden: {selector}");
            if (e.Patterns.Value.IsSupported) e.Patterns.Value.Pattern.SetValue(text);
            else { e.Focus(); e.Click(); e.AsTextBox().Enter(text); }
        }

        public bool Exists(Selector selector) => Find(selector) != null;

        private static FlaUI.Core.Definitions.ControlType ParseControlType(string value) =>
            Enum.TryParse<FlaUI.Core.Definitions.ControlType>(value, true, out var result)
                ? result : FlaUI.Core.Definitions.ControlType.Custom;

        public void Dispose()
        {
            try { _application?.Close(); } catch { }
            _automation.Dispose();
        }
    }
}
