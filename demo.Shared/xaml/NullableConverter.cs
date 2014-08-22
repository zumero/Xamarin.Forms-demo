using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace demo.xaml
{
    public class NullableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            if (value == null && targetType == typeof(bool))
                return false;
            if (value == null && targetType == typeof(DateTime))
                return DateTime.Now;
            if (value == null)
                return null;
            if (value.GetType() == targetType)
                return value;
			if (value.GetType() == typeof(byte[]))
                return "<blob>";
			if (Nullable.GetUnderlyingType(value.GetType()) == typeof(byte[]))
				return "<blob>";
            if (value != null && targetType == typeof(string))
                return value.ToString();
            else
                return null;
        }

        public object ConvertBack(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            if (value == null && targetType == typeof(bool))
                return false;
            if (value == null)
                return null;

            Type t = Nullable.GetUnderlyingType(targetType);
            if (value.GetType() == targetType
                || value.GetType() == t)
                return value;

            if (t == null)
                t = targetType;

            string valString = (string)value;
            if (valString == String.Empty)
                return null;

            try
            {
                if (t == typeof(int))
                {
                    return int.Parse(valString);
                }
                else if (t == typeof(long))
                {
                    return long.Parse(valString);
                }
                else if (t == typeof(byte))
                {
                    return byte.Parse(valString);
                }
                else if (t == typeof(short))
                {
                    return short.Parse(valString);
                }
                else if (t == typeof(float))
                {
                    return float.Parse(valString);
                }
                else if (t == typeof(double))
                {
                    return double.Parse(valString);
                }
                else if (t == typeof(decimal))
                {
                    return decimal.Parse(valString);
                }
                else if (t == typeof(DateTime))
                {
                    return DateTime.Parse(valString);
                }
                else if (t == typeof(Guid))
                {
                    return Guid.Parse(valString);
                }
            }
            catch (Exception)
            {
                return null;
            }
            return value;
        }
    }
}