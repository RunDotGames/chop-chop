
using System;
using RDG.Chop_Chop.Scripts.Animation;
using UnityEngine;

namespace RDG.Chop_Chop.Scripts.Movement {
  
  [Serializable]
  public class MotorAnimatorConfig {
    public string idleAnimName = "Idle";
    public string runningAnimName = "Running";
    public string dashAnimName = "Dashing";
    public string jumpUpAnimName = "JumpUp";
    public string fallDownAnimName = "FallDown";
    public float dashAnimTrimSeconds = 0.25f;
    public float dashAnimOffset = .1f;
    public float jumpUpAnimOffset = .1f;
  }

  public class FallDownAnimTrigger : AnimationTrigger {
    private readonly MotorBeh motor;
    private readonly MotorAnimatorConfig config;
    public FallDownAnimTrigger(MotorBeh motor, MotorAnimatorConfig config) {
      this.motor = motor;
      this.config = config;
    }
    public string AnimName => config.fallDownAnimName;
    
    public bool Eval(Animator _) {
      return (motor.Jump is MotorBeh.JumpState.Exhausted or MotorBeh.JumpState.Able) && !motor.IsGrounded;
    }
  }
  public class JumpUpAnimTrigger : AnimationTrigger, AnimationTriggerOffset {
    private readonly MotorBeh motor;
    private readonly MotorAnimatorConfig config;
    public JumpUpAnimTrigger(MotorBeh motor, MotorAnimatorConfig config) {
      this.motor = motor;
      this.config = config;
    }
    public string AnimName => config.jumpUpAnimName;
    
    public bool Eval(Animator _) {
      return motor.Jump == MotorBeh.JumpState.InProgress;
    }
    public float AnimOffset => config.jumpUpAnimOffset;
  }
  public class MotorDashAnimTrigger : AnimationTrigger, AnimationTriggerEvented, AnimationTriggerOffset {
    private readonly MotorBeh motor;
    private readonly MotorAnimatorConfig config;
    private readonly AnimSpeedNormalizer normalizer;

    public MotorDashAnimTrigger(MotorBeh motor, MotorAnimatorConfig config, Animator anim) {
      normalizer = new AnimSpeedNormalizer(anim, config.dashAnimName, config.dashAnimTrimSeconds);
      this.motor = motor;
      this.config = config;
    }
    public string AnimName => config.dashAnimName;
    
    public bool Eval(Animator _) {
      return !motor.IsRequestLocked && motor.IsDashing;
    }

    public void OnExit(Animator _) {
      normalizer.Release();
    }
    public void OnEnter(Animator _) {
      normalizer.Apply(motor.DashDuration);
    }
    public float AnimOffset => config.dashAnimOffset;
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
  }

  public class MotorAnimTriggersBeh : MonoBehaviour {
    

    [SerializeField] private MotorBeh motor;
    [SerializeField] private AnimationTriggersBeh anim;
    [SerializeField] private MotorAnimatorConfig config;

    public void Start() {
      var animator = GetComponentInChildren<Animator>();
      anim.AddTrigger(new MotorDashAnimTrigger(motor, config, animator));
      anim.AddTrigger(new JumpUpAnimTrigger(motor, config));
      anim.AddTrigger(new FallDownAnimTrigger(motor, config));
      anim.AddTrigger(new MotorIdleAnimTrigger(motor, config));
      anim.AddTrigger(new MotorRunAnimTrigger(motor, config));
    }
  }
  
}
