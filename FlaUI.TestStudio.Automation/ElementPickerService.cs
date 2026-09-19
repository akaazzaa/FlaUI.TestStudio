using System.Windows.Threading;
using FlaUI.TestStudio.Automation;
using FlaUI.TestStudio.Core.Logging;
using FlaUI.TestStudio.Core;
using FlaUI.TestStudio.Core.Model;

namespace FlaUI.TestStudio.App.Services
{
    public enum PickerMode
    {
        None,
        SingleSelect,
        Recording
    }

    /// <summary>
    /// Fragt per Timer wiederholt das UI-Element unter der Maus ab.
    /// Reine Abfrage - was ein Tastendruck damit macht (Step hinzufügen, Auswahl
    /// bestätigen, ...) entscheidet der Aufrufer (RecordingCoordinator/MainWindow).
    /// </summary>
    public sealed class ElementPickerService : IDisposable
    {
        private readonly ElementInspector _inspector;
        private readonly DispatcherTimer _timer;

        public PickerMode Mode { get; private set; } = PickerMode.None;
        public Selector? LastSelector { get; private set; }

        public event Action<Selector>? ElementHovered;

        public ElementPickerService(ElementInspector inspector, TimeSpan? pollInterval = null)
        {
            _inspector = inspector;
            _timer = new DispatcherTimer { Interval = pollInterval ?? TimeSpan.FromMilliseconds(150) };
            _timer.Tick += (_, _) => PollCursor();
        }

        public void Start(PickerMode mode)
        {
            Mode = mode;
            _timer.Start();
        }

        public void Stop()
        {
            Mode = PickerMode.None;
            _timer.Stop();
        }

        /// <summary>Fragt sofort ab, was gerade unter der Maus ist (z.B. wenn ein Hotkey gedrückt wurde).</summary>
        public Selector? PollCurrentSelector() => PollCursor();

        private Selector? PollCursor()
        {
            if (Mode == PickerMode.None) return LastSelector;
            try
            {
                var p = System.Windows.Forms.Cursor.Position;
                var el = _inspector.FromPoint(new System.Drawing.Point(p.X, p.Y));
                if (el == null) return null;

                var selector = _inspector.CreateSelector(el);
                LastSelector = selector;
                ElementHovered?.Invoke(selector);
                return selector;
            }
            catch (Exception ex)
            {
                Logger.E(ex, "Inspector Fehler");
                return null;
            }
        }

        public void Dispose() => _timer.Stop();
    }
}
