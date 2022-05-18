using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Script
{
  public class BrowserController
  {
    public string? LastUrl { get; private set; }

    public event EventHandler<NavigationRequestedEventArgs>? NavigationRequested;
    public event EventHandler<UpdateHtmlRequestedEventArgs>? UpdateHtmlRequested;
    public event EventHandler? UpdateRequested;

    public void Navigate(string url)
    {
      this.LastUrl = url;
      this.NavigationRequested?.Invoke(this, new NavigationRequestedEventArgs(url));
    }

    public void UpdateHtml(string html, string? url = null)
    {
      this.UpdateHtmlRequested?.Invoke(this, new UpdateHtmlRequestedEventArgs(html, url ?? this.LastUrl ?? string.Empty));
    }

    public void Update()
    {
      this.UpdateRequested?.Invoke(this, EventArgs.Empty);
    }
  }

  public class NavigationRequestedEventArgs : EventArgs
  {
    public string Url { get; }

    public NavigationRequestedEventArgs(string url)
    {
      this.Url = url;
    }
  }

  public class UpdateHtmlRequestedEventArgs : EventArgs
  {
    public string Html { get; }

    public string Url { get; }

    public UpdateHtmlRequestedEventArgs(string html, string url)
    {
      this.Html = html;
      this.Url = url;
    }
  }
}
