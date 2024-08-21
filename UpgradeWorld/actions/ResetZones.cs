using System.Collections.Generic;
using System.Linq;
namespace UpgradeWorld;

/// <summary>Destroys everything in a zone so that the world generator can regenerate it.</summary>
public class ResetZones : ZoneOperation
{
  private Dictionary<Vector2i, Direction> BorderZones = [];
  public ResetZones(Terminal context, FiltererParameters args) : base(context, args)
  {
    Operation = "Reset";
    ZonesPerUpdate = Settings.DestroysPerUpdate;
    args.TargetZones = TargetZones.Generated;
    InitString = args.Print("Reset");
    Filterers = FiltererFactory.Create(args);
  }
  private int Reseted = 0;
  protected override bool ExecuteZone(Vector2i zone)
  {
    var zoneSystem = ZoneSystem.instance;
    var scene = ZNetScene.instance;
    var sectorObjects = Helper.GetZDOs(zone);

    var players = ZNet.instance.m_players.Select(player => player.m_characterID).ToHashSet();
    if (sectorObjects != null)
    {
      foreach (var zdo in sectorObjects)
      {
        if (players.Contains(zdo.m_uid)) continue;
        var position = zdo.GetPosition();
        if (zoneSystem.GetZone(position) == zone)
          Helper.RemoveZDO(zdo);
      }
    }
    var locations = zoneSystem.m_locationInstances;
    if (locations.TryGetValue(zone, out var location))
    {
      location.m_placed = false;
      location.m_position = new(location.m_position.x, WorldGenerator.instance.GetHeight(location.m_position.x, location.m_position.z), location.m_position.z);
      zoneSystem.m_locationInstances[zone] = location;
    }
    zoneSystem.m_generatedZones.Remove(zone);
    Reseted++;
    AddBorder(zone, Direction.North);
    AddBorder(zone, Direction.East);
    AddBorder(zone, Direction.South);
    AddBorder(zone, Direction.West);
    AddBorder(zone, Direction.NorthWest);
    AddBorder(zone, Direction.NorthEast);
    AddBorder(zone, Direction.SouthWest);
    AddBorder(zone, Direction.SouthEast);
    return true;
  }
  private void AddBorder(Vector2i zone, Direction direction)
  {
    if (direction == Direction.North) zone.y -= 1;
    if (direction == Direction.East) zone.x -= 1;
    if (direction == Direction.South) zone.y += 1;
    if (direction == Direction.West) zone.x += 1;
    if (direction == Direction.NorthWest)
    {
      zone.y -= 1;
      zone.x += 1;
    }
    if (direction == Direction.NorthEast)
    {
      zone.y -= 1;
      zone.x -= 1;
    }
    if (direction == Direction.SouthWest)
    {
      zone.y += 1;
      zone.x += 1;
    }
    if (direction == Direction.SouthEast)
    {
      zone.y += 1;
      zone.x -= 1;
    }
    if (BorderZones.ContainsKey(zone)) direction |= BorderZones[zone];
    BorderZones[zone] = direction;
  }

  protected override void OnEnd()
  {
    var text = $"{Operation} completed. {Reseted} zones reseted.";
    if (Failed > 0) text += " " + Failed + " errors.";
    Print(text);
    BorderZones = BorderZones.Where(kvp => ZoneSystem.instance.IsZoneGenerated(kvp.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    new ResetBorder(Context, BorderZones);
    ClutterSystem.instance?.ClearAll();
    Helper.RecalculateTerrain();
    Minimap.instance?.UpdateLocationPins(1000);
  }
}
