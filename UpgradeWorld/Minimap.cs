using HarmonyLib;
using UnityEngine;
namespace UpgradeWorld;

[HarmonyPatch(typeof(Minimap), nameof(Minimap.Awake))]
public class Minimap_Alignment {
  static void Postfix(Minimap __instance) {
    // Removes the need of padding.
    __instance.m_biomeNameSmall.alignment = TextAnchor.UpperRight;
    __instance.m_biomeNameLarge.alignment = TextAnchor.UpperRight;
  }
}

[HarmonyPatch(typeof(Minimap), nameof(Minimap.UpdateBiome))]
public class Minimap_ShowPos {
  private static void AddText(UnityEngine.UI.Text input, string text) {
    if (input.text.Contains(text)) return;
    input.text += text;
  }
  private static void CleanUp(UnityEngine.UI.Text input, string text) {
    if (text == "" || !input.text.Contains(text)) return;
    input.text = input.text.Replace(text, "");
  }
  private static string PreviousText = "";
  static void Postfix(Minimap __instance, Player player) {
    var mode = __instance.m_mode;
    if (PreviousText != "") {
      CleanUp(__instance.m_biomeNameLarge, PreviousText);
      CleanUp(__instance.m_biomeNameSmall, PreviousText);
      PreviousText = "";
    }
    if (!Settings.MiniMapCoordinates && !Settings.MapCoordinates) return;
    var position = player.transform.position;
    if (mode == Minimap.MapMode.Large) {
      position = __instance.ScreenToWorldPoint(ZInput.IsMouseActive() ? Input.mousePosition : new((Screen.width / 2f), (Screen.height / 2f)));
      position.y = WorldGenerator.instance.GetHeight(position.x, position.z);
    }
    var zone = ZoneSystem.instance.GetZone(position);
    var positionText = "x: " + position.x.ToString("F0") + " y: " + position.y.ToString("F0") + " z: " + position.z.ToString("F0");
    var zoneText = "zone: " + zone.x + "/" + zone.y;
    var text = $"\n{zoneText}\n{positionText}";
    if (mode == Minimap.MapMode.Large && Settings.MapCoordinates)
      AddText(__instance.m_biomeNameLarge, text);
    if (mode == Minimap.MapMode.Small && Settings.MiniMapCoordinates)
      AddText(__instance.m_biomeNameSmall, text);
    PreviousText = text;
  }
}
