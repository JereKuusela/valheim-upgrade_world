using System.Collections.Generic;
using HarmonyLib;
namespace UpgradeWorld;

[HarmonyPatch(typeof(Terminal), nameof(Terminal.AddString), new[] { typeof(string) })]
public class RedirectOutput {
  public static ZRpc Target = null;
  static void Postfix(string text) {
    if (ZNet.m_isServer && Target != null) {
      ZNet.instance.RemotePrint(Target, text);
    }
  }
}

[HarmonyPatch(typeof(ZNet), nameof(ZNet.RPC_PeerInfo))]
public class ServerExecution {

  ///<summary>Sends command to the server so that it can be executed there.</summary>
  public static void Send(string command) {
    var server = ZNet.instance.GetServerRPC();
    Console.instance.AddString("Sending command: " + command);
    if (server != null) server.Invoke(RPC_Command, new[] { command });
  }
  ///<summary>Sends command to the server so that it can be executed there.</summary>
  public static void Send(IEnumerable<string> args) => Send(string.Join(" ", args));
  ///<summary>Sends command to the server so that it can be executed there.</summary>
  public static void Send(Terminal.ConsoleEventArgs args) => Send(args.Args);

  public static string RPC_Command = "UpgradeWorld_Command";
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
    Console.instance.AddString("Unauthorized to use Upgrade World commands.");
    return false;
  }
  private static void RPC_Do_Command(ZRpc rpc, string command) {
    RedirectOutput.Target = rpc;
    if (IsAllowed(rpc, command))
      Console.instance.TryRunCommand(command);
    RedirectOutput.Target = null;
  }
  static void Postfix(ZNet __instance, ZRpc rpc) {
    if (__instance.IsServer()) {
      rpc.Register<string>(RPC_Command, new(RPC_Do_Command));
    }
  }
}
