using System.Windows.Input;
using FlaUI.TestStudio.App.Native;

namespace FlaUI.TestStudio.App.Services
{
    /// <summary>Was ein Hotkey während der Aufnahme auslösen soll.</summary>
    public enum RecordAction
    {
        ConfirmClick,
        ConfirmSetText,
        AddWait,
        ConfirmAssert,
        StopRecording
    }

    public sealed record HotkeyDefault(RecordAction Action, NativeMethods.ModifierKeys Modifiers, Key Key, string Description);

    /// <summary>
    /// Standard-Tastenbelegung für die Aufnahme. Neue Aktion gewünscht?
    /// Einfach hier einen weiteren Eintrag ergänzen - RecordingCoordinator und
    /// GlobalHotkeyService lesen ausschließlich aus dieser Liste.
    /// </summary>
    public static class RecordingHotkeys
    {
        public static readonly IReadOnlyList<HotkeyDefault> Defaults = new List<HotkeyDefault>
        {
            new(RecordAction.ConfirmClick,   NativeMethods.ModifierKeys.None, Key.F8,  "Click-Schritt"),
            new(RecordAction.ConfirmSetText, NativeMethods.ModifierKeys.None, Key.F9,  "Text-setzen-Schritt"),
            new(RecordAction.AddWait,        NativeMethods.ModifierKeys.None, Key.F10, "Warten-Schritt"),
            new(RecordAction.ConfirmAssert,  NativeMethods.ModifierKeys.None, Key.F11, "Sichtbarkeits-Prüfung"),
            new(RecordAction.StopRecording,  NativeMethods.ModifierKeys.None, Key.F1, "Aufnahme stoppen"),
        };
    }
}
