using System.Collections.Generic;

namespace UpgradeWorld {

  public static class Executor {
    private static List<BaseOperation> operations = new List<BaseOperation>();

    public static void AddOperation(BaseOperation operation) => operations.Add(operation);
    public static void RemoveOperation() {
      if (operations.Count > 0)
        operations.RemoveAt(0);
    }
    public static void Execute() {
      if (operations.Count == 0) return;
      if (operations[0].Execute())
        RemoveOperation();
    }
  }
}