using UnityEngine;

namespace RDG.Chop_Chop.Scripts.Animation {
  public interface AnimationTrigger {
    string AnimName { get; }

    // Should become or remain active
    bool Eval(Animator anim);

    // Another trigger has become active, this trigger is no longer active
    void OnExit(Animator anim);

    // This trigger has become active
    void OnEnter(Animator anim);
  }
}
