using System.Collections.Generic;
using System.Linq;
namespace UpgradeWorld {
  ///<summary>Base class for all operations. Only provides basic utilities.</summary>
  public abstract class BaseOperation {
    protected Terminal Context = null;
    protected BaseOperation(Terminal context) {
      Context = context;
    }
    protected void Print(string value, bool addDot = true) {
      if (addDot && !value.EndsWith("")) value += ".";
      if (Context) Context.AddString(value);
    }
    protected void Log(IEnumerable<string> values) {
      foreach (var value in values) Print(value, false);
      ZLog.Log(string.Join("\n", values));
    }
    private string Previous = "";
    protected void PrintOnce(string value, bool addDot = true) {
      if (!Context) return;
      if (Context.m_chatBuffer.LastOrDefault() == value) return;
      if (Previous != "")
        while (Context.m_chatBuffer.Remove(Previous)) ;
      Previous = value;
      Print(value, addDot);
    }
  }
}
