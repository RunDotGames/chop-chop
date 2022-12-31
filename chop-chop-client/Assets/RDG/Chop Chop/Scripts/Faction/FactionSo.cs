using System;
using UnityEngine;

namespace RDG.Chop_Chop.Scripts.Faction {
  [CreateAssetMenu(menuName = "RDG/ChopChop/Faction")]
  public class FactionSo : ScriptableObject {
    public readonly  Guid ID = Guid.NewGuid();
  }
}
