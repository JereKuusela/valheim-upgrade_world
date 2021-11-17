using System.Collections.Generic;
using System.Linq;
namespace UpgradeWorld {
  ///<summary>Base class for entity related operations. Provides some utilities.</summary>
  public abstract class EntityOperation : BaseOperation {
    protected EntityOperation(Terminal context) : base(context) {
    }
    protected bool Validate(IEnumerable<string> ids) {
      if (ids.Count() == 0) {
        Print("Error: Missing ids");
        return false;
      }
      var invalidIds = ids.Where(id => ZNetScene.instance.GetPrefab(id) == null);
      if (invalidIds.Count() > 0) {
        Print("Error: Invalid entity ids " + string.Join(", ", invalidIds));
        return false;
      }
      return true;
    }
    protected IEnumerable<ZDO> GetZDOs(string id, FiltererParameters args) {
      var code = id.GetStableHashCode();
      var zdos = ZDOMan.instance.m_objectsByID.Values.Where(zdo => zdo.GetPrefab() == code);
      return FilterZdos(zdos, args);
    }
    protected IEnumerable<ZDO> FilterZdos(IEnumerable<ZDO> zdos, FiltererParameters args) => args.FilterZdos(zdos);
  }
}
