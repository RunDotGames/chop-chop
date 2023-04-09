using UnityEngine;

namespace RDG.Chop_Chop.Scripts.Animation {
  public interface AnimationTrigger {
    string AnimName { get; }
    
    // Should become or remain active
    bool Eval(Animator anim);


  }

  public interface AnimationTriggerOffset {
    // Percent offset to start the anim from
    float AnimOffset { get; }
  }

  public interface AnimationTriggerEvented {
    // Another trigger has become active, this trigger is no longer active
    void OnExit(Animator anim);

    // This trigger has become active
    void OnEnter(Animator anim);
  }
}
