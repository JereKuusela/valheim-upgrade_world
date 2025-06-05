# Developers

Mods that add new content should implement their own upgrade commands.

Add a new file to your project `UpgradeWorld.cs`

```cs
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Bootstrap;
using HarmonyLib;

namespace UpgradeWorld;

public class CommandRegistration
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

public static class Upgrade
{
  private static List<CommandRegistration> registrations = new();

  public const string GUID = "upgrade_world";
  private static bool Patched = false;
  public static void Register(string name, string description, params string[] commands)
  {
    if (!Chainloader.PluginInfos.ContainsKey(GUID)) return;
    PatchIfNeeded();
    registrations.Add(new() { name = name, description = description, commands = commands });
  }
  private static void PatchIfNeeded()
  {
    if (Patched) return;
    Patched = true;
    Harmony harmony = new("helpers.upgrade_world");
    var toPatch = AccessTools.Method(typeof(Terminal), nameof(Terminal.InitTerminal));
    var postfix = AccessTools.Method(typeof(Upgrade), nameof(AddCommands));
    harmony.Patch(toPatch, postfix: new HarmonyMethod(postfix));
  }

  static void AddCommands()
  {
    foreach (var registration in registrations)
      registration.AddCommand();
  }
}
```

Then to your plugin add

```cs
public void Start()
{
  UpgradeWorld.Upgrade.Register("MOD_locations", "Adds locations of mod MOD.", "locations_add location1,location2,location3 start");
  UpgradeWorld.Upgrade.Register("MOD_vegetation", "Adds vegetations of mod MOD.", "vegetation_reset vegetation1,vegetation2,vegetation3 start");
  UpgradeWorld.Upgrade.Register("MOD_upgrade", "Adds locations and vegetations of mod MOD.", "locations_add location1,location2,location3 start", "vegetation_reset vegetation1,vegetation2,vegetation3 start");
}
```
