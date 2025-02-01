

using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
namespace UpgradeWorld;
public struct FilterPoint
{
  public float x;
  public float y;
  public float min;
  public float max;
}
public static class Settings
{
#nullable disable
  public static ConfigEntry<bool> configDisableAutomaticGenloc;
  public static bool DisableAutomaticGenloc => configDisableAutomaticGenloc.Value;
  public static ConfigEntry<bool> configVerbose;
  public static bool Verbose => configVerbose.Value;
  public static ConfigEntry<bool> configAutoStart;
  public static bool AutoStart => configAutoStart.Value;
  public static ConfigEntry<int> configWorldRadius;
  public static int WorldRadius => configWorldRadius.Value;
  public static ConfigEntry<int> configWorldEdge;
  public static int WorldEdge => configWorldEdge.Value;
  public static ConfigEntry<string> configSafeZoneItems;
  public static HashSet<int> SafeZoneItems => [.. configSafeZoneItems.Value.Split(',').Select(name => name.Trim().GetStableHashCode())];
  public static ConfigEntry<string> configSafeZoneObjects;
  public static HashSet<int> SafeZoneObjects => [.. configSafeZoneObjects.Value.Split(',').Select(name => name.Trim().GetStableHashCode())];
  public static ConfigEntry<int> configSafeZoneSize;
  public static int SafeZoneSize => configSafeZoneSize.Value;
  public static ConfigEntry<int> configThrottle;
  public static int Throttle => configThrottle.Value;
  public static ConfigEntry<int> configDestroysPerUpdate;
  public static int DestroysPerUpdate => configDestroysPerUpdate.Value;
  public static ConfigEntry<string> configTimeBasedDataNames;
  public static IEnumerable<string> TimeBasedDataNames => configTimeBasedDataNames.Value.Split(',').Select(name => name.Trim());
  public static int ZoneControlHash = "_ZoneCtrl".GetStableHashCode();

  public static int TerrainCompilerHash = "_TerrainCompiler".GetStableHashCode();
  public static ConfigEntry<string> configRootUsers;
  private static HashSet<string> RootUsers = [];
#nullable enable
  private static void UpdateRootUsers() => RootUsers = [.. configRootUsers.Value.Split(',').Select(s => s.Trim()).Where(s => s != "")];
  public static bool IsRoot(string id)
  {
    if (RootUsers.Count == 0) return id == "-1" || ZNet.instance.ListContainsId(ZNet.instance.m_adminList, id);
    if (RootUsers.Contains(id)) return true;
    if (id.StartsWith(PrivilegeManager.GetPlatformPrefix(PrivilegeManager.Platform.Steam)))
      return RootUsers.Contains(id.Substring(PrivilegeManager.GetPlatformPrefix(PrivilegeManager.Platform.Steam).Length));
    if (!id.Contains("_"))
      return RootUsers.Contains(PrivilegeManager.GetPlatformPrefix(PrivilegeManager.Platform.Steam) + id);
    return false;
  }

  public static void Init(ConfigFile config)
  {
    var section = "1. General";
    configVerbose = config.Bind(section, "Verbose output", false, "If true, more detailed is printed (useful for debugging but may contain spoilers).");
    configAutoStart = config.Bind(section, "Automatic start", false, "If true, operations start automatically without having to use the start command.");
    configWorldRadius = config.Bind(section, "World radius", 10500, "Max radius for operations.");
    configWorldEdge = config.Bind(section, "World edge", 500, "Size of world edge.");
    configWorldRadius.SettingChanged += (sender, args) => Zones.ResetAllZones();
    configSafeZoneItems = config.Bind(section, "Safe zone items", "blastfurnace,bonfire,charcoal_kiln,fermenter,fire_pit,forge,guard_stone,hearth,piece_artisanstation,piece_bed02,piece_brazierceiling01,piece_groundtorch,piece_groundtorch_blue,piece_groundtorch_green,piece_groundtorch_wood,piece_oven,piece_spinningwheel,piece_stonecutter,piece_walltorch,piece_workbench,portal,portal_wood,smelter,windmill,piece_chest,piece_chest_blackmetal,piece_chest_private,piece_chest_treasure,piece_chest_wood", "List of player placed objects that prevent zones being modified.");
    configSafeZoneObjects = config.Bind(section, "Safe zone objects", "Player_tombstone", "List of object ids that prevent zones being modified.");
    configSafeZoneSize = config.Bind(section, "Safe zones", 2, "0 = disable, 1 = only the zone, 2 = 3x3 zones, 3 = 5x5 zones, etc.");
    configRootUsers = config.Bind(section, "Root users", "", "SteamIDs that can execute commands on servers (-1 for the dedicated server). If not set, then all admins can execute commands.");
    configRootUsers.SettingChanged += (sender, args) => UpdateRootUsers();
    UpdateRootUsers();
    configThrottle = config.Bind(section, "Operation delay", 100, "Milliseconds between each command. Prevents lots of small operations overloading the dedicated server.");
    configDisableAutomaticGenloc = config.Bind(section, "Disable automatic genloc", false, "If enabled, new content updates won't automatically redistribute locations.");

    configDestroysPerUpdate = config.Bind("2. Destroying", "Operations per update", 100, "How many zones are destroyed per Unity update.");
    configTimeBasedDataNames = config.Bind("3. Change time/day", "Time based data names", "spawntime,lastTime,SpawnTime,StartTime,alive_time,spawn_time,picked_time,plantTime,pregnant,TameLastFeeding", "Names of the data values that should be updated with the new time. Changing these is NOT recommended.");
  }
}
