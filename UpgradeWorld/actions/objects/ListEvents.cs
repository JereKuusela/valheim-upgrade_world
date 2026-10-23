using System.Collections.Generic;
using System.Linq;
namespace UpgradeWorld;
/// <summary>Lists persistent events showing their position and biome.</summary>
public class ListEvents : EntityOperation
{
  public ListEvents(Terminal context, IdParameters args) : base(context, args.Pin)
  {
    List(args);
  }
  private void List(IdParameters args)
  {
    var events = PersistentEvents.Select(args);
    if (events.Count == 0)
    {
      Print("No persistent events found.");
      return;
    }
    foreach (var e in events)
      AddPin(e.position);
    Print(events.Select(e => $"{e.internalName}: {Helper.PrintVectorXZY(e.position)} {WorldGenerator.instance.GetBiome(e.position)}, radius {e.radius:F0}"), false);
    PrintPins();
  }
}
