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
      var zone = zonesToUpgrade[zoneIndex];
      UpdateConsole(zone);
      var success = false;
      if (operation == "upgrade") {
        success = Upgrade(zone);
      }
      if (operation == "nuke") {
        success = NukeUnloaded(zone);
      }
      MoveToNextZone(success);
      if (zoneIndex == zonesToUpgrade.Length) {
        zonesToUpgrade = new Vector2i[0];
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
    private static void MoveToNextZone(bool success) {
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