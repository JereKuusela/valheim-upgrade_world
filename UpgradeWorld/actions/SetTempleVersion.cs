using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UpgradeWorld;
public class TempleVersion(Terminal context, string version, bool start) : ExecutedOperation(context, start)
{
  readonly string Version = version;

  private static readonly int HashTemple = "StartTemple".GetStableHashCode();
  private static readonly int HashEikthyr = "BossStone_Eikthyr".GetStableHashCode();
  private static readonly int HashElder = "BossStone_TheElder".GetStableHashCode();
  private static readonly int HashBonemass = "BossStone_Bonemass".GetStableHashCode();
  private static readonly int HashModer = "BossStone_DragonQueen".GetStableHashCode();
  private static readonly int HashYagluth = "BossStone_Yagluth".GetStableHashCode();
  private static readonly int HashQueen = "BossStone_TheQueen".GetStableHashCode();
  private static readonly int HashProxy = "LocationProxy".GetStableHashCode();
  // Only these two actually move.
  private static readonly HashSet<int> BossStones = [HashEikthyr, HashQueen];

  private static readonly Dictionary<int, Vector3> MistlandsPos = new() {
    {HashEikthyr, new Vector3(-7.27f, -0.14f, 3.81f)},
    {HashElder, new Vector3(-1.76f, -0.21f, 8.11f)},
    {HashBonemass, new Vector3(7.02f, -0.32f, 5.53f)},
    {HashModer, new Vector3(9.61f, -0.04f, -4.28f)},
    {HashYagluth, new Vector3(1.61f, -0.04f, -11.27f)},
    {HashQueen, new Vector3(-6.85f, 0.00f, -6.97f)}
  };
  private static readonly Dictionary<int, Quaternion> MistlandsRot = new() {
    {HashEikthyr, new Quaternion(0.00000f, 0.45976f, 0.00000f, -0.88804f)},
    {HashElder, new Quaternion(0.00000f, 0.06553f, 0.00000f, -0.99785f)},
    {HashBonemass, new Quaternion(0.00000f, 0.43752f, 0.00000f, 0.89921f)},
    {HashModer, new Quaternion(0.00000f, 0.82790f, 0.00000f, 0.56088f)},
    {HashYagluth, new Quaternion(0.00000f, 0.99785f, 0.00000f, 0.06556f)},
    {HashQueen, new Quaternion(0.00000f, 0.87991f, 0.00000f, -0.47513f)}
  };

  private static readonly Dictionary<int, Vector3> AshlandsPos = new() {
    {HashEikthyr, new Vector3(-6.40f, -0.14f, 5.04f)},
    {HashElder, new Vector3(-1.76f, -0.21f, 8.11f)},
    {HashBonemass, new Vector3(7.02f, -0.32f, 5.53f)},
    {HashModer, new Vector3(9.61f, -0.04f, -4.28f)},
    {HashYagluth, new Vector3(1.61f, -0.04f, -11.27f)},
    {HashQueen, new Vector3(-5.38f, 0.00f, -8.71f)}
  };
  private static readonly Dictionary<int, Quaternion> AshlandsRot = new() {
    {HashEikthyr, new Quaternion(0.00000f, 0.43937f, 0.00000f, -0.89830f)},
    {HashElder, new Quaternion(0.00000f, 0.06553f, 0.00000f, -0.99785f)},
    {HashBonemass, new Quaternion(0.00000f, 0.43752f, 0.00000f, 0.89921f)},
    {HashModer, new Quaternion(0.00000f, 0.82790f, 0.00000f, 0.56088f)},
    {HashYagluth, new Quaternion(0.00000f, 0.99785f, 0.00000f, 0.06556f)},
    {HashQueen, new Quaternion(0.00000f, 0.93423f, 0.00000f, -0.35667f)}
  };

  protected override bool OnExecute()
  {
    // Find StartTemple location.
    var locs = ZDOMan.instance.m_objectsByID.Where(kvp => kvp.Value.m_prefab == HashProxy && kvp.Value.GetInt(ZDOVars.s_location) == HashTemple).ToArray();
    if (locs.Length == 0)
    {
      Print("Error: StartTemple location not found.");
      return true;
    }
    foreach (var loc in locs)
    {
      var zone = ZoneSystem.GetZone(loc.Value.m_position);
      var zdos = Helper.GetZDOs(zone);
      if (zdos == null) continue;
      zdos = zdos.Where(zdo => BossStones.Contains(zdo.m_prefab)).ToList();
      var pos = loc.Value.m_position;
      var rot = loc.Value.GetRotation();
      foreach (var zdo in zdos)
      {
        var name = ZNetScene.instance.GetPrefab(zdo.m_prefab).name;
        if (Version == "mistlands")
        {
          var newPos = pos + rot * MistlandsPos[zdo.m_prefab];
          var newRot = rot * MistlandsRot[zdo.m_prefab];
          Helper.MoveZDO(zdo, newPos, newRot);
          Print($"Moved {name} to {Helper.PrintVectorXZY(newPos)} / {Helper.PrintAngleYXZ(newRot)}.");
        }
        else if (Version == "ashlands")
        {
          var newPos = pos + rot * AshlandsPos[zdo.m_prefab];
          var newRot = rot * AshlandsRot[zdo.m_prefab];
          Helper.MoveZDO(zdo, newPos, newRot);
          Print($"Moved {name} to {Helper.PrintVectorXZY(newPos)} / {Helper.PrintAngleYXZ(newRot)}.");
        }
        else
        {
          var localPos = Quaternion.Inverse(rot) * (zdo.GetPosition() - pos);
          var localRot = Quaternion.Inverse(rot) * zdo.GetRotation();
          Log([$"Stone {name} at pos {localPos} and rot {localRot}."]);
        }
      }
    }
    if (Version != "")
      Print($"Updated start temple version to {Version}.");
    return true;
  }

  protected override string OnInit()
  {
    if (Version == "")
      return $"Printing boss stone positions.";
    return $"Updating start temple version to {Version}. This moves boss stones.";
  }
}
