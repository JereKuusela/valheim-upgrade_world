namespace Service;

public static class Hash
{
  public static int Level = "level".GetStableHashCode();
  public static int Location = "location".GetStableHashCode();
  public static int AddedDefaultItems = "addedDefaultItems".GetStableHashCode();
  public static int Items = "items".GetStableHashCode();
}