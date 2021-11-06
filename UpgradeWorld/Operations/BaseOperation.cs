namespace UpgradeWorld {

  public abstract class BaseOperation {
    public string Operation = "BaseOperation";
    protected int Attempts = 0;
    protected Terminal Context = null;
    protected int Failed = 0;
    protected BaseOperation(Terminal context) {
      Context = context;
    }
    public virtual bool Execute() {
      Attempts++;
      return true;
    }
    public void Print(string value) {
      if (Context) Context.AddString(value);
    }
    protected virtual void OnEnd() {
    }
  }

}
