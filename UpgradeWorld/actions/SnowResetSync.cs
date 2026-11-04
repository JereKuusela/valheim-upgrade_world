using System;
using HarmonyLib;

namespace UpgradeWorld;

internal static class SnowResetSync
{
  private static Action<TerrainComp>? checkLoad;

  private static void Reload(TerrainComp compiler)
  {
    checkLoad ??= AccessTools.MethodDelegate<Action<TerrainComp>>(AccessTools.Method(typeof(TerrainComp), "CheckLoad"));
    checkLoad(compiler);
  }

  internal static void Refresh(ZDO zdo)
  {
    var scene = ZNetScene.instance;
    var instance = scene ? scene.FindInstance(zdo.m_uid) : null;
    var compiler = instance ? instance.GetComponent<TerrainComp>() : null;
    // Only registered compilers have found a heightmap and initialized their arrays.
    if (compiler && TerrainComp.FindTerrainCompiler(zdo.GetPosition()) == compiler) Reload(compiler);
  }
}
