namespace UpgradeWorld {

  public enum Operation {
    Upgrade,
    UpgradeInit,
    Destroy,
    Generate,
    Query,
    None
  }

  public partial class Operations {
    private static Vector2i[] zonesToUpgrade;
    private static int zoneIndex = 0;
    private static int attempts = 0;
    private static int skipped = 0;
    private static int failed = 0;
    private static Operation operation = Operation.None;
    private static Terminal context = null;


    public static string GetName(Operation operation) {
      if (operation == Operation.Destroy) return "Regenerate";
      if (operation == Operation.Upgrade) return "Upgrade";
      if (operation == Operation.Query) return "Operate";
      return "Unknown operation";
    }
    public static void Process() {
      if (operation == Operation.UpgradeInit) {
        if (attempts == 0) {
          context.AddString("Redistributing locations to old areas. This may take a while...");
          attempts++;
        } else {
          RedistributeLocations(true, true);
          SetOperation(context, Operation.Upgrade, zonesToUpgrade);
        }
      } else {
        ProcessZones();
      }
    }
    private static void ProcessZones() {
      if (zonesToUpgrade == null || zonesToUpgrade.Length == 0) {
        return;
      }
      var operations = operation == Operation.Destroy ? Settings.DestroysPerUpdate : 1;
      for (var i = 0; i < operations; i++) {
        if (GetNextZone(out var zone)) {
          if (operation == Operation.Upgrade) {
            var success = Upgrade(zone);
            MoveToNextZone(success);
          }
          if (operation == Operation.Generate) {
            var success = Generate(zone);
            MoveToNextZone(success);
          }
          if (operation == Operation.Destroy) {
            RegenerateZone(zone);
            MoveToNextZone();
          }
        } else {
          break;
        }
      }
      UpdateConsole();
      if (zoneIndex >= zonesToUpgrade.Length) {
        if (operation == Operation.Upgrade) {
          var upgraded = zonesToUpgrade.Length - skipped - failed;
          context.AddString("Upgrade completed. " + upgraded + " zones upgraded. " + skipped + " skipped. " + failed + " errors.");
        }
        if (operation == Operation.Destroy) {
          context.AddString("Zones destroyed. Run place or genloc to re-distribute the location instances.");
        }
        if (operation == Operation.Generate) {
          var generated = zonesToUpgrade.Length - skipped - failed;
          context.AddString("Generate completed. " + generated + " zones generated. " + skipped + " skipped. " + failed + " errors.");
        }
        zonesToUpgrade = new Vector2i[0];
      }
    }
    public static void SetOperation(Terminal terminal, Operation operationToDo, Vector2i[] zones) {
      context = terminal;
      zonesToUpgrade = zones;
      zoneIndex = 0;
      failed = 0;
      attempts = 0;
      skipped = 0;
      operation = operationToDo;
    }
    ///<summary>Returns the next zone that needs operating.</summary>
    private static bool GetNextZone(out Vector2i zone) {
      if (zoneIndex >= zonesToUpgrade.Length) {
        zone = new Vector2i();
        return false;
      }

      zone = zonesToUpgrade[zoneIndex];
      if (operation == Operation.Destroy) {
        return true;
      }
      if (operation == Operation.Generate) {
        while (!NeedsGenerating(zone)) {
          MoveToNextZone();
          skipped++;
          if (zoneIndex >= zonesToUpgrade.Length) {
            return false;
          }
          zone = zonesToUpgrade[zoneIndex];
        }
        return true;
      }
      while (!NeedsUpgrade(zone)) {
        MoveToNextZone();
        skipped++;
        if (zoneIndex >= zonesToUpgrade.Length) {
          return false;
        }
        zone = zonesToUpgrade[zoneIndex];
      }
      return true;
    }
    private static void MoveToNextZone(bool success = true) {
      if (success) {
        attempts = 0;
        zoneIndex++;
      } else {
        attempts++;
        if (attempts > 100) {
          failed++;
          attempts = 0;
          zoneIndex++;
        }
      }
    }
    private static void UpdateConsole() {
      var totalString = zonesToUpgrade.Length.ToString();
      var updatedString = zoneIndex.ToString().PadLeft(totalString.Length, '0');
      var text = operation + ": " + updatedString + "/" + totalString;
      context.AddString(text);
    }
  }
}