namespace UpgradeWorld;
public class StartStopCommand {
  public StartStopCommand() {
    CommandWrapper.RegisterEmpty("start");
    new Terminal.ConsoleCommand("start", "- Starts execution of operations.", (args) => {
      if (Helper.IsClient(args)) return;
      Executor.DoExecute(ServerExecution.User);
    });
    CommandWrapper.RegisterEmpty("stop");
    new Terminal.ConsoleCommand("stop", "- Stops execution of operations.", (args) => {
      if (Helper.IsClient(args)) return;
      Executor.RemoveOperations();
    });
  }
}
