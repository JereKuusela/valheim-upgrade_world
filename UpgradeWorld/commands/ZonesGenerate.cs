using System;
using System.Linq;
using Service;

namespace UpgradeWorld;

public class ZonesGenerateCommand
{
  public ZonesGenerateCommand()
  {
    CommandWrapper.Register("zones_generate", index =>
    {
      return FiltererParameters.Parameters;
    }, FiltererParameters.GetAutoComplete());
    Helper.Command("zones_generate", "[...args] [empty] - Pre-generates areas without having to visit them.", (args) =>
    {
      FiltererParameters pars = new(args)
      {
        TargetZones = TargetZones.Ungenerated,
        SafeZones = 0
      };
      var empty = Parse.Flag(pars.Unhandled, "empty");
      if (!pars.Valid(args.Context)) return;
      if (Helper.IsClient(args)) return;
      Executor.AddOperation(new Generate(args.Context, pars, empty), pars.Start);
    }, () => [.. Enum.GetNames(typeof(Heightmap.Biome))]);
  }
}
