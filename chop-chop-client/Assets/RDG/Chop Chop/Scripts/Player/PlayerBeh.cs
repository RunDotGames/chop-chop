using System.Collections.Generic;
using System.Linq;
using RDG.Chop_Chop.Scripts.Animation;
using RDG.Chop_Chop.Scripts.Camera;
using RDG.Chop_Chop.Scripts.Combat;
using RDG.Chop_Chop.Scripts.Movement;
using UnityEngine;
using RDG.UnityInput;
namespace RDG.Chop_Chop.Scripts.Player {
  public class PlayerBeh : MonoBehaviour, MotorDirector, CameraFollowable {

    [SerializeField] private KeyActionsSo keyActions;
    [SerializeField] private MovementSo movement;
    [SerializeField] private CombatSo combat;
    [SerializeField] private CameraSo cameras;
    [SerializeField] private MotorConfig motorConfig;
    [SerializeField] private MotorAnimatorConfig motorAnimConfig;
    [SerializeField] private CombatAttackConfig attackerConfig;

    private Vector2 currentDirectionX;
    private Vector2 currentDirectionY;
    private KeyActionStack xStack;
    private KeyActionStack yStack;
    private KeyActionFilter jumpFilter;
    private KeyActionFilter attackFilter;
    private Motor motor;
    private AnimationTriggerController anim;
    private bool isJumping;

    private static readonly Dictionary<KeyAction, Vector2> XAxisMap = new Dictionary<KeyAction, Vector2>(){
      {
        KeyAction.MoveLeft, Vector2.left
      },{
        KeyAction.MoveRight, Vector2.right
      },
    };
    
    
    private static readonly Dictionary<KeyAction, Vector2> YAxisMap = new Dictionary<KeyAction, Vector2>(){
      {
        KeyAction.MoveForward, Vector2.up
      },{
        KeyAction.MoveBack, Vector2.down
      }
    };
    private CombatAttacker attacker;
    private CombatTarget target;
    

    public void Start() {
      motor = movement.NewMotor(motorConfig, GetComponentInChildren<Rigidbody>(), this);
      var animator = GetComponentInChildren<Animator>();
      anim = new AnimationTriggerController(animator,
        MotorAnimTriggers.GetTriggers(motor, motorAnimConfig)
      );
      
      jumpFilter = new KeyActionFilter(keyActions, new []{ KeyAction.Translate });
      jumpFilter.OnDown += (action) => isJumping = true;
      jumpFilter.OnUp += (action) => isJumping = false;
      
      xStack = new KeyActionStack(keyActions, XAxisMap.Keys.ToArray());
      xStack.OnStackChange += HandleXMovementChange;
      
      yStack = new KeyActionStack(keyActions, YAxisMap.Keys.ToArray());
      yStack.OnStackChange += HandleYMovementChange;
      
      cameras.SetFollowed(this);

      attacker = combat.NewAttacker(attackerConfig, animator, this);
      anim.Add(attacker.AnimTrigger);
      target = combat.NewTarget();

      attackFilter = new KeyActionFilter(keyActions, new[]{
        KeyAction.Interact
      });
      attackFilter.OnDown += HandleAttack;
    }
    private void HandleAttack(KeyAction obj) {
      motor.Disable();
      attacker.Attack().ContinueWith(_ => {
        motor.Enable();
      });
    }

    public void OnDestroy() {
      xStack?.Release();
      yStack?.Release();
      jumpFilter?.Release();
      attackFilter?.Release();
    }
    
    private void HandleXMovementChange(KeyActionStack.State state) {
      if (state.Size == 0 || state.Top == KeyAction.None) {
        currentDirectionX = Vector2.zero;
        return;
      }
      currentDirectionX = XAxisMap[state.Top];
    }
    
    private void HandleYMovementChange(KeyActionStack.State state) {
      if (state.Size == 0 || state.Top == KeyAction.None) {
        currentDirectionY = Vector2.zero;
        return;
      }
      currentDirectionY = YAxisMap[state.Top];
    }
    
    public Vector2 GetDirection() {
      var inCamSpace = cameras.GameCamera.TransformVector(new Vector3(currentDirectionX.x, 0, currentDirectionY.y));
      return new Vector2(inCamSpace.x, inCamSpace.z).normalized;
    }

    public bool GetJumpRequested() {
      return isJumping;
    }

    public void FixedUpdate() {
      motor?.FixedUpdate();
    }

    public void Update() {
      anim?.Eval();
    }
    public Vector3 GetTrackingPoint() {
      return motor?.GetTrackingPoint() ?? transform.position;
    }
  }
}
