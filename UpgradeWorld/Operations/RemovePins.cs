using UnityEngine;

namespace UpgradeWorld {
  /// <summary>Removes pins from the map.</summary>
  public class RemovePins : BaseOperation {
    public RemovePins(Vector3 center, float distance, Terminal context) : base(context) {
      Remove(center, distance);
    }

    private void Remove(Vector3 center, float distance) {
      var removed = 0;
      while (Minimap.instance.RemovePin(center, distance))
        removed++;
      Print(removed + " pins removed.");
    }
  }
}