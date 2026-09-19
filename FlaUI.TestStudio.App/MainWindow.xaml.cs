using FlaUI.TestStudio.Automation;
using FlaUI.TestStudio.App.Native;
using FlaUI.TestStudio.App.Services;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using FlaUI.TestStudio.Core.Model;
using FlaUI.TestStudio.Core.Interface;
using FlaUI.TestStudio.Core.Logging;


namespace FlaUI.TestStudio.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// Enthält nur noch UI-Orchestrierung. Testausführung liegt in TestStepExecutor,
    /// Element-Picking/Recording in ElementPickerService, Logging in Logger.
    /// </summary>
    public partial class MainWindow : Window
    {
        private TestCase _test = new();
        private readonly ElementInspector _inspector = new();
        private readonly ElementPickerService _picker;
        private readonly GlobalHotkeyService _hotkeys;
        private readonly RecordingCoordinator _recorder;
        private IAutomationService? _automation;
        private int? _singleSelectHotkeyId;

        public MainWindow()
        {
            InitializeComponent();
            Refresh();

            _picker = new ElementPickerService(_inspector);
            _picker.ElementHovered += OnElementHovered;

            _hotkeys = new GlobalHotkeyService(this);
            _recorder = new RecordingCoordinator(_hotkeys, _picker);
            _recorder.StepRecorded += step =>
            {
                Dispatcher.Invoke(() =>
                {
                    _test.Steps.Add(step);
                    Refresh();
                });
            };

            Logger.OnLog += entry =>
            {
                Dispatcher.Invoke(() =>
                {
                    LogBox.AppendText(entry + Environment.NewLine);
                    LogBox.ScrollToEnd();
                });
            };
        }

        private void Refresh()
        {
            TestNameBox.Text = _test.Name;
            StepsList.ItemsSource = null;
            StepsList.ItemsSource = _test.Steps;
        }

        // ---------- Datei-Operationen ----------

        private void NewTest_Click(object sender, RoutedEventArgs e)
        {
            _test = new TestCase();
            Refresh();
            Logger.I("Neuer Test erstellt.");
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            _test.Name = TestNameBox.Text;
            var dlg = new SaveFileDialog { Filter = "FlaUI Test (*.json)|*.json", FileName = _test.Name + ".json" };
            if (dlg.ShowDialog() == true)
            {
                File.WriteAllText(dlg.FileName, JsonSerializer.Serialize(_test, new JsonSerializerOptions { WriteIndented = true }));
                Logger.I($"Test gespeichert: {dlg.FileName}");
            }
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog { Filter = "FlaUI Test (*.json)|*.json" };
            if (dlg.ShowDialog() == true)
            {
                _test = JsonSerializer.Deserialize<TestCase>(File.ReadAllText(dlg.FileName)) ?? new TestCase();
                Refresh();
                Logger.I($"Test geladen: {dlg.FileName}");
            }
        }

        // ---------- Steps hinzufügen ----------

        private void AddClick_Click(object sender, RoutedEventArgs e) => AddStep(StepType.Click, needsSelector: true);
        private void AddAssert_Click(object sender, RoutedEventArgs e) => AddStep(StepType.AssertVisible, needsSelector: true);
        private void AddWait_Click(object sender, RoutedEventArgs e) => AddStep(StepType.Wait);

        private void AddText_Click(object sender, RoutedEventArgs e)
        {
            var selector = _picker.LastSelector ?? PromptSelector();
            if (selector == null) return;
            var value = Microsoft.VisualBasic.Interaction.InputBox("Text eingeben:", "Set Text", "");
            _test.Steps.Add(new TestStep { Type = StepType.SetText, Target = selector, Value = value });
            Refresh();
        }

        private void AddStep(StepType type, bool needsSelector = false)
        {
            Selector? selector = null;
            if (needsSelector)
            {
                selector = _picker.LastSelector ?? PromptSelector();
                if (selector == null) return;
            }
            _test.Steps.Add(new TestStep { Type = type, Target = selector, WaitMilliseconds = type == StepType.Wait ? 1000 : 0 });
            Refresh();
        }

        private Selector? PromptSelector()
        {
            if (_picker.LastSelector != null) return _picker.LastSelector;
            MessageBox.Show("Bitte zuerst ein Element mit 'Element auswählen' wählen.");
            return null;
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (StepsList.SelectedIndex >= 0)
            {
                _test.Steps.RemoveAt(StepsList.SelectedIndex);
                Refresh();
            }
        }

        private void MoveUp_Click(object sender, RoutedEventArgs e)
        {
            var i = StepsList.SelectedIndex;
            if (i > 0) { (_test.Steps[i - 1], _test.Steps[i]) = (_test.Steps[i], _test.Steps[i - 1]); Refresh(); StepsList.SelectedIndex = i - 1; }
        }

        private void MoveDown_Click(object sender, RoutedEventArgs e)
        {
            var i = StepsList.SelectedIndex;
            if (i >= 0 && i < _test.Steps.Count - 1) { (_test.Steps[i + 1], _test.Steps[i]) = (_test.Steps[i], _test.Steps[i + 1]); Refresh(); StepsList.SelectedIndex = i + 1; }
        }

        // ---------- Element Picking / Recording ----------

        private void PickElement_Click(object sender, RoutedEventArgs e)
        {
            // Einmalige Auswahl: F8 ist jetzt ein globaler Hotkey, funktioniert also auch,
            // während die Zielanwendung (nicht dieses Fenster) den Fokus hat.
            StatusText.Text = "Bewege die Maus über das Ziel-Element und drücke F8.";
            SetStatusDot(Brushes.Orange);
            _picker.Start(PickerMode.SingleSelect);
            _singleSelectHotkeyId = _hotkeys.Register(NativeMethods.ModifierKeys.None, Key.F8, ConfirmSingleSelect);
        }

        private void ConfirmSingleSelect()
        {
            var selector = _picker.PollCurrentSelector();

            if (_singleSelectHotkeyId is int id)
            {
                _hotkeys.Unregister(id);
                _singleSelectHotkeyId = null;
            }
            _picker.Stop();

            Dispatcher.Invoke(() =>
            {
                SetStatusDot(Brushes.Gray);
                if (selector != null)
                {
                    StatusText.Text = "Element ausgewählt.";
                    Logger.I($"Element: {selector}");
                }
                else
                {
                    StatusText.Text = "Kein Element unter dem Mauszeiger gefunden.";
                }
            });
        }

        private void Record_Click(object sender, RoutedEventArgs e)
        {
            StatusText.Text = "Aufnahme läuft - siehe Overlay oben rechts für Hotkeys.";
            SetStatusDot((Brush)FindResource("Brush.Record"));
            _recorder.Start();
        }

        private void StopRecord_Click(object sender, RoutedEventArgs e)
        {
            _recorder.Stop();
            SetStatusDot(Brushes.Gray);
            StatusText.Text = "Aufnahme gestoppt.";
        }

        private void SetStatusDot(Brush brush) => StatusDot.Fill = brush;

        private void OnElementHovered(Selector selector)
        {
            AutomationIdBox.Text = selector.AutomationId ?? "";
            NameBox.Text = selector.Name ?? "";
            ControlTypeBox.Text = selector.ControlType ?? "";
        }

        // ---------- Verbindung zur Zielanwendung ----------

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            if (ApplicationComboBox.SelectedItem is not ApplicationItem selected)
            {
                MessageBox.Show("Bitte zuerst eine Anwendung auswählen.", "Keine Anwendung ausgewählt",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _automation?.Dispose();
                _automation = new FlaUIAutomationService();
                _automation.AttachToProcess(selected.ProcessId);

                StatusText.Text = $"Verbunden: {selected.DisplayName}";
                Logger.I($"Mit Anwendung verbunden: {selected.DisplayName} (PID {selected.ProcessId})");
            }
            catch (Exception ex)
            {
                StatusText.Text = "Verbindung fehlgeschlagen";
                Logger.E(ex, "Connect Fehler");
                MessageBox.Show(ex.Message, "Verbindung fehlgeschlagen", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshApplications_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var applications = new List<ApplicationItem>();

                foreach (var process in Process.GetProcesses())
                {
                    try
                    {
                        if (process.MainWindowHandle == IntPtr.Zero) continue;
                        if (string.IsNullOrWhiteSpace(process.MainWindowTitle)) continue;

                        applications.Add(new ApplicationItem
                        {
                            ProcessId = process.Id,
                            ProcessName = process.ProcessName,
                            DisplayName = process.MainWindowTitle
                        });
                    }
                    catch
                    {
                        // Prozess kann inzwischen beendet worden sein
                    }
                }

                ApplicationComboBox.ItemsSource = applications.OrderBy(x => x.DisplayName).ToList();
                Logger.I($"{applications.Count} Anwendungen gefunden.");
            }
            catch (Exception ex)
            {
                Logger.E(ex, "Fehler beim Auflisten der Anwendungen");
            }
        }

        // ---------- Testlauf ----------

        private async void Run_Click(object sender, RoutedEventArgs e)
        {
            _test.Name = TestNameBox.Text;
            if (_test.Steps.Count == 0) { MessageBox.Show("Der Test enthält keine Schritte."); return; }
            if (_automation == null) { MessageBox.Show("Bitte zuerst eine Anwendung verbinden."); return; }

            var executor = new TestStepExecutor(_automation);
            var result = await executor.RunAsync(_test, _automation);

            if (result.Success)
            {
                StatusText.Text = "TEST PASSED";
            }
            else
            {
                StatusText.Text = "TEST FAILED";
                MessageBox.Show(result.FailureMessage, "Test fehlgeschlagen", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _recorder.Dispose();
            _hotkeys.Dispose();
            _picker.Dispose();
            _automation?.Dispose();
            _inspector.Dispose();
            base.OnClosed(e);
        }
    }
}
