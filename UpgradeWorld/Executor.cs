using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
namespace UpgradeWorld;

public static class Executor
{
  private static readonly List<ExecutedOperation> operations = [];
  private static readonly List<Action> cleanUps = [];
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
    if (executionCoroutine == null) return;
    context.StopCoroutine(executionCoroutine);
    executionCoroutine = null;
  }
  public static void AddOperation(ExecutedOperation operation)
  {
    operation.Init();
    operations.Add(operation);

    if (executionCoroutine == null && (Settings.AutoStart || operation.AutoStart))
      StartExecution();
  }
  public static void AddCleanUp(Action cleanUp)
  {
    cleanUps.Add(cleanUp);
  }

  private static void DoClean()
  {
    foreach (var cleanUp in cleanUps) cleanUp();
    cleanUps.Clear();
    StopExecution();
  }
  public static void RemoveOperations()
  {
    operations.Clear();
    DoClean();
    LoadingIndicator.SetProgressVisibility(false);
  }
  private static IEnumerator ExecuteCoroutine()
  {
    var sw = Stopwatch.StartNew();
    while (true)
    {
      if (operations.Count == 0)
      {
        sw.Stop();
        DoClean();
        yield break;
      }

      // Execute the operation as a coroutine - let it yield its own progress
      yield return operations[0].Execute(sw);

      // Operation is complete, remove it
      operations.RemoveAt(0);
    }
  }
  public const float ProgressMin = 0.1f;
}