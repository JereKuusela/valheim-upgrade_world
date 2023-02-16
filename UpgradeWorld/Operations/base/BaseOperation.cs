using System.Collections.Generic;
namespace UpgradeWorld;
///<summary>Base class for all operations. Only provides basic utilities.</summary>
public abstract class BaseOperation
{
  protected int LocationProxyHash = "LocationProxy".GetStableHashCode();
  protected int LocationHash = "location".GetStableHashCode();
  protected Terminal Context;
  public ZRpc? User = null;
  protected BaseOperation(Terminal context)
  {
    Context = context ?? Console.instance;
    User = ServerExecution.User;
  }
  public void Print(string value, bool addDot = true)
  {
    if (addDot && !value.EndsWith(".")) value += ".";
    Helper.Print(Context, User, value);
  }
  protected void Log(IEnumerable<string> values)
  {
    ZLog.Log("\n" + string.Join("\n", values));
  }
  protected void Print(IEnumerable<string> values, bool addDot = true)
  {
    foreach (var s in values) Print(s, addDot);
  }
  protected void PrintOnce(string value, bool addDot = true)
  {
    if (addDot && !value.EndsWith(".")) value += ".";
    Helper.PrintOnce(Context, User, value, 10f);
  }
}
