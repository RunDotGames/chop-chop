using UnityEngine;

namespace RDG.Chop_Chop.Scripts.Animation {
  public class AnimationTriggerDefaultBeh : MonoBehaviour, AnimationTrigger {

    [SerializeField] private string animationName;

    public string AnimName => animationName;
    public bool Eval(Animator _) {
      return true;
    }
    public void OnExit(Animator _) {}
    public void OnEnter(Animator _) {}
  }
}
