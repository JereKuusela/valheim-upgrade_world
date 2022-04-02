using UnityEngine;
namespace UpgradeWorld;
/// <summary>Explores the map.</summary>
public class ExploreMap : BaseOperation {
  public ExploreMap(Vector3 center, float distance, bool explore, Terminal context) : base(context) {
    ExploreRadius(center, distance, explore);
  }

  ///<summary>Explores/unexplores a circle. Copypaste from the base game code</summary>
  private void ExploreRadius(Vector3 p, float radius, bool explore) {
    var minimap = Minimap.instance;
    var limit = (int)Mathf.Ceil(radius / minimap.m_pixelSize);
    minimap.WorldToPixel(p, out var x, out var y);
    var explored = 0;
    for (var i = y - limit; i <= y + limit; i++) {
      if (i < 0 || i >= minimap.m_textureSize) continue;
      for (var j = x - limit; j <= x + limit; j++) {
        if (j < 0 || j >= minimap.m_textureSize) continue;
        if (new Vector2(j - x, i - y).magnitude > limit) continue;
        if (ExploreSpot(j, i, explore)) explored++;
      }
    }
    if (explored > 0) minimap.m_fogTexture.Apply();
    Print(explored + " spots " + (explore ? "explored" : "unexplored") + ".");
  }

  ///<summary>Explores/unexplores a spot. Copypaste from the base game code.</summary>
  private bool ExploreSpot(int x, int y, bool explore) {
    var minimap = Minimap.instance;
    if (explore == minimap.m_explored[y * minimap.m_textureSize + x]) return false;
    Color pixel = minimap.m_fogTexture.GetPixel(x, y);
    pixel.r = explore ? 0 : byte.MaxValue;
    minimap.m_fogTexture.SetPixel(x, y, pixel);
    minimap.m_explored[y * minimap.m_textureSize + x] = explore;
    return true;
  }
}
