using KmyKeiba.Shared;
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
      this.CheckPath(this.GetFullPath(path));
      return Task.FromResult(new NodeJSFileHandle(this.GetFullPath(path)));
    }

    [ScriptMember("mkdir")]
    public Task Mkdir(string path)
    {
      this.CheckPath(path);
      Directory.CreateDirectory(this.GetFullPath(path));
      return Task.CompletedTask;
    }

    private string GetFullPath(string path)
    {
      return Path.GetFullPath(path, Constrants.ScriptDir);
    }

    private void CheckPath(string path)
    {
      var acceptedDir = Constrants.ScriptDir;
      var fullPath = this.GetFullPath(path);
      if (!fullPath.StartsWith(acceptedDir))
      {
        throw new ArgumentException($"スクリプトからは、script以下のディレクトリにしかアクセスできません。 scriptディレクトリ: {acceptedDir}、要求したディレクトリ: {fullPath}");
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
