using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace CaptureTool.Pages
{
    public class EnumToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var parameterString = parameter as string;
            if (null == parameterString)
                return DependencyProperty.UnsetValue;

            if (!Enum.IsDefined(value.GetType(), value))
                return DependencyProperty.UnsetValue;

            var parameterValue = Enum.Parse(value.GetType(), parameterString);
            return parameterValue.Equals(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var parameterString = parameter as string;
            if (null == parameterString)
                return DependencyProperty.UnsetValue;

            if (true.Equals(value))
                return Enum.Parse(targetType, parameterString);
            else
                return DependencyProperty.UnsetValue;
        }
    }

    /// <summary>
    /// GUIのバインディングで、bool値を反転してくれるコンバーター
    /// </summary>
    public class CBoolNegativeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(value is bool && (bool)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(value is bool && (bool)value);
        }

    }

    [ContentProperty(nameof(Converters))]
    public class ValueConverterGroup : IValueConverter
    {
        public Collection<IValueConverter> Converters { get; } = new Collection<IValueConverter>();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var result = value;

            if (Converters == null) return result;

            foreach (var conv in Converters)
            {
                result = conv.Convert(result, targetType, parameter, culture);
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var result = value;

            if (Converters == null) return result;

            foreach (var conv in Converters.Reverse())
            {
                result = conv.ConvertBack(result, targetType, parameter, culture);
            }

            return result;
        }
    }
}
