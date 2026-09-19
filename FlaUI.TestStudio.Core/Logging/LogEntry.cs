using FlaUI.TestStudio.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace FlaUI.TestStudio.Core.Logging
{
    /// <summary>
    /// Ein einzelner Log-Eintrag. Enthält neben dem Text auch Level und Zeitstempel,
    /// damit UI oder Datei-Sinks selbst entscheiden können, wie sie ihn formatieren/filtern.
    /// </summary>
    public sealed record LogEntry(DateTime Timestamp, LogLevel Level, string Message)
    {
        public override string ToString() => $"[{Timestamp:HH:mm:ss}] [{Level}] {Message}";
    }
}
