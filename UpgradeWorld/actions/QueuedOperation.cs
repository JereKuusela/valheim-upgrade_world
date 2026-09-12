using System;
using System.Collections;
using System.Diagnostics;

namespace UpgradeWorld;

public class QueuedOperation : ExecutedOperation
{
  private readonly Action _action;
  private readonly string _message;

  public QueuedOperation(Terminal context, bool pin, string message, Action action) : base(context, pin)
  {
    _message = message;
    _action = action;
  }

  protected override IEnumerator OnExecute(Stopwatch sw)
  {
    _action();
    yield break;
  }

  protected override string OnInit() => _message;
}
