using System.Collections.Generic;
using UnityEngine;

namespace UpgradeWorld {
  public abstract class Generate : ZoneOperation {
    public Generate(Terminal context, ZoneFilterer[] filterers) : base(context, filterers, TargetZones.Ungenerated) {
      Operation = "Generate";
    }

    protected override bool ExecuteZone(Vector2i zone) {
      var zoneSystem = ZoneSystem.instance;
      return zoneSystem.PokeLocalZone(zone);
    }

    protected override void OnEnd() {
      var generated = ZonesToUpgrade.Length - Failed;
      var text = Operation + " completed.";
      if (Settings.Verbose) text += " " + generated + " zones generated.";
      if (Failed > 0) text += " " + Failed + " errors.";
      Print(text);
    }
  }

  public class GenerateBiomes : Generate {
    public GenerateBiomes(IEnumerable<Heightmap.Biome> biomes, bool includeEdges, Terminal context) : base(context, new ZoneFilterer[] { new BiomeFilterer(biomes, includeEdges), new ConfigFilterer() }) {
      InitString = "Generate " + (includeEdges ? "" : "center ") + "zones in biomes " + string.Join(", ", biomes);
    }
  }
  public class GenerateAdjacent : Generate {
    public GenerateAdjacent(Vector2i center, int adjacent, Terminal context) : base(context, new ZoneFilterer[] { new AdjacencyFilterer(center, adjacent) }) {
      InitString = "Generate zones within " + adjacent + " zones from " + center.x + ", " + center.y;
    }
  }
  public class GenerateIncluded : Generate {
    public GenerateIncluded(Vector3 center, float distance, Terminal context) : base(context, new ZoneFilterer[] { new RangeFilterer(center, distance) }) {
      InitString = "Generate zones within " + distance + " meters from " + center.x + ", " + center.z;
    }
  }
}