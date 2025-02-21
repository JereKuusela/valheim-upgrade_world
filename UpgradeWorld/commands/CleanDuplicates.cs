namespace UpgradeWorld;
public class CleanDuplicatesCommand
{
  public CleanDuplicatesCommand()
  {
    CommandWrapper.Register("clean_duplicates", (int index) => FiltererParameters.Parameters, FiltererParameters.GetAutoComplete());
    new Terminal.ConsoleCommand("clean_duplicates", "[...args] - Removes duplicated objects.", (args) =>
    {
      FiltererParameters pars = new(args);
      if (!pars.Valid(args.Context)) return;
      if (Helper.IsClient(args)) return;
      new CleanDuplicates(args.Context, pars.Pin, true);
    });
  }
}
