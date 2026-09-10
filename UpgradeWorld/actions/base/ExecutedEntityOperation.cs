using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace UpgradeWorld;

/// <summary>Base class for executed entity operations that process ZDOs in batches.</summary>
public abstract class ExecutedEntityOperation(Terminal context, IEnumerable<string> ids, DataParameters args) : ExecutedOperation(context, args.Pin)
{
  protected readonly IEnumerable<string> Ids = ids;
  protected readonly DataParameters Args = args;
  protected HashSet<int> Prefabs = [];
  protected ZDO[] ZdosToProcess = [];
  private ZDOID[] selectedIds = [];
  protected Dictionary<int, int> Counts = [];
  protected int ProcessedCount = 0;
  protected int TotalCount = 0;

  protected override string OnInit()
  {
    Prefabs = GetPrefabsForOperation();
    ZdosToProcess = EntityOperation.GetZDOs(Args, Prefabs);
    selectedIds = ZdosToProcess.Select(zdo => zdo.m_uid).ToArray();
    TotalCount = ZdosToProcess.Length;
    Counts = Prefabs.ToDictionary(prefab => prefab, prefab => 0);
    if (TotalCount == 0)
    {
      Print(GetNoObjectsMessage());
      // Must return empty to not queue the operation.
      return "";
    }
    return GetInitMessage();
  }

  protected override IEnumerator OnExecute(Stopwatch sw)
  {
    if (TotalCount == 0)
      yield break;

    var processed = 0;

    for (var i = 0; i < ZdosToProcess.Length; ++i)
    {
      var zdo = ZdosToProcess[i];
      // Prior queued commands may destroy an object or recycle its ZDO instance.
      if (zdo.m_uid != selectedIds[i] || ZDOMan.instance.GetZDO(selectedIds[i]) != zdo) continue;
      var prefab = zdo.m_prefab;
      var position = zdo.GetPosition();
      if (ProcessZDO(zdo))
      {
        Counts[prefab] += 1;
        ProcessedCount += 1;
        AddPin(position);
      }

      processed++;

      if (processed >= Executor.ZdoMaxUpdates)
      {
        processed = 0;
        yield return null;
      }
    }
  }

  protected override void OnEnd()
  {
    var linq = Counts.Where(kvp => kvp.Value > 0).Select(kvp => GetCountMessage(kvp.Value, kvp.Key)).Where(msg => !string.IsNullOrEmpty(msg));
    string[] texts = [GetProcessedMessage(), .. linq];
    if (Args.Log) Log(texts);
    else Print(texts, false);
    PrintPins();
  }

  /// <summary>Gets the prefabs to operate on. Override for custom prefab selection logic.</summary>
  protected virtual HashSet<int> GetPrefabsForOperation() => EntityOperation.GetPrefabs(Ids, Args.Types);

  /// <summary>Processes a single ZDO. Return true if the ZDO was modified and should be counted.</summary>
  protected abstract bool ProcessZDO(ZDO zdo);

  /// <summary>Gets the message to display when no objects are found.</summary>
  protected abstract string GetNoObjectsMessage();

  /// <summary>Gets the initialization message to display.</summary>
  protected abstract string GetInitMessage();

  /// <summary>Gets the message showing total processed count.</summary>
  protected abstract string GetProcessedMessage();

  /// <summary>Gets the message for individual prefab counts.</summary>
  protected abstract string GetCountMessage(int count, int prefab);
}