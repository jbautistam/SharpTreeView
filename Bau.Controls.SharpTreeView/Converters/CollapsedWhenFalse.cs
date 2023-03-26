using System;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Data;
using System.Globalization;

namespace Bau.Controls.SharpTreeView.Converters
{
    public class CollapsedWhenFalse : MarkupExtension, IValueConverter
    {
        public static CollapsedWhenFalse Instance = new CollapsedWhenFalse();

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Instance;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
