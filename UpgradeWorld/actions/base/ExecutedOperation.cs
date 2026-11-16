using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
namespace UpgradeWorld;
///<summary>Base class for all operations that need execution. Provides the execution logic.</summary>
public abstract class ExecutedOperation(Terminal context, Minimap.PinType? pin = null) : BaseOperation(context, pin)
{
  protected int Failed = 0;
  private string queuedInfo = "";
  public bool ExecutionFailed { get; private set; }
  internal virtual bool IsComplete => true;

  public IEnumerator Execute(Stopwatch sw) => Execute(sw, true);

  internal IEnumerator Execute(Stopwatch sw, bool cancelQueueOnFailure)
  {
    // Unity advances yielded child enumerators outside the old try/catch.
    // Advance the stack here so errors in any nested operation stop the queue.
    Stack<IEnumerator> stack = new();
    Exception? failure = null;
    try
    {
      OnStart();
      stack.Push(OnExecute(sw));
    }
    catch (Exception e) { failure = e; }
    try
    {
      while (failure == null && stack.Count > 0)
      {
        object? next = null;
        try
        {
          var current = stack.Peek();
          if (!current.MoveNext())
          {
            stack.Pop();
            (current as IDisposable)?.Dispose();
            continue;
          }
          next = current.Current;
          if (next is IEnumerator child)
          {
            stack.Push(child);
            continue;
          }
        }
        catch (Exception e) { failure = e; }
        if (failure == null) yield return next;
      }
    }
    finally
    {
      DisposeStack(stack, ref failure);
    }
    if (failure == null && IsComplete)
    {
      try { PrintPins(); OnEnd(); }
      catch (Exception e) { failure = e; }
    }
    if (failure != null)
      ReportFailure(failure, cancelQueueOnFailure);
  }

  private void ReportFailure(Exception failure, bool cancelQueue)
  {
    ExecutionFailed = true;
    var queued = cancelQueue ? " Remaining queued operations will be cancelled." : "";
    Print($"Operation failed. {failure.Message}.{queued} Changes already made remain in the world.");
    UpgradeWorld.Log.LogError(failure);
  }

  private static void DisposeStack(Stack<IEnumerator> stack, ref Exception? failure)
  {
    while (stack.Count > 0)
    {
      try { (stack.Pop() as IDisposable)?.Dispose(); }
      catch (Exception e) { failure ??= e; }
    }
  }

  protected abstract IEnumerator OnExecute(Stopwatch sw);
  public bool Init()
  {
    queuedInfo = OnInit();
    if (queuedInfo == "") return false;
    Print(queuedInfo);
    return true;
  }
  protected abstract string OnInit();
  public string GetInfo() => queuedInfo != "" ? queuedInfo : GetType().Name;
  protected virtual void OnStart() { }
  protected virtual void OnEnd() { }
}