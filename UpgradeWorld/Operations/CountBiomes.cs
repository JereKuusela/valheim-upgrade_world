using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UpgradeWorld {
  /// <summary>Counts the amounts of biomes within a given distance.</summary>
  public class CountBiomes : BaseOperation {
    public CountBiomes(Terminal context, float frequency, FiltererParameters args) : base(context) {
      Count(frequency, args);
    }

    private void Count(float frequency, FiltererParameters args) {
      if (args.MaxDistance == 0) args.MaxDistance = 10500f;
      var biomes = new Dictionary<Heightmap.Biome, int>();
      var start = (float)Math.Ceiling(args.MaxDistance / frequency) * frequency;
      for (var x = start; x <= args.MaxDistance; x += frequency) {
        for (var y = start; y <= args.MaxDistance; y += frequency) {
          var distance = new Vector2(x - args.X, y - args.Y).magnitude;
          if (distance < args.MinDistance) continue;
          if (distance > args.MaxDistance) continue;
          var biome = WorldGenerator.instance.GetBiome(x, y);
          if (!args.IsBiomeValid(biome)) continue;
          if (!biomes.ContainsKey(biome)) biomes[biome] = 0;
          biomes[biome]++;
        }
      }
      float total = biomes.Values.Sum();
      var text = biomes.Select(kvp => kvp.Key + ": " + kvp.Value + "/" + total + " (" + (kvp.Value / total).ToString("P2") + ")");
      Log(text);
    }
  }
}