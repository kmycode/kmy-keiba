using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace KmyKeiba.Converters
{
  class BitmapConverter : IValueConverter
  {
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      try
      {
        if (value is byte[] buff)
        {
          Bitmap bmp;
          using (var stream = new MemoryStream(buff))
          {
            bmp = new Bitmap(stream);
          }

          return Imaging.CreateBitmapSourceFromHBitmap(bmp.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }

        return null;
      }
      catch
      {
        return null;
      }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
