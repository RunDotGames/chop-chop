using System;
using RDG.Chop_Chop.Scripts.Faction;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;
  
namespace RDG.Chop_Chop.Scripts.Combat {
  
  [Serializable]
  public class CombatTargetConfig {
    public float startHealth = 100;
    public bool isTargetable =  true;
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

    [SerializeField] private GameObject root;
    
    [SerializeField] private CombatSo combat;
    [SerializeField] private FactionedBeh faction;
    
    [SerializeField] private Collider target;
    [SerializeField] private CombatTargetConfig config;
    [SerializeField] private CombatTargetEvents events;


    [SerializeField, ReadOnlyAttribute] private float currentHealth;
    
    private CombatTargetState state;
    public float CurrentHealth => currentHealth;
    
    public Guid ID { get; private set; }
    public FactionSo Faction => faction.Faction;
    public Collider Collider => target;

    public CombatTargetEvents Events => events;

    public void Awake() {
      ID = Guid.NewGuid();
      state = CombatTargetState.Alive;
      currentHealth = config.startHealth;
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


    public void TakeAttack(float damage) {
      if (damage <= 0 || !IsTargetable) {
        return;
      }
      
      currentHealth = Math.Max(0, CurrentHealth - damage);
      if (damage > 0) {
        events.onHealthChange?.Invoke(this);
      }
      Debug.Log("im hit");
      events.onHit?.Invoke(this);
      if (CurrentHealth > 0) {
        return;
      }

      state = CombatTargetState.Dead;
      events.onDeath?.Invoke(this);
      return;
    }
    public bool IsTargetable => config.isTargetable && state != CombatTargetState.Dead && isActiveAndEnabled;
    public GameObject Root => root;
  }
}
