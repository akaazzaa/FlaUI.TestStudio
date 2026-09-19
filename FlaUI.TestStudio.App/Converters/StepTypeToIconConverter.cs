using System.Globalization;
using System.Windows.Data;
using FlaUI.TestStudio.Core.Model;

namespace FlaUI.TestStudio.App.Converters
{
    /// <summary>
    /// Bildet StepType auf ein kleines Glyph ab, damit die Schritteliste
    /// auch bei vielen Einträgen scannbar bleibt. Neuer StepType -> hier
    /// einfach eine Zeile ergänzen.
    /// </summary>
    public sealed class StepTypeToIconConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object parameter, CultureInfo culture) => value switch
        {
            StepType.Click => "🖱",
            StepType.SetText => "⌨",
            StepType.Wait => "⏱",
            StepType.AssertVisible => "✓",
            StepType.LaunchApplication => "🚀",
            _ => "•"
        };

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}
