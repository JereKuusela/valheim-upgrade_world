using System.Collections.Generic;

namespace Service;

public static class Hash
{
  public static int Level = "level".GetStableHashCode();
  public static int TimeOfDeath = "timeOfDeath".GetStableHashCode();
  public static int Location = "location".GetStableHashCode();
  public static int AddedDefaultItems = "addedDefaultItems".GetStableHashCode();
  public static int Items = "items".GetStableHashCode();
  public static int AliveTime = "alive_time".GetStableHashCode();
  public static int PickedTime = "picked_time".GetStableHashCode();
  public static int SpawnTime = "spawn_item".GetStableHashCode();
  public static int Changed = "override_changed".GetStableHashCode();
  public static int OverrideItems = "override_items".GetStableHashCode();
  public static KeyValuePair<int, int> SpawnId = ZDO.GetHashZDOID("spawn_id");

}