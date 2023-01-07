using System;
using UnityEngine;

namespace RDG.Chop_Chop.Scripts.Combat {
  
  public class CombatEventProducerBeh : MonoBehaviour{
    [SerializeField] private CombatSo combat;

    public void OnEnable() {
      combat.Enable();
    }

    public void OnDisable() {
      combat.Disable();
    }
  }
}
