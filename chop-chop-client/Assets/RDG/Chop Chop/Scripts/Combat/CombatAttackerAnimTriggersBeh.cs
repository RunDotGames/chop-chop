using System;
using RDG.Chop_Chop.Scripts.Animation;
using UnityEngine;

namespace RDG.Chop_Chop.Scripts.Combat {
  
  
  public class CombatAttackerAnimTrigger : AnimationTrigger {
    private readonly float attackAnimSpeed;
    private readonly CombatAttackerBeh attacker;
    
    private float priorSpeed;
    
    public CombatAttackerAnimTrigger(Animator anim, string attackAnimName, CombatAttackerBeh attacker) {
      AnimName = attackAnimName;
      attackAnimSpeed = -1.0f;
      foreach (var clip in anim.runtimeAnimatorController.animationClips) {
        if (clip.name != attackAnimName) {
          continue;
        }
        
        attackAnimSpeed = clip.length;
        break;
      }
      if (attackAnimSpeed < 0.0f) {
        throw new Exception($"Attack Animation {attackAnimName} not found");
      }

      this.attacker = attacker;
    }

    public string AnimName { get; }
    public bool Eval(Animator _) {
      return attacker.State is CombatAttackerState.AttackPre or CombatAttackerState.AttackPost;
    }

    public void OnEnter(Animator anim) {
      priorSpeed = anim.speed;
      anim.speed =  attackAnimSpeed / attacker.AttackSpeed;
    }
    public void OnExit(Animator anim) {
      anim.speed = priorSpeed;
    }
  }
  public class CombatAttackerAnimTriggersBeh : MonoBehaviour {
    
    [SerializeField] private AnimationTriggersBeh anim;
    [SerializeField] private CombatAttackerBeh attacker;
    [SerializeField] private string attackAnimName = "Attack";
    
    public void Awake() {
      var animator = GetComponentInChildren<Animator>();
      anim.AddTrigger(new CombatAttackerAnimTrigger(
        animator,
        attackAnimName,
        attacker)
      );
    }
  }
}
