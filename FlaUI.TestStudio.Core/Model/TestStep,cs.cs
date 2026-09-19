using System;
using System.Collections.Generic;
using System.Text;

namespace FlaUI.TestStudio.Core.Model
{
    public sealed class TestStep
    {
        public StepType Type { get; set; }
        public Selector? Target { get; set; }
        public string? Value { get; set; }
        public int WaitMilliseconds { get; set; } = 500;
        public override string ToString() => Type switch
        {
            StepType.Click => $"Click: {Target}",
            StepType.SetText => $"Set Text: {Target} = \"{Value}\"",
            StepType.AssertVisible => $"Assert Visible: {Target}",
            StepType.Wait => $"Wait: {WaitMilliseconds} ms",
            StepType.LaunchApplication => $"Start: {Value}",
            _ => Type.ToString()
        };
    }
}
