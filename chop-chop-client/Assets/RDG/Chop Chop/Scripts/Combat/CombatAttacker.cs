using System;
using System.Diagnostics;
using System.Threading.Tasks;
using RDG.Chop_Chop.Scripts.Faction;
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
    public UnityEvent<CombatAttacker> onAttackStart;
    public UnityEvent<CombatAttacker, CombatTarget> onAttackStrike;
    public UnityEvent<CombatAttacker> onAttackStop;

    public void Release() {
      onAttackStart.RemoveAllListeners();
      onAttackStop.RemoveAllListeners();
      onAttackStrike.RemoveAllListeners();
    }
  }

  public class CombatAttackerParameters {
    public CombatAttackerParameters(CombatAttackConfig config, CombatAttackEvents events, Transform root, MonoBehaviour parent, FactionSo faction) {
      Config = config;
      Events = events;
      Root = root;
      Parent = parent;
      Faction = faction;
    }
    public CombatAttackEvents Events { get; }
    public CombatAttackConfig Config { get; }
    public Transform Root { get; }
    public MonoBehaviour Parent { get;}
    public FactionSo Faction { get; }
  }

  public class CombatAttackerInterface {
    public CombatAttackerInterface(int hitLayer, Func<Collider, float, FactionSo, CombatTarget> processAttack, bool drawDebug) {
      HitLayer = hitLayer;
      ProcessAttack = processAttack;
      DrawDebug = drawDebug;
    }
    public int HitLayer { get; }
    public Func<Collider, float, FactionSo, CombatTarget> ProcessAttack { get; }
    public bool DrawDebug { get; }
  }
  
  public class CombatAttacker {
    private readonly CombatAttackerParameters combatParams;
    private readonly CombatAttackerInterface combatInterface;

    private static readonly Collider[] HitCache = new Collider[100];
    
    public CombatAttackerState State { get; private set; }
    
    public CombatAttacker(CombatAttackerParameters combatParams, CombatAttackerInterface combatInterface) {
      if (combatParams.Config.attackDelayFactor >= 1.0f || combatParams.Config.attackDelayFactor <= 0) {
        throw new Exception($"attack delay factor invalid {combatParams.Config.attackDelayFactor}");
      }
      this.combatParams = combatParams;
      this.combatInterface = combatInterface;
      
      State = CombatAttackerState.Idle;
    }

    public async Task<bool> Attack() {
      //We can only attack if we are ready to
      if (State != CombatAttackerState.Idle) {
        return false;
      }
      
      State = CombatAttackerState.AttackPre;
      combatParams.Events.onAttackStart?.Invoke(this);
      await TaskUtils.WaitCoroutine(combatParams.Parent, combatParams.Config.attackSpeed * combatParams.Config.attackDelayFactor);
      if (State != CombatAttackerState.AttackPre) {
        return false;
      }
      
      State = CombatAttackerState.AttackPost;
      var hitCount = Physics.OverlapSphereNonAlloc(
        combatParams.Root.position,
        combatParams.Config.attackDistance,
        HitCache,
        combatInterface.HitLayer
      );
      Debug.Log(hitCount);
      for (var i = 0; i < hitCount; i++) {
        var collider = HitCache[i];
        var rootPosition = combatParams.Root.position;
        var direction = collider.bounds.ClosestPoint(rootPosition) - rootPosition;
        var angle = Mathf.Abs(Vector3.Angle(combatParams.Root.forward, direction.normalized));
        if (angle > 180.0f) {
          angle = 360.0f - angle;
        }
        
        if (angle > combatParams.Config.attackAngle) {
          continue;
        }
        Debug.Log("SEND");
        var target = combatInterface.ProcessAttack(collider, combatParams.Config.attackDamage, combatParams.Faction);
        combatParams.Events.onAttackStrike?.Invoke(this, target);
      }
      await TaskUtils.WaitCoroutine(combatParams.Parent, combatParams.Config.attackSpeed * (1 - combatParams.Config.attackDelayFactor));
      if (State != CombatAttackerState.AttackPost) {
        return false;
      }
      
      combatParams.Events.onAttackStop?.Invoke(this);
      State = CombatAttackerState.Idle;
      return true;
    }
  

    public void Disable() {
      if (State is CombatAttackerState.AttackPre or CombatAttackerState.AttackPost) {
        combatParams.Events.onAttackStop?.Invoke(this);
      }
      State = CombatAttackerState.Locked;
    }

    public void Enable() {
      State = CombatAttackerState.Idle;
    }

    public void Update() {
      if (!combatInterface.DrawDebug) {
        return;
      }
      var distance = combatParams.Config.attackDistance;
      var forward = combatParams.Root.forward * distance;
      var root = combatParams.Root;
      var left = Quaternion.AngleAxis(combatParams.Config.attackAngle, Vector3.up);
      var right = Quaternion.AngleAxis(-combatParams.Config.attackAngle, Vector3.up);
      var position = root.position;
      Debug.DrawRay(position, forward, Color.red);
      Debug.DrawRay(position, left * forward, Color.red);
      Debug.DrawRay(position, right * forward, Color.red);
    }

    public float AttackSpeed => combatParams.Config.attackSpeed;

    public void Release() {
      combatParams.Events.Release();
      State = CombatAttackerState.Idle;
    }
  }
}
