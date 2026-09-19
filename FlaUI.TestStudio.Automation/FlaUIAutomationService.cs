using FlaUI.TestStudio.Core.Interface;
using FlaUI.TestStudio.Core.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace FlaUI.TestStudio.Automation
{
    /// <summary>
    /// Dünner Adapter um die bestehende FlaUIAutomation-Klasse, damit sie
    /// dem IAutomationService-Interface entspricht. Falls FlaUIAutomation
    /// bereits alle Methoden 1:1 hat, ist das hier reine Delegation.
    /// </summary>
    public sealed class FlaUIAutomationService : IAutomationService
    {
        private readonly FlaUIAutomation _inner = new();

        public void Launch(string path) => _inner.Launch(path);
        public void Click(Selector selector) => _inner.Click(selector);
        public void SetText(Selector selector, string text) => _inner.SetText(selector, text);
        public bool Exists(Selector selector) => _inner.Exists(selector);
        public void AttachToProcess(int processId) => _inner.AttachToProcess(processId);

        public void Dispose() => _inner.Dispose();
    }
}
