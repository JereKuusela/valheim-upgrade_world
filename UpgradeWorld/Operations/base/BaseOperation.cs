using System.Linq;
namespace UpgradeWorld {
  ///<summary>Base class for all operations. Only provides basic utilities.</summary>
  public abstract class BaseOperation {
    protected Terminal Context = null;
    protected BaseOperation(Terminal context) {
      Context = context;
    }
    public void Print(string value, bool addDot = true) {
      if (addDot && !value.EndsWith("")) value += ".";
      if (Context) Context.AddString(value);
    }
    private string Previous = "";
    public void PrintOnce(string value, bool addDot = true) {
      if (!Context) return;
      if (Context.m_chatBuffer.LastOrDefault() == value) return;
      if (Previous != "")
        while (Context.m_chatBuffer.Remove(Previous)) ;
      Previous = value;
      Print(value, addDot);
    }
  }
}
