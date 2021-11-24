namespace UpgradeWorld {
  ///<summary>Base class for all operations that need execution. Provides the execution logic.</summary>
  public abstract class ExecutedOperation : BaseOperation {
    protected int Attempts = 0;
    protected int Failed = 0;
    public bool AutoStart = false;
    protected ExecutedOperation(Terminal context, bool autoStart) : base(context) {
      AutoStart = autoStart;
    }
    public bool Execute() {
      Attempts++;
      var ret = OnExecute();
      if (ret) OnEnd();
      return ret;
    }
    protected abstract bool OnExecute();
    public void Init() {
      var output = OnInit();
      if (output != "")
        Print(output);
      Print("Use start to begin execution or stop to cancel.");
    }
    protected abstract string OnInit();
    protected void InitPrint(string text) {
      Print(text);
      Print("Use start to begin execution or stop to cancel.");
    }
    protected virtual void OnEnd() {
    }
  }

}
