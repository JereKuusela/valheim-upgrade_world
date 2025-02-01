using System;
using System.Linq;
using Service;

namespace UpgradeWorld;
public class ZonesGenerateCommand
{
  public ZonesGenerateCommand()
  {
    CommandWrapper.Register("zones_generate", (int index) =>
    {
      return FiltererParameters.Parameters;
    }, FiltererParameters.GetAutoComplete());
    new Terminal.ConsoleCommand("zones_generate", "[...args] [empty] - Pre-generates areas without having to visit them.", (args) =>
    {
      FiltererParameters pars = new(args)
      {
        TargetZones = TargetZones.Ungenerated,
        SafeZones = 0
      };
      var empty = Parse.Flag(pars.Unhandled, "empty");
      if (!pars.Valid(args.Context)) return;
      if (Helper.IsClient(args)) return;
      Executor.AddOperation(new Generate(args.Context, pars, empty));
    }, optionsFetcher: () => [.. Enum.GetNames(typeof(Heightmap.Biome))]);
  }
}
