using HarmonyLib;
using UnityEngine;
namespace UpgradeWorld;
[HarmonyPatch(typeof(Minimap), nameof(Minimap.UpdateBiome))]
public class Minimap_ShowPos {
  static void Prefix(Minimap __instance) {
    __instance.m_biomeNameSmall.text = Localization.instance.Localize("$biome_" + __instance.m_biome.ToString().ToLower());
  }
  private static void AddText(UnityEngine.UI.Text input, string text) {
    if (input.text.EndsWith(text)) return;
    input.text = $"\n\n{input.text}{text}";
  }
  static void Postfix(Minimap __instance, Player player) {
    var mode = __instance.m_mode;
    var position = player.transform.position;
    if (mode == Minimap.MapMode.Large) {
      position = __instance.ScreenToWorldPoint(ZInput.IsMouseActive() ? Input.mousePosition : new(Screen.width / 2f, Screen.height / 2f));
      position.y = WorldGenerator.instance.GetHeight(position.x, position.z);
    }
    var zone = ZoneSystem.instance.GetZone(position);
    var positionText = "x: " + position.x.ToString("F0") + " y: " + position.y.ToString("F0") + " z: " + position.z.ToString("F0");
    var zoneText = "zone: " + zone.x + "/" + zone.y;
    var text = $"\n{zoneText}\n{positionText}";
    AddText(__instance.m_biomeNameLarge, text);
    AddText(__instance.m_biomeNameSmall, text);
  }
}
