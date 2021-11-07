using System.Collections.Generic;

namespace UpgradeWorld {
  public interface ZoneFilterer {
    Vector2i[] FilterZones(Vector2i[] zones, ref List<string> messages);
  }
}