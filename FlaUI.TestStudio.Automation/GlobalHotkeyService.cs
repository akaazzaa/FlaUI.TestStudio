using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using FlaUI.TestStudio.App.Native;

namespace FlaUI.TestStudio.App.Services
{
    /// <summary>
    /// Registriert systemweite Hotkeys (Win32 RegisterHotKey), die auch dann feuern,
    /// wenn die Zielanwendung (nicht TestStudio) gerade den Fokus hat. Notwendig, damit
    /// F8/F9/... während des Testens in der Zielanwendung überhaupt ankommen.
    /// </summary>
    public sealed class GlobalHotkeyService : IDisposable
    {
        private readonly HwndSource _source;
        private readonly Dictionary<int, Action> _handlers = new();
        private int _nextId = 0xB000; // eigener ID-Bereich, um Kollisionen mit anderen Hotkeys zu vermeiden

        public GlobalHotkeyService(Window window)
        {
            var helper = new WindowInteropHelper(window);
            if (helper.Handle == IntPtr.Zero)
                helper.EnsureHandle(); // erzwingt die Fenster-Erstellung, falls noch nicht sichtbar

            _source = HwndSource.FromHwnd(helper.Handle)
                       ?? throw new InvalidOperationException("Konnte HwndSource für Hotkeys nicht ermitteln.");
            _source.AddHook(WndProc);
        }

        /// <summary>Registriert einen globalen Hotkey. Wirft, falls die Kombination schon belegt ist.</summary>
        public int Register(NativeMethods.ModifierKeys modifiers, Key key, Action onPressed)
        {
            var id = _nextId++;
            var vk = (uint)KeyInterop.VirtualKeyFromKey(key);

            if (!NativeMethods.RegisterHotKey(_source.Handle, id, (uint)(modifiers | NativeMethods.ModifierKeys.NoRepeat), vk))
                throw new InvalidOperationException($"Hotkey {modifiers}+{key} konnte nicht registriert werden (evtl. von anderer Anwendung belegt).");

            _handlers[id] = onPressed;
            return id;
        }

        public void Unregister(int id)
        {
            if (_handlers.Remove(id))
                NativeMethods.UnregisterHotKey(_source.Handle, id);
        }

        public void UnregisterAll()
        {
            foreach (var id in _handlers.Keys.ToList())
                Unregister(id);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == NativeMethods.WM_HOTKEY && _handlers.TryGetValue(wParam.ToInt32(), out var action))
            {
                action();
                handled = true;
            }
            return IntPtr.Zero;
        }

        public void Dispose()
        {
            UnregisterAll();
            _source.RemoveHook(WndProc);
        }
    }
}
