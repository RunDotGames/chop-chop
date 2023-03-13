using RDG.Chop_Chop.Scripts.Util;
using UnityEngine;

namespace RDG.Chop_Chop.Scripts.Movement {
  public class GroundIntersectFilterBeh : MonoBehaviour, GameObjectFilter {

    [SerializeField] private MovementSo movement;
    [SerializeField] private float height;
    private int groundLayerMask;

    public void Awake() {
      groundLayerMask = LayerMask.GetMask(movement.Config.groundLayerName);
    }
    public bool Check(GameObject go) {
      var start = transform.position + (Vector3.up * height);
      var end = go.transform.position + (Vector3.up * height);
      Debug.DrawLine(start, end, Color.magenta);
      return !Physics.Linecast(start, end, groundLayerMask);
    }
  }
}
