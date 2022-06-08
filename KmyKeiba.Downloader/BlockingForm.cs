using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Downloader
{
  internal class BlockingForm : Form
  {
    public BlockingForm(IntPtr jraVanDialogHandle) : this()
    {
      this.Activated += async (sender, e) =>
      {
        while (Program.IsWindowVisible(jraVanDialogHandle))
        {
          await Task.Delay(1000);
        };
        this.Close();
      };
    }

    public BlockingForm()
    {
      this.Width = 500;
      this.Height = 100;
      this.FormBorderStyle = FormBorderStyle.FixedSingle;
      this.MinimizeBox = false;
      this.MaximizeBox = false;
      this.ControlBox = false;
      this.TopMost = true;
      this.CenterToScreen();

      this.Text = "KMY競馬のタスクはブロックされています";
      this.BackColor = Color.FromArgb(255, 200, 200, 200);

      this.Controls.Add(new Label
      {
        Text = "JRA-VANからのお知らせが表示されています。\nそれを閉じるまで処理は続行されません。",
        AutoSize = true,
        TextAlign = ContentAlignment.MiddleCenter,
        Top = 20,
        Left = 70,
        Font = new Font("Meiryo UI", 14),
      });
    }
  }
}
