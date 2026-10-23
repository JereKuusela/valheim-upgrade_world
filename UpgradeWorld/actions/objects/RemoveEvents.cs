namespace UpgradeWorld;
/// <summary>Removes persistent events with their objects and not yet placed location.</summary>
public class RemoveEvents : EntityOperation
{
  public RemoveEvents(Terminal context, IdParameters args) : base(context, args.Pin)
  {
    Remove(args);
  }
  private void Remove(IdParameters args)
  {
    var events = PersistentEvents.Select(args);
    foreach (var e in events)
    {
      AddPin(e.position);
      if (Settings.Verbose)
        Print($"Removed persistent event {e.internalName} at {Helper.PrintVectorXZY(e.position)}.");
    }
    var objects = PersistentEvents.Remove(events);
    Print($"Removed {events.Count} persistent event{S(events.Count)} ({objects} object{S(objects)}).");
  }
}
