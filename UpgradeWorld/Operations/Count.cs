using System.Collections.Generic;
using System.Linq;

namespace UpgradeWorld {
  /// <summary>Counts the amount of a given entity id within a given radius.</summary>
  public class Count : BaseOperation {
    private string Id;
    private float Radius;
    public Count(string id, float radius, Terminal context) : base(context) {
      Id = id;
      Radius = radius;
    }
    protected override bool OnExecute() {
      var prefab = ZNetScene.instance.GetPrefab(Id);
      if (prefab == null)
        Print("Invalid entity ID.");
      else {
        var zdos = new List<ZDO>();
        ZDOMan.instance.GetAllZDOsWithPrefab(prefab.name, zdos);
        var position = Player.m_localPlayer.transform.position;
        var count = zdos.Count();
        if (Radius > 0) count = zdos.Where(zdo => Utils.DistanceXZ(zdo.GetPosition(), position) < Radius).Count();
        Print(prefab.name + ": " + count);
      }
      return true;
    }
  }
}