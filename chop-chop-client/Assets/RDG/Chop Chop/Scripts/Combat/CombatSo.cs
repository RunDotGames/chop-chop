using System;
using UnityEngine;

namespace RDG.Chop_Chop.Scripts.Combat {


  public class CombatTarget {
    
  }
  [Serializable]
  public class CombatConfig {
    
  }
  
  [CreateAssetMenu(menuName ="RDG/ChopChop/Combat" )]
  public class CombatSo: ScriptableObject {

    [SerializeField] private CombatConfig config;

    public CombatAttacker NewAttacker(CombatAttackConfig attackerConfig, Animator anim, MonoBehaviour parent) {
      return new CombatAttacker(attackerConfig, anim, parent);
    }

    public CombatTarget NewTarget() {
      return new CombatTarget();
    }
  }
}
