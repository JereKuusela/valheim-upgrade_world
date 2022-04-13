using System.Collections.Generic;
using HarmonyLib;
namespace UpgradeWorld;

[HarmonyPatch(typeof(ZNet), nameof(ZNet.RPC_PeerInfo))]
public class ServerExecution {
  public static ZRpc? User = null;

  ///<summary>Sends command to the server so that it can be executed there.</summary>
  public static void Send(string command) {
    if (!ZNet.instance) return;
    var server = ZNet.instance.GetServerRPC();
    if (server == null) return;
    Helper.Print(Console.instance, "Sending command: " + command);
    server.Invoke(RPC_Command, new[] { command });
  }
  ///<summary>Sends command to the server so that it can be executed there.</summary>
  public static void Send(IEnumerable<string> args) => Send(string.Join(" ", args));
  ///<summary>Sends command to the server so that it can be executed there.</summary>
  public static void Send(Terminal.ConsoleEventArgs args) => Send(args.Args);

  public static string RPC_Command = "UpgradeWorld_Command";
  public static string RPC_RemotePrintOnce = "UpgradeWorld_RemotePrintOnce";
  private static bool IsAllowed(ZRpc rpc, string command) {
    var zNet = ZNet.instance;
    if (!zNet.enabled) return false;
    if (rpc == null) return false;
    var host = rpc.GetSocket().GetHostName();
    var root = Settings.RootUsers;
    var allowed = false;
    if (root.Count > 0) allowed = root.Contains(host);
    else allowed = zNet.m_adminList.Contains(host);
    if (allowed) return true;
    Helper.Print(Console.instance, rpc, "Unauthorized to use Upgrade World commands.");
    return false;
  }
  private static void RPC_Do_Command(ZRpc rpc, string command) {
    User = rpc;
    if (IsAllowed(rpc, command))
      Console.instance.TryRunCommand(command);
    User = null;
  }
  private static void RPC_PrintOnce(ZRpc rpc, string value) {
    Helper.PrintOnce(Console.instance, null, value);
  }
  static void Postfix(ZNet __instance, ZRpc rpc) {
    if (__instance.IsDedicated())
      rpc.Register<string>(RPC_Command, new(RPC_Do_Command));
    else
      rpc.Register<string>(RPC_RemotePrintOnce, new(RPC_PrintOnce));
  }
}
