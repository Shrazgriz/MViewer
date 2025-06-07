using AnyCAD.Foundation;
using MVUnity;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace MViewer
{
    internal static class ConvertVector3
    {
        public static V3 ToV3(Vector3 value)
        {
            return new V3(value.x, value.y, value.z);
        }
    }
    internal static class ConvertV3
    {
        public static Vector3 ToVector3(V3 value)
        {
            return new Vector3((float)value.X, (float)value.Y, (float)value.Z);
        }

        public static Vector3 ToColor(V3 value)
        {
            return new Vector3((float)Math.Abs(value.X), (float)Math.Abs(value.Y), (float)Math.Abs(value.Z));
        }

        public static GPnt ToGPnt(V3 value)
        {
            return new GPnt((float)value.X, (float)value.Y, (float)value.Z);
        }
    }

    public class V3ToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((V3)value).ToString("F2");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string)
            {
                V3 v = new V3(value as string, ',');
                return v;
            }
            return null;
        }
    }

    /// <summary>
    /// Color与实色画笔转换
    /// </summary>
    public class ColorToSolidBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Color c)
            {
                return new SolidColorBrush(c);
            }
            else
            {
                return new SolidColorBrush(Colors.Transparent);
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is SolidColorBrush b)
            {
                return b.Color;
            }
            return Colors.Transparent;
        }
    }
    /// <summary>
    /// V3与实色画笔转换
    /// </summary>
    public class V3ToSolidBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            V3 v = (value as V3).Normalized();
            byte r = (byte)(int)(Math.Abs(v.X) * 255f);
            byte g = (byte)(int)(Math.Abs(v.Y) * 255f);
            byte b = (byte)(int)(Math.Abs(v.Z) * 255f);
            Color c = Color.FromRgb(r, g, b);
            return new SolidColorBrush(c);
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is SolidColorBrush c)
            {
                double x = c.Color.R;
                double y = c.Color.G;
                double z = c.Color.B;
                return new V3(x, y, z).Normalized();
            }
            return V3.Zero;
        }
    }

    public class PointInfo
    {
        private V3 coord;
        public string Value
        {
            get { return coord.ToString("F2"); }
            set { coord = new V3(value); }
        }
        public PointInfo(V3 Point)
        {
            coord = new V3(Point);
        }
    }
}
