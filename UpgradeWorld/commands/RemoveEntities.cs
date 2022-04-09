using System.Linq;
namespace UpgradeWorld;
public class RemoveEntitiesCommand {
  public RemoveEntitiesCommand() {
    CommandWrapper.Register("remove_entities", (int index) => {
      if (index == 0) return CommandWrapper.ObjectIds();
      return FiltererParameters.Parameters;
    }, FiltererParameters.GetAutoComplete());
    new Terminal.ConsoleCommand("remove_entities", "[...ids] [...args] - Removes entities.", (Terminal.ConsoleEventArgs args) => {
      if (!Helper.IsServer(args)) return;
      IdParameters pars = new(args);
      if (pars.Valid(args.Context))
        new RemoveEntities(args.Context, pars.Ids, pars);
    }, optionsFetcher: () => ZNetScene.instance.GetPrefabNames());
  }
}
