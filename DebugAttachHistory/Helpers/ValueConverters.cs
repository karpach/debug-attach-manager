using System;
using System.Windows;
using System.Windows.Data;

namespace Karpach.DebugAttachManager.Helpers
{
    internal class ThicknessToWidthConverter : IValueConverter
    {
        #region IValueConverter Members

        public Object Convert(Object value, Type targetType, Object parameter, System.Globalization.CultureInfo culture)
        {
            Thickness? thickness = (Thickness?)value;
            return thickness?.Left;
        }

        public Object ConvertBack(Object value, Type targetType, Object parameter, System.Globalization.CultureInfo culture)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }

    internal class ScaledValueConverter : IValueConverter
    {
        #region IValueConverter Members

        public Object Convert(Object value, Type targetType, Object parameter, System.Globalization.CultureInfo culture)
        {
            Double scalingFactor = 0;
            if (parameter != null)
            {
                Double.TryParse((String)(parameter), out scalingFactor);
            }

            if (Math.Abs(scalingFactor) < 0)
            {
                return Double.NaN;
            }

            return (double?)value * scalingFactor;
        }

        public Object ConvertBack(Object value, Type targetType, Object parameter, System.Globalization.CultureInfo culture)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }
}