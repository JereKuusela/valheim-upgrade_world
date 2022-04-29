using System.Linq;
namespace UpgradeWorld;
public class ObjectsCountCommand {
  public ObjectsCountCommand() {
    CommandWrapper.Register("objects_count", (int index) => {
      if (index == 0) return CommandWrapper.ObjectIds();
      return FiltererParameters.Parameters;
    }, FiltererParameters.GetAutoComplete());
    new Terminal.ConsoleCommand("objects_count", "[all] [id1,id2,...] [...args] - Counts objects. Without ids, counts all objects.", (Terminal.ConsoleEventArgs args) => {
      RequiredIdParameters pars = new(args);
      pars.Ids = Parse.Flag(pars.Ids, "all", out var showAll).ToList();
      if (!pars.Valid(args.Context)) return;
      if (Helper.IsClient(args)) return;
      if (pars.Ids.Count() == 0)
        new CountAllObjects(args.Context, showAll, pars);
      else
        new CountObjects(args.Context, pars.Ids, pars);
    }, optionsFetcher: () => ZNetScene.instance.GetPrefabNames());
  }
}
