using System;
using UnityEngine;

namespace RDG.Chop_Chop.Scripts.Camera {

  [Serializable]
  public class CameraConfig {
    public Vector2 followRotation = Vector2.up + Vector2.right;
    public float followDistance = 20;
    public float followFactor = 1;
    public float swivelSpeed = 10;
  }
  [CreateAssetMenu(menuName = "RDG/ChopChop/Cameras")]
  public class CameraSo : ScriptableObject {

    public CameraConfig config;
    public Transform GameCamera { get; set; }
    private Transform followed;
    private float swivel;

    
    public void SetFollowed(Transform aFollowed) {
      followed = aFollowed;
    }

    public void Update() {
      if (GameCamera == null || followed == null) {
        return;
      }

      var followedPosition = followed.position;
      var gameCameraToPos = followedPosition + (Quaternion.Euler(config.followRotation + (Vector2.up * swivel)) * Vector3.forward) * config.followDistance;
      var gameCameraCurrentPos = GameCamera.position;
      var forward = followedPosition - gameCameraCurrentPos;
      
      GameCamera.position = Vector3.Lerp(gameCameraCurrentPos, gameCameraToPos, Time.deltaTime * config.followFactor);
      GameCamera.rotation = Quaternion.Lerp(GameCamera.rotation, Quaternion.LookRotation(forward, Vector3.up), Time.deltaTime * config.followFactor);
    }

    public void Clear() {
      GameCamera = null;
      followed = null;
      swivel = 0;
    }
    public void Swivel(float swivelFactor) {
      swivel += swivelFactor * config.swivelSpeed;
    }
  }
}
