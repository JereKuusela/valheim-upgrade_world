using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using HarmonyLib;
using UnityEngine;
namespace UpgradeWorld;

[HarmonyPatch(typeof(ZNet), "OnDestroy")]
public static class Executor
{
  private static readonly List<ExecutedOperation> operations = [];
  private static Coroutine? executionCoroutine;
  private static MonoBehaviour? context;
  private static ExecutedOperation? hintOperation;
  private static bool queueStarted;
  private static float snowRetryAt;
  internal static RefillSnow? DeferredSnow { get; private set; }
  private static bool QueuedWorkReady => queueStarted && operations.Count > 0;
  private static bool SnowReady => DeferredSnow != null && Time.time >= snowRetryAt;

  public static void SetUser(ZRpc? user)
  {
    foreach (var operation in operations) operation.User = user;
  }
  public static void SetContext(MonoBehaviour context)
  {
    Executor.context = context;
  }

  public static void StartExecution()
  {
    queueStarted = operations.Count > 0;
    if (queueStarted) StartWorker();
  }

  internal static void RetryDeferredSnow()
  {
    if (executionCoroutine == null && SnowReady) StartWorker();
  }

  internal static void StartSnow(RefillSnow snow)
  {
    if (!snow.Init()) return;
    DeferredSnow = snow;
    snowRetryAt = Time.time;
    StartWorker();
  }

  private static void StartWorker()
  {
    if (context == null) throw new Exception("Executor context is not set. Call Executor.SetContext from a MonoBehaviour before starting execution.");
    if (executionCoroutine != null) return;
    executionCoroutine = context.StartCoroutine(ExecuteCoroutine());
    // Execute could finish before executionCoroutine is assigned.
    if (!QueuedWorkReady && !SnowReady) Stop();
  }

  public static void StopExecution()
  {
    ClearQueue();
    if (executionCoroutine != null) Stop();
    HideProgress();
  }

  private static void ClearQueue()
  {
    SnowResetLive.End();
    operations.Clear();
    hintOperation = null;
    queueStarted = false;
    DeferredSnow = null;
  }

  private static void HideProgress()
  {
    // Needed to indicate end of generation for some mods.
    var hud = Hud.instance;
    if (hud != null && hud.m_loadingIndicator != null)
      hud.m_loadingIndicator.SetShowProgress(false);
  }

  private static void Stop()
  {
    if (context == null) throw new Exception("Executor context is not set. Call Executor.SetContext from a MonoBehaviour before stopping execution.");
    var coroutine = executionCoroutine;
    executionCoroutine = null;
    if (coroutine != null) context.StopCoroutine(coroutine);
  }

  private static void Prefix(ZNet __instance)
  {
    if (__instance != ZNet.instance) return;
    SnowResetLive.Clear();
    if (operations.Count > 0 || DeferredSnow != null) StopExecution();
  }

  public static void AddOperation(ExecutedOperation operation, bool autoStart)
  {
    bool start = Settings.AutoStart || autoStart;
    if (!operation.Init())
      return;
    operations.Add(operation);

    if (!start)
      hintOperation = operation;
    else
      StartExecution();
  }

  public static void PrintHint()
  {
    var operation = hintOperation;
    if (operation == null) return;
    hintOperation = null;
    if (!queueStarted && operations.Count > 0)
      operation.Print(Helper.GetStartMessage(), false);
  }

  public static List<ExecutedOperation> GetOperations()
  {
    return operations;
  }

  private static IEnumerator ExecuteCoroutine()
  {
    var sw = Stopwatch.StartNew();
    try
    {
      while (QueuedWorkReady || SnowReady)
      {
        var fromQueue = QueuedWorkReady;
        var operation = fromQueue ? operations[0] : DeferredSnow!;
        sw.Restart();
        yield return operation.Execute(sw, fromQueue);
        if (operation.ExecutionFailed)
        {
          if (fromQueue)
          {
            ClearQueue();
            break;
          }
          DeferredSnow = null;
          SnowResetLive.End();
          continue;
        }
        if (fromQueue)
        {
          operations.Remove(operation);
          if (operations.Count == 0) queueStarted = false;
        }
        if (operation is RefillSnow snow)
        {
          DeferredSnow = snow.IsComplete ? null : snow;
          snowRetryAt = Time.time + 1f;
        }
      }
    }
    finally
    {
      sw.Stop();
      executionCoroutine = null;
      HideProgress();
    }
  }
  public const long ProgressMin = 100; // 0.1 seconds
  public const int ZdoMaxUpdates = 10000;
}