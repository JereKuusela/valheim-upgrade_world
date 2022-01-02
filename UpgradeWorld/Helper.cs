using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace UpgradeWorld {
  public static class Helper {
    public static string Normalize(string value) => value.Trim().ToLower();
    public static string JoinRows(IEnumerable<string> values) => string.Join(", ", values);

    public static bool ParseInt(string arg, out int number) => int.TryParse(arg, NumberStyles.Integer, CultureInfo.InvariantCulture, out number);
    public static bool TryParseFloat(string arg, out float number) => float.TryParse(arg, NumberStyles.Float, CultureInfo.InvariantCulture, out number);
    public static bool IsFloat(string arg) => float.TryParse(arg, NumberStyles.Float, CultureInfo.InvariantCulture, out var _);
    public static float ParseFloat(string arg) => float.Parse(arg, NumberStyles.Float, CultureInfo.InvariantCulture);
    public static IEnumerable<string> ParseArgs(IEnumerable<string> args, int skip) => string.Join(",", args.Skip(skip)).Split(',').Select(arg => arg.Trim()).Where(arg => arg != "");
    public static List<string> AvailableBiomes = new List<string>{
      "AshLands", "BlackForest", "DeepNorth", "Meadows", "Mistlands", "Mountain", "Ocean", "Plains", "Swamp"
    };
    /// <summary>Converts a biome name to a biome.</summary>
    public static Heightmap.Biome GetBiome(string name) {
      name = Helper.Normalize(name);
      if (name == "ashlands") return Heightmap.Biome.AshLands;
      if (name == "blackforest") return Heightmap.Biome.BlackForest;
      if (name == "deepnorth") return Heightmap.Biome.DeepNorth;
      if (name == "meadows") return Heightmap.Biome.Meadows;
      if (name == "mistlands") return Heightmap.Biome.Mistlands;
      if (name == "mountain") return Heightmap.Biome.Mountain;
      if (name == "ocean") return Heightmap.Biome.Ocean;
      if (name == "plains") return Heightmap.Biome.Plains;
      if (name == "swamp") return Heightmap.Biome.Swamp;
      return Heightmap.Biome.None;
    }
    public static IEnumerable<string> ParseFiltererArgs(IEnumerable<string> args, FiltererParameters parameters) {
      var parsed = ParseArgs(args, 1);
      parsed = ParseNamedInt(parsed, "safezones", ref parameters.SafeZones);
      parsed = ParseFlag(parsed, "zones", out parameters.MeasureWithZones);
      parsed = ParseFlag(parsed, "noedges", out parameters.NoEdges);
      parsed = ParseFlag(parsed, "force", out parameters.ForceStart);
      var other = parsed.Where(arg => !TryParseFloat(arg, out var _));
      var allNumbers = parsed.Where(arg => TryParseFloat(arg, out var _)).Select(ParseFloat);
      var ranges = other.Where(arg => arg.Split('-').Length == 2 && arg.Split('-').All(IsFloat));
      other = other.Where(arg => arg.Split('-').Length != 2 || !arg.Split('-').All(IsFloat));
      var range = ranges.FirstOrDefault();
      // Add back unused ranges.
      other.ToList().AddRange(ranges.Skip(1));
      if (range != null) {
        var split = range.Split('-').Select(ParseFloat);
        parameters.MinDistance = split.First();
        parameters.MaxDistance = split.Last();
      }
      var numbers = allNumbers.Take(range == null ? 3 : 2);
      // Add back unused numbers.
      other.ToList().AddRange(allNumbers.Skip(numbers.Count()).Select(arg => arg.ToString(CultureInfo.InvariantCulture)));
      if (numbers.Count() == 0 && parameters.MeasureWithZones) {
        var zone = ZoneSystem.instance.GetZone(Player.m_localPlayer.transform.position);
        parameters.X = zone.x;
        parameters.Y = zone.y;
      }
      if (numbers.Count() == 1 || numbers.Count() == 3) {
        var distance = numbers.First();
        parameters.MinDistance = 0;
        parameters.MaxDistance = 0;
        if (distance > 0)
          parameters.MinDistance = distance;
        else
          parameters.MaxDistance = -distance;
        if (numbers.Count() == 3) {
          parameters.X = numbers.Skip(1).First();
          parameters.Y = numbers.Last();
        } else {
          if (parameters.MeasureWithZones) {
            var zone = ZoneSystem.instance.GetZone(Player.m_localPlayer.transform.position);
            parameters.X = zone.x;
            parameters.Y = zone.y;
          } else {
            parameters.X = Player.m_localPlayer.transform.position.x;
            parameters.Y = Player.m_localPlayer.transform.position.z;

          }
        }
      }
      if (numbers.Count() == 2) {
        parameters.MinDistance = 0;
        parameters.MaxDistance = 0;
        parameters.X = numbers.First();
        parameters.Y = numbers.Last();
      }

      parameters.Biomes = other.Select(GetBiome).Where(biome => biome != Heightmap.Biome.BiomesMax && biome != Heightmap.Biome.None).ToHashSet();
      other = other.Where(arg => GetBiome(arg) == Heightmap.Biome.None);
      parameters.TargetZones = TargetZones.Generated;
      return other;
    }

    public static bool ParseIncludedArgs(Terminal.ConsoleEventArgs args, out float x, out float z, out float distance) {
      x = 0;
      z = 0;
      distance = 0;
      if (args.Length < 2) {
        args.Context.AddString("Error: Missing coordinate X");
        return false;
      }
      if (args.Length < 3) {
        args.Context.AddString("Error: Missing coordinate Z");
        return false;
      }
      if (!TryParseFloat(args[1], out x)) {
        args.Context.AddString("Error: Invalid format for X coordinate.");
        return false;
      }
      if (!TryParseFloat(args[2], out z)) {
        args.Context.AddString("Error: Invalid format for Z coordinate.");
        return false;
      }
      if (args.Length > 3) {
        if (!TryParseFloat(args[3], out distance)) {
          args.Context.AddString("Error: Invalid format for distance.");
          return false;
        }
      }
      return true;
    }
    public static IEnumerable<string> ParseFlag(IEnumerable<string> parameters, string flag, out bool value) {
      value = parameters.FirstOrDefault(arg => arg.ToLower() == flag) != null;
      return parameters.Where(arg => arg.ToLower() != flag);
    }
    public static IEnumerable<string> ParseNamedInt(IEnumerable<string> parameters, string key, ref int value) {
      var arg = parameters.FirstOrDefault(arg => arg.ToLower().StartsWith(key + "="));
      if (arg != null && arg != "") {
        var split = arg.Split('=');
        if (split.Length > 0 && ParseInt(split[1], out var number))
          value = number;
      }
      return parameters.Where(arg => !arg.ToLower().StartsWith(key + "="));
    }

    public static void RemoveZDO(ZDO zdo) {
      if (ZNetScene.instance.m_instances.TryGetValue(zdo, out var view)) {
        ZNetScene.instance.Destroy(view.gameObject);
      } else {
        if (!zdo.IsOwner())
          zdo.SetOwner(ZDOMan.instance.GetMyID());
        ZDOMan.instance.DestroyZDO(zdo);
      }
    }
  }
}