using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RDG.Chop_Chop.Scripts.Animation {
  public class AnimationTriggersBeh : MonoBehaviour {

    [SerializeField] private Animator anim;
    private readonly List<AnimationTrigger> triggers = new();
    
    private AnimationTrigger current;


    public void AddTrigger(AnimationTrigger trigger) {
      triggers.Add(trigger);
    }

    public Animator Anim => anim;

    public void Awake() {
      if (anim != null) {
        return;
      }

      anim = GetComponentInChildren<Animator>();
      if (anim == null) {
        throw new Exception("animation triggers requires assigned or nested animator");
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
        
        current?.OnExit(anim);
        current = trigger;
        current.OnEnter(anim);
        anim.Play(trigger.AnimName);
        return;
      }
    }
  }
}
