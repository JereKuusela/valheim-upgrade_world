using System.Collections.Generic;

namespace UpgradeWorld {

  public static class Executor {
    private static List<ExecutedOperation> operations = new List<ExecutedOperation>();
    public static bool DoExecute = false;

    public static void AddOperation(ExecutedOperation operation) {
      operation.Init();
      operations.Add(operation);
    }
    public static void RemoveOperations() {
      operations.Clear();
    }
    public static void Execute() {
      if (operations.Count == 0) DoExecute = false;
      if (!DoExecute) return;
      if (operations[0].Execute())
        operations.RemoveAt(0);
    }
  }
}