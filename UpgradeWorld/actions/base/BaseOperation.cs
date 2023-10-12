using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace UpgradeWorld;
///<summary>Base class for all operations. Only provides basic utilities.</summary>
public abstract class BaseOperation
{
  protected int LocationProxyHash = "LocationProxy".GetStableHashCode();
  protected int LocationHash = "location".GetStableHashCode();
  protected Terminal Context;
  public ZRpc? User = null;
  private readonly List<Vector3> Pins = [];
  private readonly bool Pin;
  protected BaseOperation(Terminal context, bool pin = false)
  {
    Context = context ?? Console.instance;
    User = ServerExecution.User;
    Pin = pin;
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
  protected void AddPin(Vector3 pos)
  {
    if (Pin) Pins.Add(pos);
  }
  protected void PrintPins()
  {
    if (!Pin) return;
    if (User != null)
    {
      User.Invoke(ServerExecution.RPC_Pins, string.Join("|", Pins.Select(Helper.PrintVectorXZY)));
    }
    else
    {
      var findPins = Console.instance.m_findPins;
      foreach (var pin in findPins)
        Minimap.instance?.RemovePin(pin);
      findPins.Clear();
      foreach (var pos in Pins)
      {
        var pin = Minimap.instance?.AddPin(pos, Minimap.PinType.Icon3, "", false, false, Player.m_localPlayer.GetPlayerID());
        if (pin != null)
          findPins.Add(pin);
      }
    }
  }
}
