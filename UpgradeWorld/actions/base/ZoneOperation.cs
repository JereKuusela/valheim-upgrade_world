using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
namespace UpgradeWorld;
///<summary>Base class for all zone based operations. Provides the "zone by zone" execution logic.</summary>
public abstract class ZoneOperation(Terminal context, FiltererParameters args) : ExecutedOperation(context, args.Pin)
{
  public string Operation = "BaseOperation";
  protected Vector2i[] ZonesToUpgrade = Zones.GetZones(args);
  protected int ZoneIndex = 0;
  ///<summary>Some operations can be done outside the zone loading logic.</summary>
  protected int PreOperated = 0;
  protected FiltererParameters Args = args;
  protected List<IZoneFilterer> Filterers = [];
  protected string InitString = "";
  protected override string OnInit()
  {
    List<string> messages = [];
    ZonesToUpgrade = Filterers.Aggregate(ZonesToUpgrade, (zones, filterer) => filterer.FilterZones(zones, ref messages));
    InitString += $" {ZonesToUpgrade.Length} zones";
    if (messages.Count > 0)
      InitString += $": {Helper.JoinRows(messages)}";
    InitString += $".";
    return InitString;
  }
  protected abstract bool ExecuteZone(Vector2i zone);
  protected override IEnumerator OnExecute(Stopwatch sw)
  {
    if (ZonesToUpgrade == null || ZonesToUpgrade.Length == 0)
      yield break;


    while (ZoneIndex < ZonesToUpgrade.Length)
    {
      var zone = ZonesToUpgrade[ZoneIndex];
      var success = ExecuteZone(zone);
      // Makes the zone unload as soon as possible.
      if (success && ZoneSystem.instance.m_zones.TryGetValue(zone, out var zoneObj))
        zoneObj.m_ttl = ZoneSystem.instance.m_zoneTTL;
      MoveToNextZone(success);

      // Check if enough time has passed to yield control
      var currentTime = sw.ElapsedMilliseconds;
      if (currentTime >= Executor.ProgressMin)
      {
        UpdateConsole();
        yield return null;
      }
    }

    // Final console update
    UpdateConsole();
  }

  private int Attempts = 0;
  private void MoveToNextZone(bool success = true)
  {
    if (success)
    {
      Attempts = 0;
      ZoneIndex++;
    }
    else
    {
      Attempts++;
      if (Attempts > 1000)
      {
        Failed++;
        Attempts = 0;
        ZoneIndex++;
      }
    }
  }
  private void UpdateConsole()
  {
    if (Settings.Verbose)
    {
      var totalString = (ZonesToUpgrade.Length + PreOperated).ToString();
      var updatedString = (ZoneIndex + PreOperated).ToString().PadLeft(totalString.Length, '0');
      PrintOnce(Operation + ": " + updatedString + "/" + totalString, false);
    }
    else
    {
      var percent = Math.Min(100, ZonesToUpgrade.Length == 0 ? 100 : (int)Math.Floor(100.0 * (ZoneIndex + PreOperated) / (ZonesToUpgrade.Length + PreOperated)));
      PrintOnce(Operation + ": " + percent + "%", false);
    }
  }
}
