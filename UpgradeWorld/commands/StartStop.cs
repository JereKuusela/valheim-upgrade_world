namespace UpgradeWorld;
public class StartStopCommand
{
  public StartStopCommand()
  {
    CommandWrapper.RegisterEmpty("start");
    Helper.Command("start", "- Starts execution of operations.", (args) =>
    {
      if (Helper.IsClient(args)) return;
      Executor.DoExecute(ServerExecution.User);
    });
    CommandWrapper.RegisterEmpty("stop");
    Helper.Command("stop", "- Stops execution of operations.", (args) =>
    {
      if (Helper.IsClient(args)) return;
      Executor.RemoveOperations();
    });
  }
}
