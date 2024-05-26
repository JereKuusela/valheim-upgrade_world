using System;
namespace UpgradeWorld;
///<summary>Base class for all operations that need execution. Provides the execution logic.</summary>
public abstract class ExecutedOperation(Terminal context, bool autoStart, bool pin = false) : BaseOperation(context, pin)
{
  protected int Attempts = 0;
  protected int Failed = 0;
  public bool AutoStart = autoStart;
  public bool First = true;

  public bool Execute()
  {
    try
    {
      if (First)
      {
        OnStart();
        First = false;
      }
      var ret = OnExecute();
      Attempts++;
      if (ret)
      {
        PrintPins();
        OnEnd();
      }
      return ret;
    }
    catch (InvalidOperationException e)
    {
      Helper.Print(Context, User, e.Message);
      OnEnd();
      return true;
    }
  }
  protected abstract bool OnExecute();
  public void Init()
  {
    var output = OnInit();
    if (output != "")
      Print(output);
  }
  protected abstract string OnInit();
  protected virtual void OnStart()
  {
  }
  protected virtual void OnEnd()
  {
  }
}
