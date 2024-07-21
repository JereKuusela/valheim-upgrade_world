
namespace UpgradeWorld;
/// <summary>Optimizes old dungeons.</summary>
public class CleanDungeons : EntityOperation
{
  public CleanDungeons(Terminal context, ZDO[] zdos, bool pin) : base(context, pin)
  {
    Clean(zdos);
  }

  private void Clean(ZDO[] zdos)
  {
    var updated = 0;
    foreach (var zdo in zdos)
    {
      var rooms = zdo.GetInt(ZDOVars.s_rooms);
      if (rooms == 0) continue;
      AddPin(zdo.m_position);
      ZDOMan.instance.ConvertDungeonRooms([zdo]);
      Print($"Optimized dungeon at {Helper.PrintVectorXZY(zdo.GetPosition())}.");
      updated++;
    }
    if (updated > 0)
      Print($"Optimized {updated} dungeon{S(updated)}.");

  }
}
