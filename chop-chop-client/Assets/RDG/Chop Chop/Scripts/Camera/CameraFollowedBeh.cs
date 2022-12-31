using System;
using MoreMountains.Tools;
using UnityEngine;

namespace RDG.Chop_Chop.Scripts.Camera {
  public class CameraFollowedBeh : MonoBehaviour, CameraFollowable {
    
    [SerializeField] private CameraSo cameras;
    
    public void OnEnable() {
      cameras.SetFollowed(this);
    }

    public void Start() {
      cameras.SetFollowed(this);
    }

    public void OnDestroy() {
      if ((CameraFollowedBeh)cameras.GetFollowed() == this) {
        cameras.SetFollowed(null);
      }
    }

    public void OnDisable() {
      if ((CameraFollowedBeh)cameras.GetFollowed() == this) {
        cameras.SetFollowed(null);
      }
    }

    public Vector3 GetTrackingPoint() {
      return transform.position;
    }
  }
}
