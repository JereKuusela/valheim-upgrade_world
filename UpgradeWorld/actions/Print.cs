namespace UpgradeWorld;
public class Print(Terminal context, string initText, string text, bool start) : ExecutedOperation(context, start)
{
  protected override bool OnExecute()
  {
    if (text != "")
      Print(text);
    return true;
  }

  protected override string OnInit()
  {
    return initText;
  }
}
