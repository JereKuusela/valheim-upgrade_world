using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
namespace UpgradeWorld;
///<summary>Base class for all operations that need execution. Provides the execution logic.</summary>
public abstract class ExecutedOperation(Terminal context, bool pin = false) : BaseOperation(context, pin)
{
  protected int Failed = 0;
  private string queuedInfo = "";
  public bool ExecutionFailed { get; private set; }

  public IEnumerator Execute(Stopwatch sw)
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
      while (stack.Count > 0)
      {
        try { (stack.Pop() as IDisposable)?.Dispose(); }
        catch (Exception e) { failure ??= e; }
      }
    }
    if (failure == null)
    {
      try { PrintPins(); OnEnd(); }
      catch (Exception e) { failure = e; }
    }
    if (failure != null)
    {
      ExecutionFailed = true;
      Print($"Operation failed: {failure.Message}. Remaining queued operations will be cancelled. Changes already made are not rolled back.");
      UpgradeWorld.Log.LogError(failure);
    }
  }

  protected abstract IEnumerator OnExecute(Stopwatch sw);
  public bool Init(bool autoStart)
  {
    queuedInfo = OnInit();
    if (queuedInfo == "") return false;
    var output = queuedInfo;
    if (!autoStart) output += Helper.GetStartMessage();
    Print(output);
    return true;
  }
  protected abstract string OnInit();
  public string GetInfo() => queuedInfo != "" ? queuedInfo : GetType().Name;
  protected virtual void OnStart() { }
  protected virtual void OnEnd() { }
}
