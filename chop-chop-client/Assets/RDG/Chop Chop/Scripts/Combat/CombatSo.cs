using System;
using System.Collections.Generic;
using RDG.Chop_Chop.Scripts.Faction;
using UnityEngine;

namespace RDG.Chop_Chop.Scripts.Combat {

  public interface CombatTarget {
    
    public Guid ID { get; }
    public FactionSo Faction { get; }
    public Collider Collider { get; }

    public void TakeAttack(float damage);
    public bool IsTargetable { get; }
    
    public GameObject Root { get; }
  }
   
  [Serializable]
  public class CombatConfig {
    public string combatLayerName;
    public bool drawDebug;
  }
  
  [CreateAssetMenu(menuName ="RDG/ChopChop/Combat" )]
  public class CombatSo: ScriptableObject {

    [SerializeField] private CombatConfig config;

    private bool isEnabled;
    
    private readonly Dictionary<string, CombatTarget> nameToTarget = new Dictionary<string, CombatTarget>();
    public int HitLayer { get; private set; }
    public bool DrawDebug => config.drawDebug;


    public void AddTarget(CombatTarget target) {
      target.Collider.gameObject.layer = LayerMask.NameToLayer(config.combatLayerName);
      target.Collider.name = $"CombatTarget-{target.ID}";
      nameToTarget[target.Collider.name] = target;
    }

    public void RemoveTarget(CombatTarget target) {
      nameToTarget.Remove(target.Collider.name);
    }

    public CombatTarget GetTarget(Collider collider) {
      if (collider == null || !isEnabled) {
        return null;
      }
      
      return nameToTarget.TryGetValue(collider.name, out var target) ? target : null;
    }
    public void Enable() {
      isEnabled = true;
      HitLayer = LayerMask.GetMask(config.combatLayerName);
    }

    public void Disable() {
      isEnabled = false;
      HitLayer = -1;
    }
  }
}
