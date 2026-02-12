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
    Helper.Command("uw_check", "- Prints list of queued operations.", (args) =>
    {
      if (Helper.IsClient(args)) return;
      var operations = Executor.GetOperations();
      if (operations.Count == 0)
      {
        Helper.Print(args.Context, "No operations queued.");
        return;
      }
      Helper.Print(args.Context, $"Queued operations ({operations.Count}):");
      for (int i = 0; i < operations.Count; i++)
      {
        var info = operations[i].GetInfo();
        Helper.Print(args.Context, $"{i + 1}. {info}");
      }
    });
  }
}
