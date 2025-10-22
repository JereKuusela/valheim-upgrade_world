using System.Collections;
using System.Diagnostics;
namespace UpgradeWorld;

public class Print(Terminal context, string initText, string text, bool start) : ExecutedOperation(context, start)
{
  protected override IEnumerator OnExecute(Stopwatch sw)
  {
    if (text != "")
      Print(text);
    yield break;
  }

  protected override string OnInit()
  {
    return initText;
  }
}
