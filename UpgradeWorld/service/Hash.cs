namespace Service;

public static class Hash {
  public static int Changed = "override_changed".GetStableHashCode();
  public static int OverrideItems = "override_items".GetStableHashCode();
}