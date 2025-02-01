using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Bootstrap;
using UnityEngine;

namespace Service;

public class ComponentInfo
{
  private static Type[]? types;
  public static Type[] Types => types ??= LoadTypes();
  private static Type[] LoadTypes()
  {
    List<Assembly> assemblies = [Assembly.GetAssembly(typeof(ZNetView)), .. Chainloader.PluginInfos.Values.Where(p => p.Instance != null).Select(p => p.Instance.GetType().Assembly)];
    var assembly = Assembly.GetAssembly(typeof(ZNetView));
    var baseType = typeof(MonoBehaviour);
    return [.. assemblies.SelectMany(s =>
    {
      try
      {
        return s.GetTypes();
      }
      catch (ReflectionTypeLoadException e)
      {
        return e.Types.Where(t => t != null);
      }
    }).Where(t =>
    {
      try
      {
        return baseType.IsAssignableFrom(t);
      }
      catch
      {
        return false;
      }
    })];
  }
  private static readonly Dictionary<string, HashSet<int>> PrefabComponents = [];
  public static IEnumerable<KeyValuePair<int, GameObject>> HaveComponent(IEnumerable<KeyValuePair<int, GameObject>> objs, List<string[]> typeSets)
  {
    if (PrefabComponents.Count == 0)
    {
      foreach (var kvp in ZNetScene.instance.m_namedPrefabs)
      {

        kvp.Value.GetComponentsInChildren<MonoBehaviour>(ZNetView.m_tempComponents);
        foreach (var component in ZNetView.m_tempComponents)
        {
          var type = component.GetType().Name.ToLowerInvariant();
          if (!PrefabComponents.ContainsKey(type))
            PrefabComponents[type] = [];
          PrefabComponents[type].Add(kvp.Key);
        }
      }
    }
    return objs.Where(obj =>
      typeSets.Any(types =>
        types.All(type =>
          PrefabComponents.TryGetValue(type.ToLowerInvariant(), out var hashes) && hashes.Contains(obj.Key))));
  }
}
