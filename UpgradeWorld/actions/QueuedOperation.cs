using System;
using System.Collections;
using System.Diagnostics;

namespace UpgradeWorld;

public class QueuedOperation(Terminal context, Minimap.PinType? pin, string message, Action action) : ExecutedOperation(context, pin)
{
  private readonly Action _action = action;
  private readonly string _message = message;

  protected override IEnumerator OnExecute(Stopwatch sw)
  {
    SharedPins = Pins;
    try { _action(); }
    finally { SharedPins = null; }
    yield break;
  }

  protected override string OnInit() => _message;
}
