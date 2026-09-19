using System;
using System.Collections.Generic;
using System.Text;

namespace FlaUI.TestStudio.Core.Model
{
    public class ApplicationItem
    {
        public int ProcessId { get; set; }
        public string DisplayName { get; set; } = "";
        public string ProcessName { get; set; } = "";

        public override string ToString()
        {
            return $"{DisplayName} ({ProcessName}, PID {ProcessId})";
        }
    }
}
