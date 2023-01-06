using System;
using System.Collections.Generic;
namespace UpgradeWorld;
public static class Executor
{
  private static List<ExecutedOperation> operations = new();
  private static List<Action> cleanUps = new();
  private static bool ShouldExecute = false;
  private static bool PrintInit = false;
  public static void DoExecute(ZRpc? user)
  {
    foreach (var operation in operations) operation.User = user;
    ShouldExecute = true;
  }
  public static void AddOperation(ExecutedOperation operation)
  {
    operation.Init();
    PrintInit = true;
    operations.Add(operation);
  }
  public static void AddCleanUp(Action cleanUp)
  {
    cleanUps.Add(cleanUp);
  }

  private static void DoClean()
  {
    foreach (var cleanUp in cleanUps) cleanUp();
    cleanUps.Clear();
  }
  public static void RemoveOperations()
  {
    operations.Clear();
    DoClean();
    ShouldExecute = false;
  }
  private static DateTime LastCommand = DateTime.MinValue;
  public static void Execute()
  {
    if (operations.Count == 0)
    {
      ShouldExecute = false;
      return;
    }
    if (!ShouldExecute && !Settings.AutoStart && !operations[0].AutoStart)
    {
      if (PrintInit) operations[operations.Count - 1].Print("Use <color=yellow>start</color> to begin execution or <color=yellow>stop</color> to cancel.");
      PrintInit = false;
      return;
    }
    if (DateTime.Now - LastCommand < TimeSpan.FromMilliseconds(Settings.Throttle)) return;
    if (operations[0].Execute())
    {
      LastCommand = DateTime.Now;
      operations.RemoveAt(0);
    }
    if (operations.Count == 0)
      DoClean();
  }
}
