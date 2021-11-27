using System;
using System.Collections.Generic;

namespace UpgradeWorld {

  public static class Executor {
    private static List<ExecutedOperation> operations = new List<ExecutedOperation>();
    private static List<Action> cleanUps = new List<Action>();
    public static bool DoExecute = false;
    private static bool PrintInit = false;

    public static void AddOperation(ExecutedOperation operation) {
      operation.Init();
      PrintInit = true;
      operations.Add(operation);
    }
    public static void AddCleanUp(Action cleanUp) {
      cleanUps.Add(cleanUp);
    }

    private static void DoClean() {
      foreach (var cleanUp in cleanUps) cleanUp();
      cleanUps.Clear();
    }
    public static void RemoveOperations() {
      operations.Clear();
      DoClean();
      DoExecute = false;
    }
    public static void Execute() {
      if (operations.Count == 0) return;
      if (!DoExecute && !Settings.AutoStart && !operations[0].AutoStart) {
        if (PrintInit) operations[operations.Count - 1].Print("Use start to begin execution or stop to cancel.");
        PrintInit = false;
        return;
      }
      if (operations[0].Execute())
        operations.RemoveAt(0);
      if (operations.Count == 0) {
        DoClean();
        DoExecute = false;
      }
    }
  }
}