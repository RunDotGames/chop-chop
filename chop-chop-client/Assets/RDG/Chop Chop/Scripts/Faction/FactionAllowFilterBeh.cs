using System.Collections.Generic;
using RDG.Chop_Chop.Scripts.Util;
using UnityEngine;

namespace RDG.Chop_Chop.Scripts.Faction {
  public class FactionAllowFilterBeh : MonoBehaviour, GameObjectFilter {

    public List<FactionSo> allowed = new List<FactionSo>();
    public bool Check(GameObject go) {
      var faction = go.GetComponentInChildren<FactionedBeh>();
      return faction != null && allowed.Contains(faction.Faction);
    }
  }
}
