using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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

          try
          {
            return Imaging.CreateBitmapSourceFromHBitmap(bmp.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
          }
          finally
          {
            DeleteObject(bmp.GetHbitmap());
          }
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

    [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DeleteObject([In] IntPtr hObject);
  }
}
