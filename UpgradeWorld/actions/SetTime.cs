namespace UpgradeWorld;
/// <summary>Safely sets the time.</summary>
public class SetTime : TimeOperation {
  public SetTime(Terminal context, double time) : base(context) {
    Change(time);
  }
}
