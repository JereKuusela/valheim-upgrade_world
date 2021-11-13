using UnityEngine;

namespace UpgradeWorld {
  /// <summary>Removes pins from the map.</summary>
  public class RemovePins : BaseOperation {
    private Vector3 Center;
    private float Distance;
    private int Removed = 0;
    public RemovePins(Vector3 center, float distance, Terminal context) : base(context) {
      Center = center;
      Distance = distance;
    }

    protected override bool OnExecute() {
      while (Minimap.instance.RemovePin(Center, Distance))
        Removed++;
      return true;
    }
    protected override void OnEnd() {
      Print(Removed + " pins removed.");
    }
  }
}