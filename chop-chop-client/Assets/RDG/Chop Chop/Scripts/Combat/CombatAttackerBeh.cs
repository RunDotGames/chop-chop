using System;
using System.Threading.Tasks;
using RDG.Chop_Chop.Scripts.Faction;
using UnityEngine;

namespace RDG.Chop_Chop.Scripts.Combat {
  public class CombatAttackerBeh : MonoBehaviour {


    private readonly Lazy<CombatAttacker> combatAttacker;

    [SerializeField] private CombatSo combat;
    [SerializeField] private FactionSo faction;
    
    [SerializeField] private CombatAttackConfig config;
    [SerializeField] private CombatAttackEvents events;
    [SerializeField] private Transform attackRoot;
    
    public CombatAttacker CombatAttacker => combatAttacker.Value;

    public CombatAttackerBeh() {
      combatAttacker = new Lazy<CombatAttacker>(() => {
        if (combat == null) {
          throw new Exception("attacker missing combat so");
        }
        var combatParams = new CombatAttackerParameters(config, events, attackRoot, this, faction);
        return combat.NewAttacker(combatParams);
      });
    }

    public void Update() {
      CombatAttacker.Update();
    }

    public void OnDisable() {
      CombatAttacker.Disable();
    }

    public void OnEnable() {
      CombatAttacker.Enable();
    }

    public void OnDestroy() {
      CombatAttacker.Release();
    }

    public void Attack() {
      CombatAttacker.Attack().ContinueWith(result => {
        // handle result if needed
      }, TaskContinuationOptions.ExecuteSynchronously);
    }
  }
}
