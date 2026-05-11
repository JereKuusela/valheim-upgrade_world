// v1.1
/**
* Reflection bridge from Upgrade World to Location Placement Accelerator (LPA).
*
* UW keeps no hard reference to LPA so the mod is optional. This shim looks
* LPA up at runtime via the BepInEx plugin registry, caches the relevant
* MethodInfo, and exposes a typed wrapper that UW callers can yield against
* like any other coroutine. Super duper.
*
* 1.1: builds an LpaApiOptions instance and sets CallerTag = "UW" so LPA's
* per-run log file is named LPA_UW_... instead of colliding with world-gen logs.
*/
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Bootstrap;
using UnityEngine;

namespace UpgradeWorld;

internal static class LPACompatibilityShim
{
    private const string LpaPluginGuid = "nickpappas.locationplacementaccelerator";
    private const string DefaultCallerTag = "UW";

    private static bool _initialized;
    private static bool _isAvailable;
    private static MethodInfo _runCustomPlacementMethod;
    private static System.Type _optionsType;
    private static FieldInfo _callerTagField;


    // Returns true if LPA is loaded and the expected API surface is reachable.
    // saves the lookup so subsequent calls are a single boolean read.
    public static bool IsAvailable()
    {
        if (_initialized) return _isAvailable;
        _initialized = true;

        if (!Chainloader.PluginInfos.TryGetValue(LpaPluginGuid, out var plugin))
        {
            return false;
        }
        if (plugin?.Instance == null)
        {
            return false;
        }

        var assembly = plugin.Instance.GetType().Assembly;
        var apiType = assembly.GetType("LPA.API");
        if (apiType == null)
        {
            return false;
        }

        // Probe LPA.API.IsAvailable - cheap sanity check that the API is built.
        var probe = apiType.GetMethod("IsAvailable", BindingFlags.Public | BindingFlags.Static);
        if (probe == null)
        {
            return false;
        }
        try
        {
            if (probe.Invoke(null, null) is not bool ok || !ok) return false;
        }
        catch
        {
            return false;
        }

        // Resolve the Dictionary overload of RunCustomPlacement by exact
        // parameter signature. Using the dictionary overload keeps us out of the
        // business of reflecting on LPA's PlacementRequest struct.
        var optionsType = assembly.GetType("LPA.LpaApiOptions");
        if (optionsType == null)
        {
            return false;
        }

        _runCustomPlacementMethod = apiType.GetMethod(
            "RunCustomPlacement",
            BindingFlags.Public | BindingFlags.Static,
            null,
            new[]
            {
                typeof(Dictionary<ZoneSystem.ZoneLocation, int>),
                typeof(HashSet<Vector2i>),
                optionsType
            },
            null);
        if (_runCustomPlacementMethod == null)
        {
            return false;
        }

        _optionsType = optionsType;
        _callerTagField = optionsType.GetField("CallerTag", BindingFlags.Public | BindingFlags.Instance);

        _isAvailable = true;
        return true;
    }


    // Hand off a request set to LPA. 
    public static IEnumerator RunCustomPlacement(
    Dictionary<ZoneSystem.ZoneLocation, int> requests,
    HashSet<Vector2i> allowedZones)
    {
        if (!IsAvailable()) yield break;
        if (requests == null || requests.Count == 0) yield break;

        object options = null;
        if (_optionsType != null)
        {
            options = System.Activator.CreateInstance(_optionsType);
            _callerTagField?.SetValue(options, DefaultCallerTag);
        }

        var inner = (IEnumerator)_runCustomPlacementMethod.Invoke(
            null,
            new object[] { requests, allowedZones, options });

        yield return inner;
    }
}