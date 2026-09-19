using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using FlaUI.TestStudio.App.Native;
using FlaUI.TestStudio.Core;
using FlaUI.TestStudio.Core.Model;

namespace FlaUI.TestStudio.App.Overlay
{
    public partial class RecordingIndicatorWindow : Window
    {
        public RecordingIndicatorWindow()
        {
            InitializeComponent();
            Loaded += (_, _) =>
            {
                PositionTopRight();
                PreventFocusSteal();
                StartPulse();
            };
        }

        private void PositionTopRight()
        {
            var area = SystemParameters.WorkArea;
            Left = area.Right - ActualWidth - 16;
            Top = area.Top + 16;
        }

        // Verhindert, dass das Overlay beim Anzeigen der Zielanwendung den Fokus wegnimmt -
        // sonst würde jedes Aufblitzen des Overlays das gerade getestete Programm stören.
        private void PreventFocusSteal()
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            var exStyle = NativeMethods.GetWindowLongPtr(hwnd, NativeMethods.GWL_EXSTYLE).ToInt32();
            NativeMethods.SetWindowLongPtr(hwnd, NativeMethods.GWL_EXSTYLE,
                new IntPtr(exStyle | NativeMethods.WS_EX_NOACTIVATE | NativeMethods.WS_EX_TOOLWINDOW));
        }

        private void StartPulse()
        {
            var anim = new DoubleAnimation(1.0, 0.35, TimeSpan.FromMilliseconds(700))
            {
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };
            Dot.BeginAnimation(OpacityProperty, anim);
        }

        public void SetStepCount(int count) =>
            InfoText.Text = count == 0 ? "Noch kein Schritt aufgenommen" : $"{count} Schritt(e) aufgenommen";

        public void UpdateHoveredElement(Selector selector) =>
            InfoText.Text = $"Ziel: {selector}";
    }
}
