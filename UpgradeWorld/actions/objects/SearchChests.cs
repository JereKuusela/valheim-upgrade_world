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
    var hash = StandItems.GetHash(zdo, prefix);
    if (hash == 0 || !ids.Contains(hash)) return "";
    var item = ZNetScene.instance.GetPrefab(hash)?.name ?? $"hash:{hash}";
    var variant = zdo.GetInt(prefix + "variant");
    var quality = zdo.GetInt(prefix + "quality");
    if (variant > 0) item += ", style " + variant + "";
    if (quality > 1) item += ", level " + quality + "";
    return item;
  }
  private void Search(IEnumerable<string> ids, DataParameters args)
  {
    var prefabs = GetPrefabs(ids, args.Types);
    var zdos = GetZDOs(args);

    var zs = ZNetScene.instance;
    var standContents = zdos.Select(zdo =>
    {
      var content = StandItems.Prefixes(zdo).Select(prefix => SearchStand(zdo, prefix, prefabs)).Where(x => x != "").ToList();
      if (content.Count == 0) return "";
      var name = zs.GetPrefab(zdo.m_prefab)?.name ?? $"hash:{zdo.m_prefab}";
      var id = name + " " + zdo.m_uid.ID + " " + Helper.PrintVectorXZY(zdo.GetPosition());
      return id + "\n" + string.Join("\n", content);
    }).Where(x => x != "").ToList();

    if (args.Log) Log(standContents);
    else Print(standContents, false);

    var chestContents = zdos.Select(zdo =>
    {
      if (!ChestInventory.TryRead(zdo, out var inventory, out var error))
      {
        if (Settings.Verbose) Print($"Skipping inventory {zdo.m_uid}: {error}");
        return "";
      }
      var content = SearchChest(inventory, prefabs);
      if (content.Count == 0) return "";
      AddPin(zdo.GetPosition());
      var name = zs.GetPrefab(zdo.m_prefab)?.name ?? $"hash:{zdo.m_prefab}";
      var id = name + " " + zdo.m_uid.ID + " " + Helper.PrintVectorXZY(zdo.GetPosition());
      return id + "\n" + string.Join("\n", content.Select(x => x.Key + ": " + x.Value));
    }).Where(x => x != "").ToList();


    if (args.Log) Log(chestContents);
    else Print(chestContents, false);
    PrintPins();
  }

  private Dictionary<string, int> SearchChest(ChestInventory inventory, HashSet<int> ids)
  {
    Dictionary<string, int> amounts = [];
    foreach (var item in inventory.Items)
    {
      if (!ids.Contains(item.Hash)) continue;
      var name = ZNetScene.instance.GetPrefab(item.Hash)?.name ?? (item.Name != "" ? item.Name : $"hash:{item.Hash}");
      var key = name + (item.Variant > 0 ? $", style {item.Variant}" : "") + (item.Quality > 1 ? $", level {item.Quality}" : "");
      amounts[key] = amounts.TryGetValue(key, out var count) ? count + item.Stack : item.Stack;
    }
    return amounts;
  }
}
