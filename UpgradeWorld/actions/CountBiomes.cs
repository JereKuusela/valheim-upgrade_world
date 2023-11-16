using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
namespace UpgradeWorld;
/// <summary>Counts the amounts of biomes within a given distance.</summary>
public class CountBiomes : BaseOperation
{
  public CountBiomes(Terminal context, float frequency, FiltererParameters args) : base(context)
  {
    Count(frequency, args);
  }

  private void Count(float frequency, FiltererParameters args)
  {
    if (!args.Pos.HasValue) return;
    if (args.MaxDistance == 0) args.MaxDistance = Settings.WorldRadius;
    Dictionary<Heightmap.Biome, int> biomes = [];
    var start = -(float)Math.Ceiling(args.MaxDistance / frequency) * frequency;
    for (var x = start; x <= args.MaxDistance; x += frequency)
    {
      for (var y = start; y <= args.MaxDistance; y += frequency)
      {
        var distance = new Vector2(x, y).magnitude;
        if (distance < args.MinDistance) continue;
        if (distance > args.MaxDistance) continue;
        var biome = WorldGenerator.instance.GetBiome(args.Pos.Value.x + x, args.Pos.Value.y + y);
        if (!args.IsBiomeValid(biome)) continue;
        if (!biomes.ContainsKey(biome)) biomes[biome] = 0;
        biomes[biome]++;
      }
    }
    float total = biomes.Values.Sum();
    var text = biomes.OrderBy(kvp => Enum.GetName(typeof(Heightmap.Biome), kvp.Key)).Select(kvp => Enum.GetName(typeof(Heightmap.Biome), kvp.Key) + ": " + kvp.Value + "/" + total + " (" + (kvp.Value / total).ToString("P2", CultureInfo.InvariantCulture) + ")");
    Print(string.Join("\n", text));
  }
}
