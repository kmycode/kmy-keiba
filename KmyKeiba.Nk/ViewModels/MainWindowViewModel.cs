using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Prism.Mvvm;
using System;
using System.Reactive.Disposables;

namespace KmyKeiba.Nk.ViewModels
{
  public class MainWindowViewModel : BindableBase, IDisposable
  {
    private readonly CompositeDisposable disposables = new CompositeDisposable();
    private string _title = "Prism Application";
    public string Title
    {
      get { return _title; }
      set { SetProperty(ref _title, value); }
    }

    public MainWindowViewModel()
    {
      IWebDriver driver = new ChromeDriver();
      this.disposables.Add(this.disposables);
      driver.Navigate().GoToUrl("https://nar.netkeiba.com/race/shutuba.html?race_id=202136050501&rf=race_list");

      var name = driver.FindElement(By.ClassName("RaceName")).Text.Trim();
      var data1 = driver.FindElement(By.ClassName("RaceData01")).Text.Trim().Replace(" ", "").Replace("\t", "");
      var data2 = driver.FindElement(By.ClassName("RaceData02")).FindElements(By.TagName("span"));
    }

    public void Dispose()
    {
      this.disposables.Dispose();
    }
  }
}
