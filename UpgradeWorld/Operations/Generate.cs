using UnityEngine;

namespace UpgradeWorld {
  public abstract class Generate : ZoneOperation {
    public Generate(Terminal context, ZoneFilterer filterer) : base(context, Zones.GetWorldZones(), filterer) {
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

    protected override bool NeedsOperation(Vector2i zone) {
      var zoneSystem = ZoneSystem.instance;
      return !zoneSystem.IsZoneGenerated(zone);
    }
  }

  public class GenerateAll : Generate {
    public GenerateAll(Terminal context) : base(context, new AllZones()) {
    }
  }
  public class GenerateAdjacent : Generate {
    public GenerateAdjacent(Vector2i center, int adjacent, Terminal context) : base(context, new AdjacentZones(center, adjacent)) {
    }
  }
  public class GenerateIncluded : Generate {
    public GenerateIncluded(Vector3 center, float radius, Terminal context) : base(context, new IncludedZones(center, radius)) {
    }
  }
}