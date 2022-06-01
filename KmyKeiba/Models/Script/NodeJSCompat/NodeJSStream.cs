using Microsoft.ClearScript;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Script.NodeJSCompat
{
  [NoDefaultScriptAccess]
  public class NodeJSStream : IDisposable
  {
    private readonly List<StreamEvent> _eventHandlers = new();
    private readonly List<NodeJSStream> _pipes = new();
    private bool _isReading;

    public bool IsReadCompleted { get; private set; }

    protected int ChunkSize { get; set; } = 65536;

    protected bool IsRunning { get; set; }

    protected bool IsDisposed { get; private set; }

    [ScriptMember("on")]
    public void On(string eventName, Action<object?> chunk)
    {
      this._eventHandlers.Add(new StreamEvent(eventName, chunk));
      this.OnEventAdded(eventName, chunk);
    }

    [ScriptMember("once")]
    public void Once(string eventName, Action<object?> chunk)
    {
      this._eventHandlers.Add(new StreamEvent(eventName, chunk, isOnce: true));
      this.OnEventAdded(eventName, chunk);
    }

    protected virtual void OnEventAdded(string eventName, Action<object?> chunk)
    {
    }

    [ScriptMember("pipe")]
    public void Pipe(NodeJSStream others)
    {
      this._pipes.Add(others);
      this.OnPipeAdded();
    }

    protected virtual void OnPipeAdded()
    {
    }

    [ScriptMember("write")]
    public virtual void Write(object chunk)
    {
    }

    [ScriptMember("end")]
    public void End()
    {
      this.Dispose();
    }

    public virtual void Dispose()
    {
      if (this.IsDisposed)
      {
        return;
      }
      this.IsDisposed = true;

      this.InvokeEvent("end");
      this._eventHandlers.Clear();
      this._pipes.Clear();
    }

    protected void InvokeEvent(string eventName, object? chunk = null)
    {
      if (eventName == "data" && chunk != null)
      {
        foreach (var pipe in this._pipes)
        {
          pipe.Write(chunk);
        }
      }

      var onces = new List<StreamEvent>();
      foreach (var ev in this._eventHandlers.Where(eh => eh.EventName == eventName))
      {
        ev.Chunk(chunk);
        if (ev.IsOnce)
        {
          onces.Add(ev);
        }
      }

      foreach (var ev in onces)
      {
        this._eventHandlers.Remove(ev);
      }

      if (eventName == "end")
      {
        foreach (var pipe in this._pipes)
        {
          pipe.End();
        }
        this.End();
      }
    }

    protected void BeginRead(Stream stream)
    {
      if (this._isReading)
      {
        return;
      }
      this._isReading = true;

      Task.Run(async () =>
      {
        var size = stream.Length;
        var buffer = new byte[this.ChunkSize];
        var asize = size;
        while (asize > 0)
        {
          if (!this.IsRunning)
          {
            await Task.Delay(10);
            continue;
          }
          if (this.IsDisposed)
          {
            break;
          }

          asize = await stream.ReadAsync(buffer.AsMemory((int)stream.Position, this.ChunkSize));
          if (asize > 0)
          {
            var arr = buffer;
            if (asize != buffer.Length)
            {
              var newBuf = new byte[asize];
              Array.Copy(buffer, newBuf, asize);
              arr = newBuf;
            }

            this.InvokeEvent("data", arr);
          }
        }

        this.InvokeEvent("end");
        this.IsReadCompleted = true;
      });
    }

    private class StreamEvent
    {
      public string EventName { get; }

      public Action<object?> Chunk { get; }

      public bool IsOnce { get; }

      public StreamEvent(string eventName, Action<object?> chunk, bool isOnce = false)
      {
        this.EventName = eventName;
        this.Chunk = chunk;
        this.IsOnce = true;
      }
    }
  }

  [NoDefaultScriptAccess]
  public class NodeJSReadFileStream : NodeJSStream
  {
    private readonly FileStream _stream;

    public NodeJSReadFileStream(string fileName)
    {
      var stream = new FileStream(fileName, FileMode.Open);
      this._stream = stream;

      this.BeginRead(this._stream);
    }

    protected override void OnEventAdded(string eventName, Action<object?> chunk)
    {
      if (eventName == "data")
      {
        this.IsRunning = true;
      }
      if (eventName == "end")
      {
        if (this.IsReadCompleted)
        {
          this.InvokeEvent(eventName);
        }
      }
    }

    protected override void OnPipeAdded()
    {
      this.IsRunning = true;
    }

    public override void Dispose()
    {
      this.IsRunning = false;
      base.Dispose();
      this._stream.Dispose();
    }
  }

  public class NodeJSWriteFileStream : NodeJSStream
  {
    private readonly FileStream _stream;

    public NodeJSWriteFileStream(string fileName)
    {
      this._stream = new FileStream(fileName, FileMode.OpenOrCreate);
    }

    public override void Write(object chunk)
    {
      if (chunk is byte[] bin)
      {
        this._stream.Write(bin);
      }
    }

    public override void Dispose()
    {
      base.Dispose();
      this._stream.Dispose();
    }
  }
}
