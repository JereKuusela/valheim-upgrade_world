using HarmonyLib;
using UnityEngine;

namespace UpgradeWorld {
  [HarmonyPatch(typeof(Minimap), "UpdateBiome")]

  public class Minimap_ShowPos {
    // Text doesn't always get updated so extra stuff must be reseted manually.
    private static string previousText = "";
    public static void Prefix(Minimap __instance) {
      __instance.m_biomeNameLarge.text = previousText;
      __instance.m_biomeNameSmall.text = previousText;

    }
    public static void Postfix(Minimap __instance, Player player) {
      previousText = __instance.m_biomeNameLarge.text;
      var mode = Patch.Get<Minimap.MapMode>(__instance, "m_mode");
      var position = player.transform.position;
      if (mode == Minimap.MapMode.Large)
        position = Patch.ScreenToWorldPoint(__instance, ZInput.IsMouseActive() ? Input.mousePosition : new Vector3((float)(Screen.width / 2), (float)(Screen.height / 2)));
      var zone = ZoneSystem.instance.GetZone(position);
      var positionText = "x: " + position.x.ToString("F0") + " z: " + position.z.ToString("F0");
      var zoneText = "zone: " + zone.x + "/" + zone.y;
      var text = "\n\n" + previousText + "\n" + zoneText + "\n" + positionText;
      __instance.m_biomeNameLarge.text = text;
      __instance.m_biomeNameSmall.text = text;
    }
  }
}