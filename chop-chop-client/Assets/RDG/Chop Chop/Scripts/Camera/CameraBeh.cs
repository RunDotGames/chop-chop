using System;
using RDG.UnityInput;
using UnityEngine;

namespace RDG.Chop_Chop.Scripts.Camera {
  public class CameraBeh : MonoBehaviour {
    public CameraSo cameras;
    public KeyActionsSo keyActions;

    private KeyActionStack swivelStack;

    private float swivelFactor = 0.0f;
    public void Start() {
      swivelStack = new KeyActionStack(keyActions, new[]{
        KeyAction.MoveLeftSecondary, KeyAction.MoveRightSecondary
      });
      swivelStack.OnStackTopChange += HandleSwivelChange;
    }
    
    private void HandleSwivelChange(KeyAction keyAction) {
      switch (keyAction) {
        case KeyAction.MoveLeftSecondary:
          swivelFactor = -1;
          return;
        case KeyAction.MoveRightSecondary:
          swivelFactor = 1;
          return;
        default:
          swivelFactor = 0;
          return;
      }
    }

    public void Update() {
      if (swivelFactor != 0) {
        cameras.Swivel(swivelFactor * Time.deltaTime);  
      }
      
    }

    public void OnEnable() {
      cameras.GameCamera = transform;
    }

    public void OnDisable() {
      if (cameras.GameCamera == transform) {
        cameras.GameCamera = null;
      }
    }

    public void OnDestroy() {
      swivelStack.Release();
    }
  }
}
