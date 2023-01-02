using System;
using System.Collections.Generic;
namespace UpgradeWorld;

[Flags]
public enum Direction { None, North, East, South = 4, West = 8, NorthEast = 16, SouthEast = 32, SouthWest = 64, NorthWest = 128 }

/// <summary>Destroys everything in a zone so that the world generator can regenerate it.</summary>
public class ResetBorder : EntityOperation
{
  public ResetBorder(Terminal context, Dictionary<Vector2i, Direction> zones) : base(context)
  {
    Execute(zones);
  }
  private void Execute(Dictionary<Vector2i, Direction> zones)
  {
    var zdos = GetZDOs(Settings.TerrainCompilerId);
    var zs = ZoneSystem.instance;
    var reseted = 0;
    foreach (var zdo in zdos)
    {
      var zone = zs.GetZone(zdo.GetPosition());
      if (!zones.ContainsKey(zone)) continue;
      Update(zdo, zones[zone]);
      reseted += 1;
    }
    Print($"{reseted} border zones reseted");
  }

  private void Update(ZDO zdo, Direction direction)
  {
    var byteArray = zdo.GetByteArray("TCData");
    if (byteArray == null)
      return;
    var change = false;
    var from = new ZPackage(Utils.Decompress(byteArray));
    var to = new ZPackage();
    to.Write(from.ReadInt());
    to.Write(from.ReadInt() + 1);
    to.Write(from.ReadVector3());
    to.Write(from.ReadSingle());
    var size = from.ReadInt();
    to.Write(size);
    var width = (int)Math.Sqrt(size);
    for (int index = 0; index < size; index++)
    {
      var wasModified = from.ReadBool();
      var modified = wasModified;
      var j = index / width;
      var i = index % width;
      if (direction.HasFlag(Direction.North) && j == width - 1)
        modified = false;
      if (direction.HasFlag(Direction.East) && i == width - 1)
        modified = false;
      if (direction.HasFlag(Direction.South) && j == 0)
        modified = false;
      if (direction.HasFlag(Direction.West) && i == 0)
        modified = false;
      if (direction.HasFlag(Direction.NorthEast) && j == width - 1 && i == width - 1)
        modified = false;
      if (direction.HasFlag(Direction.NorthWest) && j == width - 1 && i == 0)
        modified = false;
      if (direction.HasFlag(Direction.SouthWest) && j == 0 && i == 0)
        modified = false;
      if (direction.HasFlag(Direction.SouthEast) && j == 0 && i == width - 1)
        modified = false;
      to.Write(modified);
      if (modified)
      {
        to.Write(from.ReadSingle());
        to.Write(from.ReadSingle());
      }
      if (wasModified && !modified)
      {
        change = true;
        from.ReadSingle();
        from.ReadSingle();
      }
    }
    size = from.ReadInt();
    to.Write(size);
    for (int index = 0; index < size; index++)
    {
      var modified = from.ReadBool();
      to.Write(modified);
      if (modified)
      {
        to.Write(from.ReadSingle());
        to.Write(from.ReadSingle());
        to.Write(from.ReadSingle());
        to.Write(from.ReadSingle());
      }
    }
    var bytes = Utils.Compress(to.GetArray());
    if (!change) return;
    if (!zdo.IsOwner())
      zdo.SetOwner(ZDOMan.instance.GetMyID());
    zdo.Set("TCData", bytes);
  }

}
