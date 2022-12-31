using System;
using RDG.UnityInput;
using UnityEngine;

namespace RDG.Chop_Chop.Scripts.Camera {
  public class CameraBeh : MonoBehaviour {
    public CameraSo cameras;
    public KeyActionsRegistrySo keyActions;
    public KeyActionSo swivelRight;
    public KeyActionSo swivelLeft;

    private KeyActionStack swivelStack;

    private float swivelFactor = 0.0f;
    public void Start() {
      swivelStack = new KeyActionStack(keyActions, new[]{
        swivelLeft, swivelRight
      });
      swivelStack.OnStackTopChange += HandleSwivelChange;
    }
        
    private void HandleSwivelChange(KeyActionSo keyAction) {
      if (keyAction == swivelLeft) {
        swivelFactor = 1;
        return;
      }
      if (keyAction == swivelRight) {
        swivelFactor = -1;
        return;
      }

      swivelFactor = 0;
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
