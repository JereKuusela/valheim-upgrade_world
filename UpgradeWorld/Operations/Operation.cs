namespace UpgradeWorld {

  public partial class Operation {
    private static Vector2i[] zonesToUpgrade;
    private static int zoneIndex = 0;
    private static int attempts = 0;
    private static int operationsFailed = 0;
    private static int lastInputLength = 0;
    private static string operation = "";
    public static void ProcessOne() {
      if (zonesToUpgrade == null || zonesToUpgrade.Length == 0) {
        return;
      }
      var operations = operation == "nuke" ? Settings.NukesPerUpdate : 1;
      for (var i = 0; i < operations; i++) {
        var zone = zonesToUpgrade[zoneIndex];
        UpdateConsole(zone);
        if (operation == "upgrade") {
          var success = Upgrade(zone);
          MoveToNextZone(success);
        }
        if (operation == "nuke") {
          NukeUnloaded(zone);
          MoveToNextZone();
        }
        if (zoneIndex >= zonesToUpgrade.Length) {
          break;
        }
      }
      if (zoneIndex >= zonesToUpgrade.Length) {
        zonesToUpgrade = new Vector2i[0];
        if (operation == "nuke") {
          Console.instance.Print("Zones destroyed. Run genloc to re-distribute location instances.");
        }

      }
    }
    public static void SetOperation(string operationToDo, Vector2i[] zones) {
      zonesToUpgrade = zones;
      zoneIndex = 0;
      lastInputLength = 0;
      operationsFailed = 0;
      attempts = 0;
      operation = operationToDo;
    }
    private static void MoveToNextZone(bool success = true) {
      if (success) {
        attempts = 0;
        zoneIndex++;
      } else {
        attempts++;
        if (attempts > 10) {
          operationsFailed++;
          attempts = 0;
          zoneIndex++;
        }
      }
    }
    private static void UpdateConsole(Vector2i zone) {
      var totalString = zonesToUpgrade.Length.ToString();
      var updatedString = (zoneIndex + 1 - operationsFailed).ToString().PadLeft(totalString.Length, ' ');
      var input = operation + ": " + updatedString + "/" + totalString + " (" + zone.ToString() + ")";
      var newText = Console.instance.m_output.text.Substring(0, Console.instance.m_output.text.Length - lastInputLength) + input;
      Console.instance.m_output.text = newText;
      lastInputLength = input.Length;
    }

  }
}