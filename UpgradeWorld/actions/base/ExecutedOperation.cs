using System;
using System.Collections;
using System.Diagnostics;
namespace UpgradeWorld;
///<summary>Base class for all operations that need execution. Provides the execution logic.</summary>
public abstract class ExecutedOperation(Terminal context, bool pin = false) : BaseOperation(context, pin)
{
  protected int Failed = 0;
  public bool First = true;

  public IEnumerator Execute(Stopwatch sw)
  {
    IEnumerator? executeEnumerator;
    try
    {
      if (First)
      {
        OnStart();
        First = false;
      }
      executeEnumerator = OnExecute(sw);
    }
    catch (InvalidOperationException e)
    {
      Helper.Print(Context, User, e.Message);
      OnEnd();
      yield break;
    }

    if (executeEnumerator != null)
      yield return executeEnumerator;

    PrintPins();
    OnEnd();
  }
  protected abstract IEnumerator OnExecute(Stopwatch sw);
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
