using FlaUI.TestStudio.Core.Logging;
using FlaUI.TestStudio.Core.Interface;
using FlaUI.TestStudio.Core.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace FlaUI.TestStudio.Automation
{
    /// <summary>
    /// Ergebnis eines kompletten Testlaufs.
    /// </summary>
    public sealed record TestRunResult(bool Success, string? FailureMessage);

    /// <summary>
    /// Führt die Schritte eines TestCase aus. Statt eines starren switch-Statements
    /// wird pro StepType ein Handler registriert (Dictionary). Neue Step-Typen lassen
    /// sich so hinzufügen, ohne diese Klasse zu ändern (Open/Closed-Prinzip) -
    /// z.B. per RegisterHandler von außen, oder durch Erweitern von BuildDefaultHandlers.
    /// </summary>
    public sealed class TestStepExecutor
    {
        public delegate Task StepHandler(TestStep step, IAutomationService automation);

        private readonly Dictionary<StepType, StepHandler> _handlers;

        public TestStepExecutor(IAutomationService automation, IEnumerable<KeyValuePair<StepType, StepHandler>>? extraHandlers = null)
        {
            _handlers = BuildDefaultHandlers();
            if (extraHandlers != null)
            {
                foreach (var kvp in extraHandlers)
                    _handlers[kvp.Key] = kvp.Value; // erlaubt auch Überschreiben bestehender Handler
            }
        }

        public void RegisterHandler(StepType type, StepHandler handler) => _handlers[type] = handler;

        public async Task<TestRunResult> RunAsync(TestCase test, IAutomationService automation)
        {
            Logger.I($"=== RUN: {test.Name} ===");

            foreach (var step in test.Steps)
            {
                Logger.I("→ " + step);

                if (!_handlers.TryGetValue(step.Type, out var handler))
                {
                    var msg = $"Kein Handler für Step-Typ '{step.Type}' registriert.";
                    Logger.E(msg);
                    return new TestRunResult(false, msg);
                }

                try
                {
                    await handler(step, automation);
                }
                catch (Exception ex)
                {
                    Logger.E(ex, "=== TEST FAILED ===");
                    return new TestRunResult(false, ex.Message);
                }

                Logger.I("  ✓ OK");
            }

            Logger.I("=== TEST PASSED ===");
            return new TestRunResult(true, null);
        }

        private static Dictionary<StepType, StepHandler> BuildDefaultHandlers() => new()
        {
            [StepType.LaunchApplication] = (step, automation) =>
            {
                automation.Launch(step.Value!);
                return Task.CompletedTask;
            },
            [StepType.Click] = (step, automation) =>
            {
                automation.Click(step.Target!);
                return Task.CompletedTask;
            },
            [StepType.SetText] = (step, automation) =>
            {
                automation.SetText(step.Target!, step.Value ?? "");
                return Task.CompletedTask;
            },
            [StepType.Wait] = (step, _) => Task.Delay(step.WaitMilliseconds),
            [StepType.AssertVisible] = (step, automation) =>
            {
                if (!automation.Exists(step.Target!))
                    throw new Exception($"Assertion fehlgeschlagen: {step.Target}");
                return Task.CompletedTask;
            },
        };
    }
}
