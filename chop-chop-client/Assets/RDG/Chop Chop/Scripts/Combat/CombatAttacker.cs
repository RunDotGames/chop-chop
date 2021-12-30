using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RDG.Chop_Chop.Scripts.Animation;
using UnityEngine;

namespace RDG.Chop_Chop.Scripts.Combat {

  [Serializable]
  public class CombatAttackConfig {
    public string attackAnimName = "Attack";
    public float attackSpeed = 1.2f;
    public float attackDelayFactor = 0.5f;
  }

  public enum CombatAttackerState {
    Idle, Locked, AttackPre, AttackPost
  }

  public class CombatAttackerAnimTrigger : AnimationTrigger {
    private readonly float attackAnimSpeed;
    private readonly CombatAttackConfig config;
    private readonly Animator anim;
    private readonly MonoBehaviour parent;

    private float priorSpeed;
    private bool triggered;

    public CombatAttackerAnimTrigger(float attackAnimSpeed, CombatAttackConfig config, Animator anim, MonoBehaviour parent) {
      this.attackAnimSpeed = attackAnimSpeed;
      this.config = config;
      this.anim = anim;
      this.parent = parent;
    }

    public Task Trigger() {
      triggered = true;
      var source = new TaskCompletionSource<bool>();
      parent.StartCoroutine(WaitForAttack(source));
      return source.Task;
    }

    private IEnumerator<YieldInstruction> WaitForAttack(TaskCompletionSource<bool> source) {
      yield return new WaitForSeconds(config.attackSpeed);
      triggered = false;
      source.SetResult(true);
    }
    public string AnimName => config.attackAnimName;
    public bool Eval() {
      if (!triggered) {
        return false;
      }

      triggered = false;
      priorSpeed = anim.speed;
      anim.speed =  attackAnimSpeed / config.attackSpeed;
      return true;
    }
    public void OnExit() {
      anim.speed = priorSpeed;
    }
  }
  
  public class CombatAttacker {

    public event Action OnAttackDone;
    
    public CombatAttackerState State { get; private set; }
    public CombatAttackerAnimTrigger AnimTrigger { get; }

    public CombatAttacker(CombatAttackConfig attackConfig, Animator anim, MonoBehaviour parent) {
      if (attackConfig.attackDelayFactor >= 1.0f || attackConfig.attackDelayFactor <= 0) {
        throw new Exception($"attack delay factor invalid {attackConfig.attackDelayFactor}");
      }
      
      State = CombatAttackerState.Idle;
      var attackAnimSpeed = -1.0f;
      foreach (var clip in anim.runtimeAnimatorController.animationClips) {
        if (clip.name != attackConfig.attackAnimName) {
          continue;
        }
        
        attackAnimSpeed = clip.length;
        break;
      }
      if (attackAnimSpeed < 0.0f) {
        throw new Exception($"Attack Animation {attackConfig.attackAnimName} not found");
      }

      AnimTrigger = new CombatAttackerAnimTrigger(attackAnimSpeed, attackConfig, anim, parent);
    }

    public Task Attack() {
      if (State != CombatAttackerState.Idle) {
        return Task.CompletedTask;
      }
      
      State = CombatAttackerState.AttackPre;
      return AnimTrigger.Trigger().ContinueWith(_ => {
        OnAttackDone?.Invoke();
        State = CombatAttackerState.Idle;
      });
    }

    public void Release() {
      OnAttackDone = null;
    }
  }
}
