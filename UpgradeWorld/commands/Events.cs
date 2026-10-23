using System.Collections.Generic;
using System.Linq;
namespace UpgradeWorld;

public static class EventsCommands
{
  private static readonly Dictionary<string, System.Func<int, List<string>?>> Named = CreateNamed();
  private static readonly List<string> Parameters = [.. FiltererParameters.Parameters.Concat(["id", "ignore"]).OrderBy(x => x)];

  private static Dictionary<string, System.Func<int, List<string>?>> CreateNamed()
  {
    var named = FiltererParameters.GetAutoComplete();
    named["id"] = index => PersistentEvents.Names();
    named["ignore"] = index => PersistentEvents.Names();
    return named;
  }
  public static void Register(string name) =>
    CommandWrapper.Register(name, index => index == 0 ? PersistentEvents.Names() : Parameters, Named);
}

public class EventsListCommand
{
  public EventsListCommand()
  {
    EventsCommands.Register("events_list");
    Helper.Command("events_list", "[id1,id2,...] [...args] - Lists persistent events showing their position and biome.", (args) =>
    {
      IdParameters pars = new(args, false, false);
      if (!pars.Valid(args.Context)) return;
      if (Helper.IsClient(args)) return;
      new ListEvents(args.Context, pars);
    }, () => PersistentEvents.Names());
  }
}

public class EventsRemoveCommand
{
  public EventsRemoveCommand()
  {
    EventsCommands.Register("events_remove");
    Helper.Command("events_remove", "[id1,id2,...] [...args] - Removes persistent events with their objects. Events are selected by their center position.", (args) =>
    {
      IdParameters pars = new(args, false, false);
      if (!pars.Valid(args.Context)) return;
      if (Helper.IsClient(args)) return;
      Executor.AddOperation(new QueuedOperation(args.Context, pars.Pin, "Remove persistent events.", () =>
      {
        new RemoveEvents(args.Context, pars);
      }), pars.Start);
    }, () => PersistentEvents.Names());
  }
}
