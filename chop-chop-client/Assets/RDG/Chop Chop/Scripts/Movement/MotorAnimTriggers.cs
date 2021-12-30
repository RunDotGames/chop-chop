
using System;
using System.Collections.Generic;
using RDG.Chop_Chop.Scripts.Animation;

namespace RDG.Chop_Chop.Scripts.Movement {
  
  [Serializable]
  public class MotorAnimatorConfig {
    public string idleAnimName = "Idle";
    public string runningAnimName = "Running";
  }

  public class MotorRunAnimTrigger : AnimationTrigger {
    private readonly Motor motor;
    private readonly MotorAnimatorConfig config;

    public MotorRunAnimTrigger(Motor motor, MotorAnimatorConfig config) {
      this.motor = motor;
      this.config = config;

    }
    public string AnimName => config.runningAnimName;
    public bool Eval() {
      return !motor.IsDisabled && motor.GetLastDirectedMovement().sqrMagnitude > 0;
    }
    
    public void OnExit() {}
  }
  
  public class MotorIdleAnimTrigger : AnimationTrigger {
    private readonly Motor motor;
    private readonly MotorAnimatorConfig config;

    public MotorIdleAnimTrigger(Motor motor, MotorAnimatorConfig config) {
      this.motor = motor;
      this.config = config;

    }
    public string AnimName => config.idleAnimName;
    public bool Eval() {
      return !motor.IsDisabled && motor.GetLastDirectedMovement().sqrMagnitude == 0;
    }
    
    public void OnExit(){}
  }

  public static class MotorAnimTriggers {
    public static IEnumerable<AnimationTrigger> GetTriggers(Motor motor, MotorAnimatorConfig config) {
      return new AnimationTrigger[]{
        new MotorIdleAnimTrigger(motor, config),
        new MotorRunAnimTrigger(motor, config)
      };
    }
  }
  
}
