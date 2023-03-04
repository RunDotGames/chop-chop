using System;
using System.Linq;
using RDG.Chop_Chop.Scripts.Combat;
using RDG.Chop_Chop.Scripts.Sense;
using RDG.Chop_Chop.Scripts.Util;
using RDG.UnityFSM;
using UnityEngine;
using UnityEngine.Events;

namespace RDG.Chop_Chop.Scripts.Bots {

  [Serializable]
  public class BotStateAttackEvents {
    public UnityEvent onAttack;
  }
  public class BotStateAttackBeh : MonoBehaviour, FiniteState {

    [SerializeField] private BotStateAttackEvents events;
    
    [SerializeField] private FiniteStateKeySo myKey;
    [SerializeField] private FiniteStateKeySo idleKey;

    private readonly EventedCollection<GameObject> attackTargets = new();
    public FiniteStateKeySo Key => myKey;

    public FiniteStateKeySo UpdateState(FiniteStateKeySo priorActive) {
      if (!attackTargets.Items.Any()) {
        return idleKey;
      }
      
      events.onAttack.Invoke();
      return myKey;
    }

    public void ModifyAttackTargets(GameObject item, bool isAdded) {
      attackTargets.OnChange(item, isAdded);
    }
  }
}
