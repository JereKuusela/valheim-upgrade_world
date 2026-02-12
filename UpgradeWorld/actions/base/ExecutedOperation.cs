using System;
using System.Collections;
using System.Diagnostics;
namespace UpgradeWorld;
///<summary>Base class for all operations that need execution. Provides the execution logic.</summary>
public abstract class ExecutedOperation(Terminal context, bool pin = false) : BaseOperation(context, pin)
{
  protected int Failed = 0;

  public IEnumerator Execute(Stopwatch sw)
  {
    IEnumerator executeEnumerator;
    try
    {
      OnStart();
      executeEnumerator = OnExecute(sw);
    }
    catch (InvalidOperationException e)
    {
      Helper.Print(Context, User, e.Message);
      OnEnd();
      yield break;
    }
    yield return executeEnumerator;
    PrintPins();
    OnEnd();
  }

  protected abstract IEnumerator OnExecute(Stopwatch sw);
  public bool Init(bool autoStart)
  {
    var output = OnInit();
    if (output == "") return false;
    if (!autoStart)
      output += Helper.GetStartMessage();
    Print(output);
    return true;
  }
  protected abstract string OnInit();
  public string GetInfo()
  {
    var info = OnInit();
    return info != "" ? info : GetType().Name;
  }
  protected virtual void OnStart()
  {
  }
  protected virtual void OnEnd()
  {
  }
}
