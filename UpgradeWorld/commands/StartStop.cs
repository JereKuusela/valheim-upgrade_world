namespace UpgradeWorld;
public class StartStopCommand {
  public StartStopCommand() {
    CommandWrapper.RegisterEmpty("start");
    new Terminal.ConsoleCommand("start", "- Starts execution of operations.", (Terminal.ConsoleEventArgs args) => {
      Executor.DoExecute = true;
    });
    CommandWrapper.RegisterEmpty("stop");
    new Terminal.ConsoleCommand("stop", "- Stops execution of operations.", (Terminal.ConsoleEventArgs args) => {
      Executor.RemoveOperations();
    });
  }
}
