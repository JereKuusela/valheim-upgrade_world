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
    var prefabs = GetPrefabs(ids, args.Ignore, args.Types);
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
      var content = SearchChest(zdo, prefabs);
      if (content == null || content.Count == 0) return "";
      AddPin(zdo.GetPosition());
      var name = zs.m_namedPrefabs[zdo.m_prefab].name;
      var id = name + " " + zdo.m_uid.ID + " " + Helper.PrintVectorXZY(zdo.GetPosition());
      return id + "\n" + string.Join("\n", content.Select(x => x.Key + ": " + x.Value));
    }).Where(x => x != "").ToList();


    if (args.Log) Log(chestContents);
    else Print(chestContents, false);
    PrintPins();
  }

  private Dictionary<string, int>? SearchChest(ZDO zdo, HashSet<int> ids)
  {
    var items = ItemDataHelper.Load(zdo);
    if (items.Count == 0) return null;
    Dictionary<string, int> amounts = [];
    foreach (var record in items)
    {
      var text = record.PrefabName;
      if (text == "" || !ids.Contains(text.GetStableHashCode())) continue;
      var variant = record.Variant > 0 ? ", style " + record.Variant : "";
      var quality = record.Quality > 1 ? " , level " + record.Quality : "";
      var key = text + variant + quality;
      if (amounts.ContainsKey(key))
        amounts[key] += record.Stack;
      else
        amounts.Add(key, record.Stack);
    }
    return amounts;
  }
}

