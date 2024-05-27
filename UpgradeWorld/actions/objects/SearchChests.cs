using System.Collections.Generic;
using System.Linq;
using Service;

namespace UpgradeWorld;
/// <summary>Searchs objects from chests.</summary>
public class SearchChests : EntityOperation
{
  public SearchChests(Terminal context, IEnumerable<string> ids, DataParameters args) : base(context, args.Pin)
  {
    Search(ids, args);
  }
  private string SearchStand(ZDO zdo, string prefix, HashSet<int> ids)
  {
    var item = zdo.GetString(prefix + "item", "");
    if (item == "") return "";
    if (!ids.Contains(item.GetStableHashCode())) return "";
    var variant = zdo.GetInt(prefix + "variant");
    var quality = zdo.GetInt(prefix + "quality");
    if (variant > 1) item += ", style " + variant + "";
    if (quality > 1) item += ", level " + quality + "";
    return item;
  }
  private void Search(IEnumerable<string> ids, DataParameters args)
  {
    var prefabs = GetPrefabs(ids, args.Types);
    var zdos = GetZDOs(args);

    var zs = ZNetScene.instance;
    string[] prefixes = ["", "0_", "1_", "2_", "3_", "4_", "5_", "6_", "7_", "8_", "9_"];
    var standContents = zdos.Select(zdo =>
    {
      var content = prefixes.Select(prefix => SearchStand(zdo, prefix, prefabs)).Where(x => x != "").ToList();
      if (content.Count == 0) return "";
      var name = zs.m_namedPrefabs[zdo.m_prefab].name;
      var id = name + " " + zdo.m_uid.ID + " " + Helper.PrintVectorXZY(zdo.GetPosition());
      return id + "\n" + string.Join("\n", content);
    }).Where(x => x != "").ToList();

    if (args.Log) Log(standContents);
    else Print(standContents, false);

    var chestContents = zdos.Select(zdo =>
    {
      var items = zdo.GetString(ZDOVars.s_items);
      if (items == "") return "";
      ZPackage loadPackage = new(zdo.GetString(ZDOVars.s_items));
      var content = SearchChest(loadPackage, prefabs);
      if (content.Count == 0) return "";
      AddPin(zdo.GetPosition());
      var name = zs.m_namedPrefabs[zdo.m_prefab].name;
      var id = name + " " + zdo.m_uid.ID + " " + Helper.PrintVectorXZY(zdo.GetPosition());
      return id + "\n" + string.Join("\n", content.Select(x => x.Key + ": " + x.Value));
    }).Where(x => x != "").ToList();


    if (args.Log) Log(chestContents);
    else Print(chestContents, false);
    PrintPins();
  }

  private Dictionary<string, int> SearchChest(ZPackage from, HashSet<int> ids)
  {
    Dictionary<string, int> amounts = [];
    var version = from.ReadInt();
    var items = from.ReadInt();
    for (int i = 0; i < items; i++)
    {
      var text = from.ReadString();
      var stack = from.ReadInt();
      // Durability.
      from.ReadSingle();
      from.ReadVector2i();
      from.ReadBool();
      var quality = "";
      if (version >= 101)
      {
        var value = from.ReadInt();
        if (value > 1) quality = " , level " + value + "";
      }
      var variant = "";
      if (version >= 102)
      {
        var value = from.ReadInt();
        if (value > 0) variant = ", style " + value;
      }
      if (version >= 103)
      {
        from.ReadLong();
        from.ReadString();
      }
      if (version >= 104)
      {
        var dataAmount = from.ReadInt();
        for (int j = 0; j < dataAmount; j++)
        {
          from.ReadString();
          from.ReadString();
        }
      }
      if (ids.Contains(text.GetStableHashCode()))
      {
        var key = text + variant + quality;
        if (amounts.ContainsKey(key))
          amounts[key] += stack;
        else
          amounts.Add(key, stack);
      }
    }
    return amounts;
  }
}
