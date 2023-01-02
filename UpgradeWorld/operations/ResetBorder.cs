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
    var zdos = GetZDOs("_TerrainCompiler");
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
    var from = new ZPackage(Utils.Decompress(byteArray));
    var to = new ZPackage();
    to.Write(from.ReadInt());
    to.Write(from.ReadInt() + 1);
    to.Write(from.ReadVector3());
    to.Write(from.ReadSingle());
    var size = from.ReadInt();
    to.Write(size);
    var width = (int)Math.Sqrt(size);
    for (int i = 0; i < size; i++)
    {
      var wasModified = from.ReadBool();
      var modified = wasModified;
      var x = i / width;
      var y = i % width;
      if (direction.HasFlag(Direction.North) && x == width - 1)
        modified = false;
      if (direction.HasFlag(Direction.East) && y == width - 1)
        modified = false;
      if (direction.HasFlag(Direction.South) && x == 0)
        modified = false;
      if (direction.HasFlag(Direction.West) && y == 0)
        modified = false;
      if (direction.HasFlag(Direction.NorthEast) && x == width - 1 && y == width - 1)
        modified = false;
      if (direction.HasFlag(Direction.NorthWest) && x == width - 1 && y == 0)
        modified = false;
      if (direction.HasFlag(Direction.SouthWest) && x == 0 && y == 0)
        modified = false;
      if (direction.HasFlag(Direction.SouthEast) && x == 0 && y == width - 1)
        modified = false;
      to.Write(modified);
      if (modified)
      {
        to.Write(from.ReadSingle());
        to.Write(from.ReadSingle());
      }
      if (wasModified && !modified)
      {
        from.ReadSingle();
        from.ReadSingle();
      }
    }
    size = from.ReadInt();
    to.Write(size);
    for (int i = 0; i < size; i++)
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
    zdo.Set("TCData", bytes);
  }

}
