using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Converters
{
    public class Base64ToImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string base64 && !string.IsNullOrEmpty(base64))
            {
                byte[] imageBytes = System.Convert.FromBase64String(base64.Replace("data:image/png;base64,", ""));
                return ImageSource.FromStream(() => new MemoryStream(imageBytes));
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
