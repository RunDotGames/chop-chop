using System;
using System.Collections.Generic;
using RDG.Chop_Chop.Scripts.Faction;
using RDG.Chop_Chop.Scripts.Util;
using RDG.UnityUtil;
using UnityEngine;
using UnityEngine.Events;
using Debug = UnityEngine.Debug;

namespace RDG.Chop_Chop.Scripts.Combat {
  
  public enum CombatAttackerState {
    Idle, Locked, AttackPre, AttackPost
  }

  [Serializable]
  public class CombatAttackConfig {
    public float attackSpeed = 1.2f;
    public float attackDelayFactor = 0.5f;
    public float attackDistance = 1.0f;
    public float attackDamage = 100;
    public float attackAngle = 30;
  }

  [Serializable]
  public class CombatAttackEvents {
    public UnityEvent<GameObject> onAttackStart;
    public UnityEvent<GameObject, GameObject> onAttackStrike; //attacker, attacked
    public UnityEvent<GameObject> onAttackStop;
    public UnityEvent<GameObject, bool> onPossibleTargetsModified;
    
    public void Release() {
      onAttackStart.RemoveAllListeners();
      onAttackStop.RemoveAllListeners();
      onAttackStrike.RemoveAllListeners();
      onPossibleTargetsModified.RemoveAllListeners();
    }
  }

  public class CombatAttackerBeh : MonoBehaviour {
    [SerializeField] private CombatSo combat;
    [SerializeField] private FactionedBeh faction;
    [SerializeField] private CombatAttackConfig config;
    [SerializeField] private CombatAttackEvents events;
    
    public CombatAttackerState State { get; private set; }
    
    public float AttackSpeed => config.attackSpeed;

    private Dictionary<CombatTarget, GameObject> possibleTargets = new();

    public void Awake() {
      if (config.attackDelayFactor >= 1.0f || config.attackDelayFactor <= 0) {
        throw new Exception($"attack delay factor invalid {config.attackDelayFactor}");
      }
      State = CombatAttackerState.Idle;
      possibleTargets.Clear();
    }
    
    public async void Attack() {
      //We can only attack if we are ready to
      if (State != CombatAttackerState.Idle) {
        return;
      }
      
      State = CombatAttackerState.AttackPre;
      events.onAttackStart.Invoke(gameObject);
      await TaskUtils.WaitCoroutine(this, config.attackSpeed * config.attackDelayFactor);
      if (State != CombatAttackerState.AttackPre) { //if we got interrupted during await
        return;
      }
      
      State = CombatAttackerState.AttackPost;
      foreach (var target in possibleTargets.Keys) {
        target.TakeAttack(config.attackDamage);
        events.onAttackStrike.Invoke(gameObject, target.Root);
      }
      await TaskUtils.WaitCoroutine(this, config.attackSpeed * (1 - config.attackDelayFactor));
      if (State != CombatAttackerState.AttackPost) { //if we got interrupted during await
        return;
      }
      
      events.onAttackStop?.Invoke(gameObject);
      State = CombatAttackerState.Idle;
    }
    
   
    private void GatherPossibleTargets() {
      var targets = new Dictionary<CombatTarget, GameObject>();
      
      var hitCount = Physics.OverlapSphereNonAlloc(
        transform.position,
        config.attackDistance,
        CacheUtil.Colliders,
        combat.HitLayer
      );
     
      for (var i = 0; i < hitCount; i++) {
        var aCollider = CacheUtil.Colliders[i];
        var rootPosition = transform.position;
        var direction = aCollider.bounds.ClosestPoint(rootPosition) - rootPosition;
        var angle = Mathf.Abs(Vector3.Angle(transform.forward, direction.normalized));
        if (angle > 180.0f) {
          angle = 360.0f - angle;
        }
        if (angle > config.attackAngle) {
          continue;
        }
        
        var target = combat.GetTarget(aCollider);
        if (target.Faction == faction.Faction || !target.IsTargetable) {
          continue;
        }

        targets.Add(target, target.Root);
        if (possibleTargets.ContainsKey(target)) {
          possibleTargets.Remove(target);
          continue;
        }
        
        events.onPossibleTargetsModified.Invoke(target.Root, true);
      }
      foreach (var target in possibleTargets.Values) {
        events.onPossibleTargetsModified.Invoke(target, false);
      }
      possibleTargets = targets;
    }


    public void OnDisable() {
      if (State != CombatAttackerState.Idle) {
        events.onAttackStop?.Invoke(gameObject);
      }
      State = CombatAttackerState.Locked;
    }

    public void OnEnable() {
      State = CombatAttackerState.Idle;
    }

    public void Update() {
      GatherPossibleTargets();
      if (!combat.DrawDebug) {
        return;
      }
      var distance = config.attackDistance;
      var forward = transform.forward * distance;
      var left = Quaternion.AngleAxis(config.attackAngle, Vector3.up);
      var right = Quaternion.AngleAxis(-config.attackAngle, Vector3.up);
      var position = transform.position;
      Debug.DrawRay(position, forward, Color.red);
      Debug.DrawRay(position, left * forward, Color.red);
      Debug.DrawRay(position, right * forward, Color.red);
    }

    public void OnDestroy() {
      events.Release();
      State = CombatAttackerState.Locked;
    }
    
  }
}
