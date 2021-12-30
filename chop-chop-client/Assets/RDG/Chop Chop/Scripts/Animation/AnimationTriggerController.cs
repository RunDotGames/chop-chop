using System.Collections.Generic;
using UnityEngine;

namespace RDG.Chop_Chop.Scripts.Animation {

  public interface AnimationTrigger {
    string AnimName { get; }

    bool Eval();

    void OnExit();
  }
  
  public class AnimationTriggerController {
    
    private readonly Animator anim;
    private readonly List<AnimationTrigger> triggers = new List<AnimationTrigger>();

    private AnimationTrigger current;
    
    public AnimationTriggerController(Animator anim, IEnumerable<AnimationTrigger> fromTriggers) {
      this.anim = anim;
      triggers.AddRange(fromTriggers);
    }

    public void Add(AnimationTrigger trigger) {
      triggers.Add(trigger);
    }

    public void Eval() {
      foreach (var trigger in triggers) {
        if (trigger == current) {
          continue;
        }

        if (!trigger.Eval()) {
          continue;
        }
        
        current?.OnExit();
        current = trigger;
        anim.Play(trigger.AnimName);
        return;
      }
    }
  }
}
