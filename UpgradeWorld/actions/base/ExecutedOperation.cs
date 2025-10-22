using System;
using System.Collections;
using System.Diagnostics;
namespace UpgradeWorld;
///<summary>Base class for all operations that need execution. Provides the execution logic.</summary>
public abstract class ExecutedOperation(Terminal context, bool autoStart, bool pin = false) : BaseOperation(context, pin)
{
  protected int Attempts = 0;
  protected int Failed = 0;
  public bool AutoStart = autoStart;
  public bool First = true;

  public IEnumerator Execute(Stopwatch sw)
  {
    if (First)
    {
      try
      {
        OnStart();
      }
      catch (InvalidOperationException e)
      {
        Helper.Print(Context, User, e.Message);
        OnEnd();
        yield break;
      }
      First = false;
      yield return null; // Yield after initialization
    }
    // Execute the operation as a coroutine
    IEnumerator? executeEnumerator;
    try
    {
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

    Attempts++;
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
