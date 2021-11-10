using System.Linq;

namespace UpgradeWorld {

  public abstract class BaseOperation {
    public string Operation = "BaseOperation";
    protected int Attempts = 0;
    protected Terminal Context = null;
    protected int Failed = 0;
    protected BaseOperation(Terminal context) {
      Context = context;
    }
    public bool Execute() {
      Attempts++;
      var ret = OnExecute();
      if (ret) OnEnd();
      return ret;
    }
    protected abstract bool OnExecute();
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
    protected virtual void OnEnd() {
    }
  }

}
