using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
namespace UpgradeWorld;

[HarmonyPatch(typeof(ZNet), nameof(ZNet.RPC_PeerInfo))]
public class ServerExecution
{
  public static ZRpc? User = null;

  ///<summary>Sends command to the server so that it can be executed there.</summary>
  public static void Send(string command)
  {
    if (!ZNet.instance) return;
    var server = ZNet.instance.GetServerRPC();
    if (server == null) return;
    Helper.Print(Console.instance, "Sending command: " + command);
    server.Invoke(RPC_Command, [command]);
  }
  ///<summary>Sends command to the server so that it can be executed there.</summary>
  public static void Send(IEnumerable<string> args) => Send(string.Join(" ", args));
  ///<summary>Sends command to the server so that it can be executed there.</summary>
  public static void Send(Terminal.ConsoleEventArgs args) => Send(args.Args);

  public static string RPC_Command = "UpgradeWorld_Command";
  public static string RPC_Pins = "DEV_Pins";

  public static string RPC_RemotePrintOnce = "UpgradeWorld_RemotePrintOnce";
  public static string RPC_RequestSync = "UpgradeWorld_RequestSync";
  public static string RPC_SyncData = "UpgradeWorld_SyncData";
  private static bool IsAllowed(ZRpc rpc)
  {
    var zNet = ZNet.instance;
    if (!zNet.enabled) return false;
    if (rpc == null) return false;
    var host = rpc.GetSocket().GetHostName();
    if (Settings.IsRoot(host)) return true;
    Helper.Print(Console.instance, rpc, "Unauthorized to use Upgrade World commands.");
    return false;
  }
  private static void RPC_Do_Command(ZRpc rpc, string command)
  {
    User = rpc;
    if (IsAllowed(rpc))
    {
      // Server doesn't have cheats enabled by default, so ensure it is set to run commands.
      Terminal.m_cheat = true;
      Console.instance.TryRunCommand(command);
    }
    User = null;
  }
  private static void RPC_PrintOnce(ZRpc rpc, string value)
  {
    Helper.PrintOnce(Console.instance, null, value);
  }
  private static void RPC_Do_RequestSync(ZRpc rpc, int clientLocationHash, int clientVegetationHash)
  {
    if (!IsAllowed(rpc)) return;
    var locationIds = string.Join("|", LocationOperation.AllIds());
    var vegetationIds = string.Join("|", VegetationOperation.AllIds());
    var serverLocationHash = locationIds.GetStableHashCode();
    var serverVegetationHash = vegetationIds.GetStableHashCode();
    // Only send data if hashes differ
    if (serverLocationHash != clientLocationHash || serverVegetationHash != clientVegetationHash)
      rpc.Invoke(RPC_SyncData, [locationIds, vegetationIds]);
  }
  private static void RPC_Do_SyncData(ZRpc rpc, string locationIds, string vegetationIds)
  {
    LocationOperation.SetServerIds(locationIds);
    VegetationOperation.SetServerIds(vegetationIds);
  }
  ///<summary>Requests location and vegetation IDs from the server.</summary>
  public static void RequestSync()
  {
    var server = ZNet.instance.GetServerRPC();
    if (server == null)
      return;
    var locationHash = LocationOperation.GetServerIdsHash();
    var vegetationHash = VegetationOperation.GetServerIdsHash();
    server.Invoke(RPC_RequestSync, [locationHash, vegetationHash]);
  }
  static void Postfix(ZNet __instance, ZRpc rpc)
  {
    if (__instance.IsDedicated())
    {
      rpc.Register<string>(RPC_Command, new(RPC_Do_Command));
      rpc.Register<int, int>(RPC_RequestSync, new(RPC_Do_RequestSync));
    }
    else
    {
      rpc.Register<string>(RPC_RemotePrintOnce, new(RPC_PrintOnce));
      rpc.Register<string, string>(RPC_SyncData, new(RPC_Do_SyncData));
    }
  }
}
