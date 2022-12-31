using UnityEngine;

namespace RDG.Chop_Chop.Scripts.Animation {
  public interface AnimationTrigger {
    string AnimName { get; }

    bool Eval(Animator anim);

    void OnExit(Animator anim);

    void OnEnter(Animator anim);
  }
}
