using System;
using UnityEngine;

namespace RDG.Chop_Chop.Scripts.Sense {
  public class SenseEventProducerBeh : MonoBehaviour {

    [SerializeField] private SenseSo sense;
    public void OnEnable() {
      sense.Enable();
    }

    public void OnDisable() {
      sense.Disable();
    }
  }
}
