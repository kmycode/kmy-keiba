using Microsoft.ClearScript;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Script.NodeJSCompat
{
  [NoDefaultScriptAccess]
  public class NodeJSFileSystem
  {
    [ScriptMember("open")]
    public Task<NodeJSFileHandle> Open(string path)
    {
      this.CheckPath(path);
      return Task.FromResult(new NodeJSFileHandle(path));
    }

    [ScriptMember("mkdir")]
    public Task Mkdir(string path)
    {
      this.CheckPath(path);
      Directory.CreateDirectory(path);
      return Task.CompletedTask;
    }

    private void CheckPath(string path)
    {
      var dirName = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location ?? Directory.GetCurrentDirectory()) ?? string.Empty;
      var dirName2 = Path.GetFullPath(path, Directory.GetCurrentDirectory());
      if (!dirName2.StartsWith(dirName))
      {
        throw new ArgumentException($"スクリプトからは、KMY競馬のexe直下のディレクトリにしかアクセスできません。 KMY競馬exeのあるディレクトリ: {dirName}、要求したディレクトリ: {dirName2}");
      }
    }
  }

  [NoDefaultScriptAccess]
  public class NodeJSFileHandle
  {
    private readonly string _path;

    public NodeJSFileHandle(string path)
    {
      this._path = path;
    }

    [ScriptMember("close")]
    public void Close()
    {
    }

    [ScriptMember("createReadStream")]
    public NodeJSReadFileStream CreateReadStream()
    {
      return new NodeJSReadFileStream(this._path);
    }

    [ScriptMember("createWriteStream")]
    public NodeJSWriteFileStream CreateWriteStream()
    {
      return new NodeJSWriteFileStream(this._path);
    }

    [ScriptMember("readFile")]
    public async Task<string> ReadFileAsync()
    {
      return await File.ReadAllTextAsync(this._path);
    }

    [ScriptMember("readFileSync")]
    public string ReadFile()
    {
      return File.ReadAllText(this._path);
    }

    [ScriptMember("writeFile")]
    public async Task WriteFileAsync(string text)
    {
      await File.WriteAllTextAsync(this._path, text);
    }

    [ScriptMember("writeFileSync")]
    public void WriteFile(string text)
    {
      File.WriteAllText(this._path, text);
    }
  }
}
