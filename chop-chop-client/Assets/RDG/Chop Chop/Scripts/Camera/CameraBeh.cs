using UnityEngine;

namespace RDG.Chop_Chop.Scripts.Camera {
  public class CameraBeh : MonoBehaviour {
    [SerializeField] private CameraSo cameras;
    
    private float swivelFactor;
    
    public void SetSwivelFactor(float factor) {
      swivelFactor = factor;
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
  }
}
