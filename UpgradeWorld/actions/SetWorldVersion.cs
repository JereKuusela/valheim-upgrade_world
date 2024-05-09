namespace UpgradeWorld;
public class WorldVersion(Terminal context, int version, bool start) : ExecutedOperation(context, start)
{
  readonly int Version = version;

  protected override bool OnExecute()
  {
    var world = WorldGenerator.instance.m_world;
    world.m_worldGenVersion = Version;
    WorldGenerator.Initialize(world);
    Minimap.instance?.ForceRegen();
    Print($"Updated world version to {world.m_worldGenVersion}.");
    return true;
  }

  protected override string OnInit()
  {
    return $"Updating world version to {Version}. This may change biome distribution.";
  }
}
