using UnityEngine;

namespace UpgradeWorld {
  public abstract class Generate : ZoneOperation {
    public Generate(Terminal context, ZoneFilterer[] filterers) : base(context, Zones.GetWorldZones(), filterers) {
      Operation = "Generate";
    }

    protected override bool ExecuteZone(Vector2i zone) {
      var zoneSystem = ZoneSystem.instance;
      return zoneSystem.PokeLocalZone(zone);
    }

    protected override void OnEnd() {
      var generated = ZonesToUpgrade.Length - Failed;
      Print("Generate completed. " + generated + " zones generated. " + Failed + " errors.");
    }
  }

  public class GenerateBiomes : Generate {
    public GenerateBiomes(string[] biomes, bool includeEdges, Terminal context) : base(context, new ZoneFilterer[] { new BiomeFilterer(biomes, includeEdges), new ConfigFilterer() }) {
    }
  }
  public class GenerateAdjacent : Generate {
    public GenerateAdjacent(Vector2i center, int adjacent, Terminal context) : base(context, new ZoneFilterer[] { new AdjacencyFilterer(center, adjacent) }) {
    }
  }
  public class GenerateIncluded : Generate {
    public GenerateIncluded(Vector3 center, float radius, Terminal context) : base(context, new ZoneFilterer[] { new RangeFilterer(center, radius) }) {
    }
  }
}