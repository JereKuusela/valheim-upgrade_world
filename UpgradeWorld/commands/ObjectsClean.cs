namespace UpgradeWorld;
public class ObjectsCleanCommand
{
  public ObjectsCleanCommand()
  {
    CommandWrapper.Register("objects_clean", (int index) => FiltererParameters.Parameters, FiltererParameters.GetAutoComplete());
    new Terminal.ConsoleCommand("objects_clean", "[...args] - Removes missing objects.", (args) =>
    {
      FiltererParameters pars = new(args);
      if (!pars.Valid(args.Context)) return;
      if (Helper.IsClient(args)) return;
      new CleanObjects(args.Context, pars);
    });
  }
}
