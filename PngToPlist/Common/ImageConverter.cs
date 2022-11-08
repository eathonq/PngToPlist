using System;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace PngToPlist.Common
{
    public class ImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string imgpath = (string)value;
            return string.IsNullOrEmpty(imgpath)
                ? new BitmapImage(new Uri("../Resources/Images/Default.png", UriKind.Relative))
                : new BitmapImage(new Uri(imgpath, UriKind.Absolute));

            //string imgPath = "pack://application:,,,/Resources/Image/toolbargraphics/15x15/check-15.png";
            // imgInfo.Source = new BitmapImage(new Uri(imgPath, UriKind.RelativeOrAbsolute));
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
