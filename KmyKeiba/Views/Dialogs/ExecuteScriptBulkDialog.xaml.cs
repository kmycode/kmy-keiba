using KmyKeiba.Models.Script;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KmyKeiba.Views.Dialogs
{
  /// <summary>
  /// ExecuteScriptBulkDialog.xaml の相互作用ロジック
  /// </summary>
  public partial class ExecuteScriptBulkDialog : UserControl
  {
    public static readonly DependencyProperty ScriptBulkProperty
    = DependencyProperty.Register(
        nameof(ScriptBulk),
        typeof(ScriptBulkModel),
        typeof(ExecuteScriptBulkDialog),
        new PropertyMetadata(null));

    public ScriptBulkModel? ScriptBulk
    {
      get { return (ScriptBulkModel)GetValue(ScriptBulkProperty); }
      set { SetValue(ScriptBulkProperty, value); }
    }

    public ExecuteScriptBulkDialog()
    {
      InitializeComponent();
    }
  }
}
