using FlaUI.TestStudio.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace FlaUI.TestStudio.Core.Logging
{
    /// <summary>
    /// Zentraler, UI-unabhängiger Logger.
    /// Erzeugt strukturierte LogEntry-Objekte und feuert ein Event.
    /// Beliebig viele Abonnenten (TextBox, Datei, Konsole, ...) können sich anmelden,
    /// ohne dass der Logger selbst etwas über sie wissen muss.
    /// </summary>
    public static class Logger
    {
        public static event Action<LogEntry>? OnLog;

        /// <summary>
        /// Minimales Level, das überhaupt weitergereicht wird. Standard: Debug (alles).
        /// So kann man z.B. im Release-Build auf Info hochsetzen, ohne Aufrufstellen zu ändern.
        /// </summary>
        public static LogLevel MinimumLevel { get; set; } = LogLevel.Debug;

        public static void D(string message) => Write(LogLevel.Debug, message);
        public static void I(string message) => Write(LogLevel.Info, message);
        public static void W(string message) => Write(LogLevel.Warning, message);
        public static void E(string message) => Write(LogLevel.Error, message);
        public static void E(Exception ex, string? context = null) =>
            Write(LogLevel.Error, context is null ? ex.ToString() : $"{context}: {ex}");

        private static void Write(LogLevel level, string message)
        {
            if (level < MinimumLevel) return;
            OnLog?.Invoke(new LogEntry(DateTime.Now, level, message));
        }
    }
}
