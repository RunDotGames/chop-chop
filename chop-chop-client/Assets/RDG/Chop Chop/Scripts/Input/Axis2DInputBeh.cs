using RDG.Chop_Chop.Scripts.Camera;
using UnityEngine;
using UnityEngine.Events;

namespace RDG.Chop_Chop.Scripts.Input {

  // Converts 2 axis inputs into a Vector2 direction (oriented away from the camera)
  
  public class Axis2DInputBeh : MonoBehaviour {
    
    [SerializeField] private CameraSo cameras;
    [SerializeField] private UnityEvent<Vector2> onChange;

    private Vector2 rawValue = Vector2.zero;
    private Vector3 oldForward;
    public void OnDestroy() {
      onChange?.RemoveAllListeners();
    }

    public void Update() {
      var forward = cameras.GameCamera.forward;
      if (forward != oldForward) {
        onChange?.Invoke(Value);
      }
      oldForward = forward; 
    }

    public Vector2 Value {
      get {
        if (!isActiveAndEnabled) {
          return Vector2.zero;
        }

        var inCamSpace = cameras.GameCamera.TransformVector(new Vector3(rawValue.x, 0, rawValue.y));
        return new Vector2(inCamSpace.x, inCamSpace.z).normalized;
      }
    }

    public void SetX(float value) {
      rawValue.x = Mathf.Max(-1, Mathf.Min(value, 1));
      onChange?.Invoke(Value);
    }

    public void SetY(float value) {
      rawValue.y = Mathf.Max(-1, Mathf.Min(value, 1));
      onChange?.Invoke(Value);
    }
  }
}
