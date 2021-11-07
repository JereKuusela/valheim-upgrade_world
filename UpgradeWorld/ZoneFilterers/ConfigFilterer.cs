using System.Collections.Generic;
using UnityEngine;

namespace UpgradeWorld {
  public class ConfigFilterer : ZoneFilterer {
    public Vector2i[] FilterZones(Vector2i[] zones, ref List<string> messages) {
      var amount = zones.Length;
      var filterPoints = Settings.GetFilterPoints(GetPlayerPosition());
      foreach (var filterPoint in filterPoints) {
        zones = Filter.FilterByRange(zones, new Vector3(filterPoint.x, 0, filterPoint.y), filterPoint.min, filterPoint.max);
      }
      var filtered = amount - zones.Length;
      if (filtered > 0) messages.Add(filtered + " filtered by the config");
      return zones;
    }
    private static Vector3 GetPlayerPosition() {
      var player = Player.m_localPlayer;
      return player ? player.transform.position : new Vector3(0, 0, 0);
    }

  }
}