using System;
using UnityEngine;

namespace RDG.Chop_Chop.Scripts.Animation {
  public class AnimSpeedNormalizer {
    
    private readonly Animator anim;
    
    private readonly float targetAnimDuration;
    
    private float priorSpeed = -1;

    public AnimSpeedNormalizer(Animator anim, string targetAnimName, float trimSeconds) {
      this.anim = anim;
      targetAnimDuration = -1.0f;
      foreach (var clip in anim.runtimeAnimatorController.animationClips) {
        if (clip.name != targetAnimName) {
          continue;
        }
        
        targetAnimDuration =  Math.Max(.01f, clip.length - trimSeconds);
        return;
      }
      
      throw new Exception($"Animation {targetAnimName} not found");
    }
    
    public void Apply(float duration) {
      if (priorSpeed >= 0) {
        throw new Exception($"Normalizer Re-applied without release");
      }
      
      priorSpeed = anim.speed;
      anim.speed =  Math.Max(.01f, targetAnimDuration / duration);
    }
    public void Release() {
      if (priorSpeed < 0) {
        throw new Exception($"Normalizer released without apply");
      }
      
      anim.speed = priorSpeed;
      priorSpeed = -1;
    }
  }
}
