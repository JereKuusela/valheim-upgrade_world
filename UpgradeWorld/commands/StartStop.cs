namespace UpgradeWorld;

public class StartStopCommand
{
  public StartStopCommand()
  {
    CommandWrapper.RegisterEmpty("start");
    Helper.Command("start", "- Starts execution of operations.", (args) =>
    {
      if (Helper.IsClient(args)) return;
      // Redirect all messsage to the starter.
      Executor.SetUser(ServerExecution.User);
      Executor.StartExecution();
    });
    CommandWrapper.RegisterEmpty("stop");
    Helper.Command("stop", "- Stops execution of operations.", (args) =>
    {
      if (Helper.IsClient(args)) return;
      Executor.StopExecution();
    });
    CommandWrapper.RegisterEmpty("uw_check");
    Helper.Command("uw_check", "- Prints queued operations and any pending snow reset.", (args) =>
    {
      if (Helper.IsClient(args)) return;
      var operations = Executor.GetOperations();
      var snow = Executor.DeferredSnow;
      if (operations.Count == 0 && snow == null)
      {
        Helper.Print(args.Context, "No operations queued.");
        return;
      }
      if (operations.Count > 0)
      {
        Helper.Print(args.Context, $"Queued operations ({operations.Count}):");
        for (int i = 0; i < operations.Count; i++)
          Helper.Print(args.Context, $"{i + 1}. {operations[i].GetInfo()}");
      }
      if (snow != null)
        Helper.Print(args.Context, $"resetsnow has {snow.PendingCount} terrain records pending.");
    });
  }
}
