using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Merge.Common;

namespace Merge.Desktop
{
    /// <summary>
    /// State to Color Brush converter.
    /// </summary>
    public class StateToColorConverter : IValueConverter
    {
        /// <summary>
        /// Convert Change State to Color Brush.
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var color = Colors.Transparent;
            if (value is ChangeState)
            {
                switch ((ChangeState)value)
                {
                    case ChangeState.Conflict:
                        color = Colors.LightCoral;
                        break;
                    case ChangeState.Modified:
                        color = Color.FromRgb(255, 255, 178);
                        break;
                    case ChangeState.Add:
                        color = Colors.LightGreen;
                        break;
                    case ChangeState.Delete:
                        color = Color.FromRgb(230, 230, 230);
                        break;
                }
            }

            return new SolidColorBrush(color);
        }

        /// <summary>
        /// Convert back.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
