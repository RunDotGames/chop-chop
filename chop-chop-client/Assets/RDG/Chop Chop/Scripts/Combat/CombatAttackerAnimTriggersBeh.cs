using System;
using RDG.Chop_Chop.Scripts.Animation;
using UnityEngine;

namespace RDG.Chop_Chop.Scripts.Combat {
  
  
  public class CombatAttackerAnimTrigger : AnimationTrigger, AnimationTriggerEvented {
    private readonly float attackAnimSpeed;
    private readonly CombatAttackerBeh attacker;
    private readonly AnimSpeedNormalizer normalizer;
    

    public CombatAttackerAnimTrigger(Animator anim, string attackAnimName, CombatAttackerBeh attacker) {
      normalizer = new AnimSpeedNormalizer(anim, attackAnimName, 0);
      AnimName = attackAnimName;
      this.attacker = attacker;
    }

    public string AnimName { get; }
    public bool Eval(Animator _) {
      return attacker.State is CombatAttackerState.AttackPre or CombatAttackerState.AttackPost;
    }

    public void OnEnter(Animator _) {
      normalizer.Apply(attacker.AttackSpeed);
    }
    public void OnExit(Animator _) {
      normalizer.Release();
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
