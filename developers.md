Mods that add new content should implement their own upgrade commands.

Add a new file to your project `UpgradeWorld.cs`

```
using System.Collections.Generic;
using BepInEx.Bootstrap;
using HarmonyLib;

namespace UpgradeWorld;

private class CommandRegistration
{
  public string name = "";
  public string description = "";
  public string[] commands = new string[0];

  public void AddCommand()
  {
    new Console.ConsoleCommand(name, description, (args) =>
    {
     foreach (var command in commands)
       args.Context.TryRunCommand(command);
    });
  }
}

[HarmonyPatch(typeof(Terminal), nameof(Terminal.InitTerminal))]
public static class Upgrade
{
  private static List<CommandRegistration> registrations = new();
  
  public const string GUID = "upgrade_world";
  public static void Register(string name, string description, params string[] commands)
  {
    if (!Chainloader.PluginInfos.ContainsKey(GUID)) return;
    registrations.Add(new() { name = name, description = description, commands = commands });
  }
  
  static void Postfix()
  {
    foreach (var registration in registrations)
      registration.AddCommand();
  }
}
```

Then to your plugin add

```
public void Start()
{
  UpgradeWorld.Upgrade.Register("MOD_locations", "Adds locations of mod MOD.", "locations_add location1,location2,location3 start");
  UpgradeWorld.Upgrade.Register("MOD_vegetation", "Adds vegetations of mod MOD.", "vegetation_reset vegetation1,vegetation2,vegetation3 start");
  UpgradeWorld.Upgrade.Register("MOD_upgrade", "Adds locations and vegetations of mod MOD.", "locations_add location1,location2,location3 start", "vegetation_reset vegetation1,vegetation2,vegetation3 start");
}
```
