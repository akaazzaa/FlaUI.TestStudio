using FlaUI.TestStudio.App.Overlay;
using FlaUI.TestStudio.Core.Logging;
using FlaUI.TestStudio.Core.Model;

namespace FlaUI.TestStudio.App.Services
{
    /// <summary>
    /// Bündelt Hotkeys, Maus-Polling und das Recording-Overlay zu einem einzigen
    /// "Aufnahme"-Baustein. MainWindow ruft nur noch Start()/Stop() auf und
    /// reagiert auf StepRecorded - die Verdrahtung von Hotkeys/Overlay passiert hier,
    /// nicht mehr im Code-Behind des Fensters.
    /// </summary>
    public sealed class RecordingCoordinator : IDisposable
    {
        private readonly GlobalHotkeyService _hotkeys;
        private readonly ElementPickerService _picker;
        private readonly RecordingIndicatorWindow _indicator = new();
        private readonly List<int> _registeredHotkeyIds = new();
        private int _stepCount;

        public bool IsRecording { get; private set; }

        /// <summary>Gefeuert, wenn während der Aufnahme ein neuer Step entstehen soll.</summary>
        public event Action<TestStep>? StepRecorded;

        public RecordingCoordinator(GlobalHotkeyService hotkeys, ElementPickerService picker)
        {
            _hotkeys = hotkeys;
            _picker = picker;
            _picker.ElementHovered += selector =>
            {
                if (IsRecording) _indicator.UpdateHoveredElement(selector);
            };
        }

        public void Start()
        {
            if (IsRecording) return;
            IsRecording = true;
            _stepCount = 0;

            _picker.Start(PickerMode.Recording);
            _indicator.SetStepCount(0);
            _indicator.Show();

            foreach (var def in RecordingHotkeys.Defaults)
            {
                var action = def.Action;
                var id = _hotkeys.Register(def.Modifiers, def.Key, () => OnHotkey(action));
                _registeredHotkeyIds.Add(id);
            }

            Logger.I("Aufnahme gestartet.");
        }

        public void Stop()
        {
            if (!IsRecording) return;
            IsRecording = false;

            foreach (var id in _registeredHotkeyIds) _hotkeys.Unregister(id);
            _registeredHotkeyIds.Clear();

            _picker.Stop();
            _indicator.Hide();
            Logger.I($"Aufnahme gestoppt. {_stepCount} Schritt(e) aufgenommen.");
        }

        private void OnHotkey(RecordAction action)
        {
            switch (action)
            {
                case RecordAction.StopRecording:
                    Stop();
                    break;
                case RecordAction.ConfirmClick:
                    AddStepWithSelector(StepType.Click);
                    break;
                case RecordAction.ConfirmAssert:
                    AddStepWithSelector(StepType.AssertVisible);
                    break;
                case RecordAction.ConfirmSetText:
                    AddSetTextStep();
                    break;
                case RecordAction.AddWait:
                    Emit(new TestStep { Type = StepType.Wait, WaitMilliseconds = 1000 });
                    break;
            }
        }

        private void AddStepWithSelector(StepType type)
        {
            var selector = _picker.PollCurrentSelector();
            if (selector == null)
            {
                Logger.W("Kein Element unter dem Mauszeiger gefunden.");
                return;
            }
            Emit(new TestStep { Type = type, Target = selector });
        }

        private void AddSetTextStep()
        {
            var selector = _picker.PollCurrentSelector();
            if (selector == null)
            {
                Logger.W("Kein Element unter dem Mauszeiger gefunden.");
                return;
            }
            // Der Hotkey-Callback läuft synchron über die Window-Message-Loop des UI-Threads,
            // eine blockierende InputBox ist hier deshalb unproblematisch.
            var value = Microsoft.VisualBasic.Interaction.InputBox("Text eingeben:", "Set Text", "");
            Emit(new TestStep { Type = StepType.SetText, Target = selector, Value = value });
        }

        private void Emit(TestStep step)
        {
            _stepCount++;
            _indicator.SetStepCount(_stepCount);
            Logger.I($"{step.Type}-Schritt aufgenommen: {step}");
            StepRecorded?.Invoke(step);
        }

        public void Dispose()
        {
            Stop();
            _indicator.Close();
        }
    }
}
