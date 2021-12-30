using System;
using UnityEngine;

namespace RDG.Chop_Chop.Scripts.Movement {

  public class Ground {
    public Guid Id;
    public Collider Collider;
  }
  public class GroundBeh : MonoBehaviour {

    public MovementSo movement;

    private readonly Ground ground = new Ground();
    
    public void Start() {
      ground.Id = Guid.NewGuid();
      ground.Collider = GetComponentInChildren<Collider>();
      ground.Collider.name = "ground: " + ground.Id;
      var body = GetComponentInChildren<Rigidbody>();
      if (body != null) {
        body.isKinematic = true;
      } 
      movement.AddGround(ground);
    }

    public void OnDestroy() {
      movement.RemoveGround(ground);
    }

  }
}
