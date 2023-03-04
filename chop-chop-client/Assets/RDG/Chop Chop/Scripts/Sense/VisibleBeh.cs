using System;
using UnityEngine;

namespace RDG.Chop_Chop.Scripts.Sense {
  public class VisibleBeh : MonoBehaviour, Visible {
    
    [SerializeField] private SenseSo sense;
    [SerializeField] private Transform root;
    
    public Transform Root => root;
    public Collider VisibleCollider { get; private set; }

    private Guid id;
    public void Awake() {
      id = Guid.NewGuid();
      VisibleCollider = GetComponentInChildren<Collider>();
      VisibleCollider.name = "visible: " + id;
      sense.AddVisible(this);
    }

    public void OnDestroy() {
      sense.RemoveVisible(this);
    }
  }
}
