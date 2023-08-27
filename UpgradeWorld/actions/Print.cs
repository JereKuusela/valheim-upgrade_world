namespace UpgradeWorld;
public class Print : ExecutedOperation {
  readonly string InitText = "";
  readonly string Text = "";
  public Print(Terminal context, string initText, string text, bool start) : base(context, start) {
    InitText = initText;
    Text = text;
  }


  protected override bool OnExecute() {
    if (Text != "")
      Print(Text);
    return true;
  }

  protected override string OnInit() {
    return InitText;
  }
}
