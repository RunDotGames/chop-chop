using System;
using System.Collections.Generic;
using RDG.UnityUtil;
using UnityEngine;

namespace RDG.Chop_Chop.Scripts.Animation {
  public class AnimationTriggersBeh : MonoBehaviour {

    private Animator anim;
    
    private readonly List<AnimationTrigger> triggers = new();
    
    private AnimationTrigger current;


    public void AddTrigger(AnimationTrigger trigger) {
      triggers.Add(trigger);
    }

    public Animator Anim => anim;

    public void Awake() {
      anim = GetComponentInChildren<Animator>();
      if (anim == null) {
        throw new Exception("Anim Triggers requires child animator");
      }
    }
    
    public void Update() {
      foreach (var trigger in triggers) {
        if (!trigger.Eval(anim)) {
          continue;
        }
        
        if (trigger == current) {
          return;
        }

        if (current is AnimationTriggerEvented preEvented) {
          preEvented.OnExit(anim);
        }
        current = trigger;
        var animOffset = float.NegativeInfinity;
        if (current is AnimationTriggerOffset offset) {
          animOffset = offset.AnimOffset;
        }
        anim.Play(trigger.AnimName, -1, animOffset);
        if (current is AnimationTriggerEvented postEvented) {
          postEvented.OnEnter(anim);
        }
        return;
      }
    }
  }
}
