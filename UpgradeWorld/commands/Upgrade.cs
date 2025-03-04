using Service;

namespace UpgradeWorld;
public class UpgradeCommand
{
  public UpgradeCommand()
  {
    CommandWrapper.Register("upgrade", index => index == 0 ? Upgrade.Types : FiltererParameters.Parameters, FiltererParameters.GetAutoComplete());
    Helper.Command("upgrade", "[operation] [...args] - Performs a predefined upgrade operation.", (args) =>
    {
      FiltererParameters pars = new(args);
      var selectedType = "";
      foreach (var type in Upgrade.Types)
      {
        if (Parse.Flag(pars.Unhandled, type.ToLower())) selectedType = type;
      }
      if (!pars.Valid(args.Context)) return;
      if (Helper.IsClient(args)) return;
      new Upgrade(args.Context, selectedType, pars.Unhandled, pars);
    }, () => Upgrade.Types);
  }
}
