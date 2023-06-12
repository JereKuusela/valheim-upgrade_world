namespace UpgradeWorld;
/// <summary>Safely changes time.</summary>
public class ChangeTime : TimeOperation {
  public ChangeTime(Terminal context, double time) : base(context) {
    Change(ZNet.instance.GetTimeSeconds() + time);
  }
}
