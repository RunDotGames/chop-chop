
using System;
using RDG.Chop_Chop.Scripts.Animation;
using UnityEngine;

namespace RDG.Chop_Chop.Scripts.Movement {
  
  [Serializable]
  public class MotorAnimatorConfig {
    public string idleAnimName = "Idle";
    public string runningAnimName = "Running";
  }

  public class MotorRunAnimTrigger : AnimationTrigger {
    private readonly MotorBeh motor;
    private readonly MotorAnimatorConfig config;

    public MotorRunAnimTrigger(MotorBeh motor, MotorAnimatorConfig config) {
      this.motor = motor;
      this.config = config;

    }
    public string AnimName => config.runningAnimName;
    public bool Eval(Animator _) {
      return !motor.IsRequestLocked && motor.LastMoveDirection.sqrMagnitude > 0;
    }
    
    public void OnExit(Animator _) {}
    public void OnEnter(Animator _){}
  }
  
  public class MotorIdleAnimTrigger : AnimationTrigger {
    private readonly MotorBeh motor;
    private readonly MotorAnimatorConfig config;

    public MotorIdleAnimTrigger(MotorBeh motor, MotorAnimatorConfig config) {
      this.motor = motor;
      this.config = config;

    }
    public string AnimName => config.idleAnimName;
    public bool Eval(Animator _) {
      return motor.IsRequestLocked || motor.LastMoveDirection.sqrMagnitude == 0;
    }
    
    public void OnExit(Animator _){}
    public void OnEnter(Animator _){}
  }

  public class MotorAnimTriggersBeh : MonoBehaviour {
    

    [SerializeField] private MotorBeh motor;
    [SerializeField] private AnimationTriggersBeh anim;
    [SerializeField] private MotorAnimatorConfig config;

    public void Start() {
     anim.AddTrigger(new MotorIdleAnimTrigger(motor, config));
     anim.AddTrigger(new MotorRunAnimTrigger(motor, config));
    }
  }
  
}
