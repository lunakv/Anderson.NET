using System;
using System.Globalization;
using System.Windows.Data;

namespace Anderson.Structures
{
    /// <summary>
    /// Converts DateTime info on AndersonMessage to string for display
    /// </summary>
    [ValueConversion(typeof(DateTime), typeof(string))]
    class DateTimeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
                                              // HH:mm
            return ((DateTime)value).ToString("t", CultureInfo.InvariantCulture) + "\t";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
