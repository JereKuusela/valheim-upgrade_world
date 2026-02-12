using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
namespace UpgradeWorld;

public static class Executor
{
  private static readonly List<ExecutedOperation> operations = [];
  private static Coroutine? executionCoroutine;
  private static MonoBehaviour? context;
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
    if (context == null) throw new Exception("Executor context is not set. Call Executor.SetContext from a MonoBehaviour before starting execution.");
    if (executionCoroutine != null) return;
    executionCoroutine = context.StartCoroutine(ExecuteCoroutine());
  }

  public static void StopExecution()
  {
    if (context == null) throw new Exception("Executor context is not set. Call Executor.SetContext from a MonoBehaviour before stopping execution.");
    operations.Clear();
    // Needed to indicate end of generation for some mods.
    if (Hud.instance)
      Hud.instance.m_loadingIndicator.SetShowProgress(false);

    if (executionCoroutine == null) return;
    context.StopCoroutine(executionCoroutine);
    executionCoroutine = null;
  }
  public static void AddOperation(ExecutedOperation operation, bool autoStart)
  {
    bool start = Settings.AutoStart || autoStart;
    if (!operation.Init(start))
      return;
    operations.Add(operation);

    if (executionCoroutine == null && start)
      StartExecution();
  }

  public static List<ExecutedOperation> GetOperations()
  {
    return operations;
  }

  private static IEnumerator ExecuteCoroutine()
  {
    var sw = Stopwatch.StartNew();
    while (operations.Count > 0)
    {
      sw.Restart();
      yield return operations[0].Execute(sw);
      operations.RemoveAt(0);
    }
    sw.Stop();
    StopExecution();
  }
  public const long ProgressMin = 100; // 0.1 seconds
  public const int ZdoMaxUpdates = 10000;
}