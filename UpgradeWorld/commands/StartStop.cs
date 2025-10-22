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
      Executor.RemoveOperations();
    });
  }
}
