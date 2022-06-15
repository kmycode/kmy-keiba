using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;

namespace KmyKeiba.Views.Parts
{
  public class ImePopup : Popup
  {
    // https://social.msdn.microsoft.com/Forums/en-US/b2428b85-adc9-4a1e-a588-8dbb3b9aac06/ime-cant-be-turned-on-for-textboxes-inside-a-popup?forum=wpf

    [DllImport("user32.dll")]
    static extern IntPtr SetActiveWindow(IntPtr hWnd);

    static ImePopup()
    {
      EventManager.RegisterClassHandler(
          typeof(ImePopup),
          PreviewGotKeyboardFocusEvent,
          new KeyboardFocusChangedEventHandler(OnPreviewGotKeyboardFocus),
          true);
    }

    private static void OnPreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
      var textBox = e.NewFocus as TextBoxBase;
      if (textBox != null)
      {
        var hwndSource = PresentationSource.FromVisual(textBox) as HwndSource;
        if (hwndSource != null)
        {
          SetActiveWindow(hwndSource.Handle);
        }
      }
    }
  }
}
