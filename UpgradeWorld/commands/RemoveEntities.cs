using System.Linq;
namespace UpgradeWorld;
public class RemoveEntitiesCommand {
  public RemoveEntitiesCommand() {
    new Terminal.ConsoleCommand("remove_entities", "[...ids] [...args] - Removes entities.", (Terminal.ConsoleEventArgs args) => {
      if (!Helper.IsServer(args)) return;
      FiltererParameters parameters = new();
      var ids = Parse.FiltererArgs(args.Args, parameters);
      new RemoveEntities(args.Context, ids, parameters);
    }, optionsFetcher: () => ZNetScene.instance.GetPrefabNames());
  }
}
