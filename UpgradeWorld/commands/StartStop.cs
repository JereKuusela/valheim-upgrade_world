namespace UpgradeWorld;
public class StartStopCommand {
  public StartStopCommand() {
    new Terminal.ConsoleCommand("start", "- Starts execution of operations.", (Terminal.ConsoleEventArgs args) => {
      Executor.DoExecute = true;
    });
    new Terminal.ConsoleCommand("stop", "- Stops execution of operations.", (Terminal.ConsoleEventArgs args) => {
      Executor.RemoveOperations();
    });
  }
}
