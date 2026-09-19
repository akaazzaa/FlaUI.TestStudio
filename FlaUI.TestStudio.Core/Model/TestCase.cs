using System;
using System.Collections.Generic;
using System.Text;

namespace FlaUI.TestStudio.Core.Model
{
    public sealed class TestCase
    {
        public string Name { get; set; } = "New Test";
        public List<TestStep> Steps { get; set; } = [];
    }
}
