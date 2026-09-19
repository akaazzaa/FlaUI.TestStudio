using FlaUI.TestStudio.Core.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace FlaUI.TestStudio.Core.Interface
{
    // <summary>
    /// Schnittstelle über die konkrete FlaUIAutomation-Implementierung.
    /// Damit kann der TestStepExecutor unabhängig von FlaUI getestet werden
    /// (z.B. mit einem Fake/Mock), und neue Automation-Backends lassen sich
    /// später ergänzen, ohne den Executor anzufassen.
    /// </summary>
    public interface IAutomationService : IDisposable
    {
        void Launch(string path);
        void Click(Selector selector);
        void SetText(Selector selector, string text);
        bool Exists(Selector selector);
        void AttachToProcess(int processId);
    }
}
