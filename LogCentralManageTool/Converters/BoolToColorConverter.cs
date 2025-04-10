using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace LogCentralManageTool.Converters;

/// <summary>
/// bool 값을 받아 true일 때와 false일 때 각각 다른 Brush를 반환하는 Converter입니다.
/// </summary>
public class BoolToColorConverter : IValueConverter
{
    /// <summary>
    /// true일 때의 Brush (예: LightGreen)
    /// </summary>
    public Brush TrueBrush { get; set; } = Brushes.LightGreen;

    /// <summary>
    /// false일 때의 Brush (예: LightGray)
    /// </summary>
    public Brush FalseBrush { get; set; } = Brushes.LightGray;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool flag)
            return flag ? TrueBrush : FalseBrush;
        return FalseBrush;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
