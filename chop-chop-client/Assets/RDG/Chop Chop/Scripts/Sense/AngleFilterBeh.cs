using System;
using RDG.Chop_Chop.Scripts.Util;
using UnityEngine;

namespace RDG.Chop_Chop.Scripts.Sense {
  public class AngleFilterBeh : MonoBehaviour, GameObjectFilter {

    public float maxAngle;
    public bool Check(GameObject go) {
      var myTransform = transform;
      var dir = go.transform.position - myTransform.position;
      dir.y = 0;
      var angle = Vector3.Angle(
        myTransform.forward,
        Vector3.Normalize(dir)
      );
      return Math.Abs(angle) < maxAngle;
    }
  }
}
