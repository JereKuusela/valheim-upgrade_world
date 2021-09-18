namespace UpgradeWorld {

  public partial class Operation {
    private static Vector2i[] zonesToUpgrade;
    private static int zoneIndex = 0;
    private static int attempts = 0;
    private static int operationsFailed = 0;
    private static string operation = "";

    public static void Process() {
      if (operation == "upgrade_init") {
        if (attempts == 0) {
          Console.instance.Print("Redistributing locations to old areas. This may take a while...");
          attempts++;
        } else {
          RedistributeLocations();
          SetOperation("upgrade", zonesToUpgrade);
        }
      } else {
        ProcessZones();
      }
    }
    private static void ProcessZones() {
      if (zonesToUpgrade == null || zonesToUpgrade.Length == 0) {
        return;
      }
      var operations = operation == "nuke" ? Settings.NukesPerUpdate : 1;
      for (var i = 0; i < operations; i++) {
        if (GetNextZone(out var zone)) {
          if (operation == "upgrade") {
            var success = Upgrade(zone);
            MoveToNextZone(success);
          }
          if (operation == "nuke") {
            NukeUnloaded(zone);
            MoveToNextZone();
          }
        } else {
          break;
        }
      }
      UpdateConsole();
      if (zoneIndex >= zonesToUpgrade.Length) {
        zonesToUpgrade = new Vector2i[0];
        if (operation == "upgrade") {
          Console.instance.Print("Upgrade completed. " + operationsFailed + " failures.");
        }
        if (operation == "nuke") {
          Console.instance.Print("Zones destroyed. Run genloc to re-distribute location instances.");
        }

      }
    }
    public static void SetOperation(string operationToDo, Vector2i[] zones) {
      zonesToUpgrade = zones;
      zoneIndex = 0;
      operationsFailed = 0;
      attempts = 0;
      operation = operationToDo;
    }
    ///<summary>Returns the next zone that needs operating.</summary>
    private static bool GetNextZone(out Vector2i zone) {
      if (zoneIndex >= zonesToUpgrade.Length) {
        zone = new Vector2i();
        return false;
      }

      zone = zonesToUpgrade[zoneIndex];
      if (operation == "nuke") {
        return true;
      }

      while (!NeedsUpgrade(zone)) {
        MoveToNextZone();
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
          operationsFailed++;
          attempts = 0;
          zoneIndex++;
        }
      }
    }
    private static void UpdateConsole() {
      var totalString = zonesToUpgrade.Length.ToString();
      var updatedString = zoneIndex.ToString().PadLeft(totalString.Length, '0');
      var text = operation + ": " + updatedString + "/" + totalString;
      Console.instance.Print(text);
    }
  }
}