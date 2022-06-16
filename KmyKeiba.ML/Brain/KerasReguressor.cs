using Keras;
using Keras.Callbacks;
using Keras.Models;
using Numpy;
using Python.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.ML.Brain
{
  class KerasReguressor : Base
  {
    private dynamic estimator;

    public KerasReguressor(BaseModel model, int initial_epoch = 0, int epochs = 32, int batch_size = 2)
    {
      dynamic module = Py.Import("keras.wrappers.scikit_learn");

      var args = new Dictionary<string, object>
      {
        ["epochs"] = epochs,
        ["batch_size"] = batch_size,
      };
      var dele = (Func<dynamic>)(() => model.ToPython());
      this.estimator = module.KerasRegressor(build_fn: dele.ToPython(), initial_epoch: initial_epoch, epochs: epochs, batch_size: batch_size);

      var field = typeof(KerasReguressor).GetField("PyInstance", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.SetField | System.Reflection.BindingFlags.GetField);
      field!.SetValue(this, this.estimator);
    }

    public History Fit(NDarray x, NDarray y)
    {
      var args = new Dictionary<string, object>
      {
        ["x"] = x,
        ["y"] = y,
      };
      var history = InvokeMethod("fit", args);
      return new History(history);
    }
  }

  class KerasObjectWrapper : Base
  {
    public KerasObjectWrapper(dynamic obj)
    {
      var field = typeof(KerasReguressor).GetField("PyInstance", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.SetField | System.Reflection.BindingFlags.GetField);
      field!.SetValue(this, obj);
    }
  }
}
