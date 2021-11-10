using System.Collections.Generic;

namespace UpgradeWorld {
  public static class Helper {
    public static string Normalize(string value) => value.Trim().ToLower();
    public static string JoinRows(IEnumerable<string> values) => string.Join(", ", values);
  }
}