using KmyKeiba.Models.Race.Memo;
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

namespace KmyKeiba.Views.Parts
{
    /// <summary>
    /// ExpansionMemoItem.xaml の相互作用ロジック
    /// </summary>
    public partial class ExpansionMemoItem : UserControl
    {
    public static readonly DependencyProperty MemoItemProperty
= DependencyProperty.Register(
nameof(MemoItem),
typeof(RaceMemoItem),
typeof(ExpansionMemoItem),
new PropertyMetadata(null));

    public RaceMemoItem? MemoItem
    {
      get { return (RaceMemoItem)GetValue(MemoItemProperty); }
      set { SetValue(MemoItemProperty, value); }
    }

    public static readonly DependencyProperty CanEditConfigProperty
= DependencyProperty.Register(
nameof(CanEditConfig),
typeof(bool),
typeof(ExpansionMemoItem),
new PropertyMetadata(true));

    public bool CanEditConfig
    {
      get { return (bool)GetValue(CanEditConfigProperty); }
      set { SetValue(CanEditConfigProperty, value); }
    }

    public static readonly DependencyProperty CanSaveProperty
= DependencyProperty.Register(
nameof(CanSave),
typeof(bool),
typeof(ExpansionMemoItem),
new PropertyMetadata(true));

    public bool CanSave
    {
      get { return (bool)GetValue(CanSaveProperty); }
      set { SetValue(CanSaveProperty, value); }
    }

    public static readonly DependencyProperty EditMemoConfigCommandProperty
= DependencyProperty.Register(
nameof(EditMemoConfigCommand),
typeof(ICommand),
typeof(ExpansionMemoItem),
new PropertyMetadata(null));

    public ICommand? EditMemoConfigCommand
    {
      get { return (ICommand)GetValue(EditMemoConfigCommandProperty); }
      set { SetValue(EditMemoConfigCommandProperty, value); }
    }

    public ExpansionMemoItem()
        {
            InitializeComponent();
        }
    }
}
