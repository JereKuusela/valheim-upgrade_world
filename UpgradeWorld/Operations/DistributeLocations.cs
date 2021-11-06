namespace UpgradeWorld {

  /// <summary>Distributes given location ids to already generated zones.</summary>
  public class DistributeLocations : BaseOperation {
    private string[] Ids;
    private bool ToAlreadyGenerated;
    public DistributeLocations(string[] ids, bool toAlreadyGenerated, Terminal context) : base(context) {
      Ids = ids;
      ToAlreadyGenerated = toAlreadyGenerated;
    }
    public override bool Execute() {
      base.Execute();
      if (Attempts == 1) {
        Print("Redistributing locations to old areas. This may take a while...");
        return false;
      } else {
        Operations.RedistributeLocations(Ids, ToAlreadyGenerated);
        return true;
      }
    }
    protected override void OnEnd() {
      Print("Locations distributed.");
    }
  }
}