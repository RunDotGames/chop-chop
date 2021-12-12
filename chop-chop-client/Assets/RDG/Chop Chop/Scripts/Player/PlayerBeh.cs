using System.Collections.Generic;
using System.Linq;
using RDG.Chop_Chop.Scripts.Movement;
using UnityEngine;
using RDG.UnityInput;
namespace RDG.Chop_Chop.Scripts.Player {
  public class PlayerBeh : MonoBehaviour, MotorDirector {

    [SerializeField] private KeyActionsSo keyActions;
    [SerializeField] private MovementSo movement;
    [SerializeField] private MotorConfig motorConfig;

    private Vector2 currentDirectionX;
    private Vector2 currentDirectionY;
    private KeyActionStack xStack;
    private KeyActionStack yStack;
    private KeyActionFilter jumpFilter;
    private Motor motor;
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

    public void Start() {
      motor = movement.NewMotor(motorConfig, GetComponentInChildren<Rigidbody>(), this);
      
      jumpFilter = new KeyActionFilter(keyActions, new []{ KeyAction.Translate });
      jumpFilter.OnDown += (action) => isJumping = true;
      jumpFilter.OnUp += (action) => isJumping = false;
      
      xStack = new KeyActionStack(keyActions, XAxisMap.Keys.ToArray());
      xStack.OnStackChange += HandleXMovementChange;
      
      yStack = new KeyActionStack(keyActions, YAxisMap.Keys.ToArray());
      yStack.OnStackChange += HandleYMovementChange;
    }

    public void OnDestroy() {
      xStack?.Release();
      yStack?.Release();
      jumpFilter?.Release();
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
      return (currentDirectionX + currentDirectionY).normalized;
    }

    public bool GetJumpRequested() {
      return isJumping;
    }

    public void FixedUpdate() {
      motor?.FixedUpdate();
    }
  }
}
