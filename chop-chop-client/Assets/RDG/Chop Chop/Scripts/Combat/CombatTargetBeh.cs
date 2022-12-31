using System;
using RDG.Chop_Chop.Scripts.Faction;
using UnityEngine;
using UnityEngine.Events;

namespace RDG.Chop_Chop.Scripts.Combat {
  
  [Serializable]
  public class CombatTargetConfig {
    public float startHealth = 100;
  }

  [Serializable]
  public class CombatTargetEvents {
    public UnityEvent<CombatTarget> onDeath;
    public UnityEvent<CombatTarget> onHit;
    public UnityEvent<CombatTarget> onHealthChange;

    public void Release() {
      onDeath.RemoveAllListeners();
      onHit.RemoveAllListeners();
      onHealthChange.RemoveAllListeners();
    }
  }

  public enum CombatTargetState {
    Alive, Dead
  }
  public class CombatTargetBeh : MonoBehaviour, CombatTarget {

    [SerializeField] private CombatSo combat;
    [SerializeField] private FactionSo faction;
    
    [SerializeField] private Collider target;
    [SerializeField] private CombatTargetConfig config;
    [SerializeField] private CombatTargetEvents events;

    private CombatTargetState state;
    public  float CurrentHealth { get; private set; }
    
    public Guid ID { get; private set; }
    public FactionSo Faction => faction;
    public Collider Collider => target;

    public CombatTargetEvents Events => events;

    public void Start() {
      ID = Guid.NewGuid();
      state = CombatTargetState.Alive;
      CurrentHealth = config.startHealth;
      if (target == null) {
        target = GetComponentInChildren<Collider>();
      }
      if (target == null) {
        throw new Exception("combat target requires assigned or nested collider");
      }
      
      combat.AddTarget(this);
      events.onHealthChange.Invoke(this);
    }

    public void OnDestroy() {
      combat.RemoveTarget(this);
    }


    public bool TakeAttack(float damage) {
      if (damage <= 0 || state == CombatTargetState.Dead || !isActiveAndEnabled) {
        return false;
      }
      
      CurrentHealth = Math.Max(0, CurrentHealth - damage);
      if (damage > 0) {
        events.onHealthChange?.Invoke(this);
      }
      events.onHit?.Invoke(this);
      if (CurrentHealth > 0) {
        return true;
      }

      state = CombatTargetState.Dead;
      events.onDeath?.Invoke(this);
      return true;
    }
  }
}
