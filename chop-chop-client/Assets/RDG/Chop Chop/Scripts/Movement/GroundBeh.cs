using System;
using UnityEngine;

namespace RDG.Chop_Chop.Scripts.Movement {

  public interface Ground {
    public Guid Id { get; }
    public Collider Collider { get; }
  }
  public class GroundBeh : MonoBehaviour, Ground {

    public MovementSo movement;

    
    public void Awake() {
      Id = Guid.NewGuid();
      Collider = GetComponentInChildren<Collider>();
      Collider.name = "ground: " + Id;
      var body = GetComponentInChildren<Rigidbody>();
      
      if (body != null) {
        body.isKinematic = true;
      } 
      movement.AddGround(this);
    }

    public void OnDestroy() {
      movement.RemoveGround(this);
    }

    public Guid Id { get; private set; }
    public Collider Collider { get; private set; }
  }
}
