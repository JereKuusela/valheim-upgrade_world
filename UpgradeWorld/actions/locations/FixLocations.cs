namespace UpgradeWorld;
///<summary>Spawns locations in explored areas.</summary>
public class FixLocations : BaseOperation
{
  private int Fixed = 0;
  public FixLocations(Terminal context, FiltererParameters args) : base(context, args.Pin)
  {
    Execute();
  }

  void Execute()
  {
    // Iterate over all objects, find location proxies and check if the registered location is still valid.
    var zdos = ZDOMan.instance.m_objectsByID.Values;
    foreach (var zdo in zdos)
    {
      if (zdo.m_prefab != LocationProxyHash) continue;
      var locationId = zdo.GetInt(ZDOVars.s_location);
      var location = ZoneSystem.instance.GetLocation(locationId);
      if (location == null || !Helper.IsValid(location)) continue;
      var zone = ZoneSystem.GetZone(zdo.m_position);
      // If something already exists, no need to fix it.
      if (ZoneSystem.instance.m_locationInstances.ContainsKey(zone)) continue;
      ZoneSystem.instance.m_locationInstances[zone] = new ZoneSystem.LocationInstance
      {
        m_location = location,
        m_position = zdo.m_position,
        m_placed = true
      };
      Fixed++;
      AddPin(zdo.m_position);
      if (Settings.Verbose)
        Print($"Fixed location {location.m_prefabName} at {zone}");
    }
    Print($"{Fixed} locations fixed.");
    PrintPins();
  }

}
