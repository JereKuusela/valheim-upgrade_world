using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Bootstrap;
namespace UpgradeWorld;
#nullable disable
public static class CommandWrapper
{
  public static Assembly ServerDevcommands = null;
  const string GUID = "server_devcommands";
  public static void Init()
  {
    if (Chainloader.PluginInfos.TryGetValue(GUID, out var info))
    {
      if (info.Metadata.Version.Major == 1 && info.Metadata.Version.Minor < 51)
      {
        UpgradeWorld.Log.LogWarning($"Server devcommands v{info.Metadata.Version.Major}.{info.Metadata.Version.Minor} is outdated. Please update for better command instructions!");
      }
      else
      {
        ServerDevcommands = info.Instance.GetType().Assembly;
      }
    }
  }
  private static readonly BindingFlags PublicBinding = BindingFlags.Static | BindingFlags.Public;
  private static Type Type() => ServerDevcommands!.GetType("ServerDevcommands.AutoComplete");
  private static Type InfoType() => ServerDevcommands!.GetType("ServerDevcommands.ParameterInfo");
  private static MethodInfo GetMethod(Type type, string name, Type[] types) => type.GetMethod(name, PublicBinding, null, CallingConventions.Standard, types, null);
  public static void Register(string command, Func<int, int, List<string>> action)
  {
    if (ServerDevcommands == null) return;
    GetMethod(Type(), "Register", [typeof(string), typeof(Func<int, int, List<string>>)]).Invoke(null, [command, action]);
  }
  public static void Register(string command, Func<int, List<string>> action)
  {
    if (ServerDevcommands == null) return;
    GetMethod(Type(), "Register", [typeof(string), typeof(Func<int, List<string>>)]).Invoke(null, [command, action]);
  }
  public static void Register(string command, Func<int, List<string>> action, Dictionary<string, Func<int, List<string>>> named)
  {
    if (ServerDevcommands == null) return;
    GetMethod(Type(), "Register", [typeof(string), typeof(Func<int, List<string>>), typeof(Dictionary<string, Func<int, List<string>>>)]).Invoke(null, [command, action, named]);
  }
  public static List<string> Info(string value)
  {
    if (ServerDevcommands == null) return new();
    return GetMethod(InfoType(), "Create", [typeof(string)]).Invoke(null, [value]) as List<string>;
  }
  public static List<string> XZ(string name, string description, int index)
  {
    if (ServerDevcommands == null) return new();
    return GetMethod(InfoType(), "XZ", [typeof(string), typeof(string), typeof(int)]).Invoke(null, [name, description, index]) as List<string>;
  }
  public static List<string> XZY(string name, string description, int index)
  {
    if (ServerDevcommands == null) return new();
    return GetMethod(InfoType(), "XZY", [typeof(string), typeof(string), typeof(int)]).Invoke(null, [name, description, index]) as List<string>;
  }
  public static List<string> Flag(string name, string description)
  {
    if (ServerDevcommands == null) return new();
    return GetMethod(InfoType(), "Flag", [typeof(string), typeof(string)]).Invoke(null, new[] { name, description }) as List<string>;
  }
  public static List<string> ObjectIds()
  {
    if (ServerDevcommands == null) return new();
    return InfoType().GetProperty("ObjectIds", PublicBinding).GetValue(null) as List<string>;
  }
  public static List<string> LocationIds()
  {
    if (ServerDevcommands == null) return new();
    return InfoType().GetProperty("LocationIds", PublicBinding).GetValue(null) as List<string>;
  }
  public static void RegisterEmpty(string command)
  {
    if (ServerDevcommands == null) return;
    Type().GetMethod("RegisterEmpty", PublicBinding).Invoke(null, [command]);
  }
}
