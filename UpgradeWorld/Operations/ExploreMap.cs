using UnityEngine;

namespace UpgradeWorld {
  /// <summary>Explores the map.</summary>
  public class ExploreMap : BaseOperation {
    private Vector3 Center;
    private float Distance;
    private bool Explore;
    private int Explored = 0;
    public ExploreMap(Vector3 center, float distance, bool explore, Terminal context) : base(context) {
      Center = center;
      Distance = distance;
      Explore = explore;
    }

    protected override bool OnExecute() {
      ExploreRadius(Center, Distance, Explore);
      return true;
    }

    ///<summary>Explores/unexplores a circle. Copypaste from the base game code</summary>
    private void ExploreRadius(Vector3 p, float radius, bool explore) {
      var minimap = Minimap.instance;
      var limit = (int)Mathf.Ceil(radius / minimap.m_pixelSize);
      minimap.WorldToPixel(p, out var x, out var y);
      for (var i = y - limit; i <= y + limit; i++) {
        if (i < 0 || i >= minimap.m_textureSize) continue;
        for (var j = x - limit; j <= x + limit; j++) {
          if (j < 0 || j >= minimap.m_textureSize) continue;
          if (new Vector2(j - x, i - y).magnitude > limit) continue;
          ExploreSpot(j, i, explore);
        }
      }
      if (Explored > 0) minimap.m_fogTexture.Apply();
    }

    ///<summary>Explores/unexplores a spot. Copypaste from the base game code.</summary>
    private void ExploreSpot(int x, int y, bool explore) {
      var minimap = Minimap.instance;
      if (explore == minimap.m_explored[y * minimap.m_textureSize + x]) return;
      Color pixel = minimap.m_fogTexture.GetPixel(x, y);
      pixel.r = explore ? 0 : byte.MaxValue;
      minimap.m_fogTexture.SetPixel(x, y, pixel);
      minimap.m_explored[y * minimap.m_textureSize + x] = explore;
      Explored++;
      return;
    }
    protected override void OnEnd() {
      Print(Explored + " spots " + (Explore ? "explored" : "unexplored") + ".");
    }
  }
}