using System;
using System.Linq;
using RDG.Chop_Chop.Scripts.Sense;
using RDG.Chop_Chop.Scripts.Util;
using RDG.UnityFSM;
using UnityEngine;
using UnityEngine.Events;

namespace RDG.Chop_Chop.Scripts.Bots {

  [Serializable]
  public class BotStateChaseEvents {
    public UnityEvent<Vector2> onMoveRequested;
    public UnityEvent onEnter;
    public UnityEvent onExit;
  }
  public class BotStateChaseBeh: MonoBehaviour, FiniteState {

    [SerializeField] private BotStateChaseEvents events;
    
    [SerializeField] private FiniteStateKeySo myStateKey;
    [SerializeField] private FiniteStateKeySo idleStateKey;
    [SerializeField] private FiniteStateKeySo attackStateKey;
    
    private readonly EventedCollection<GameObject> chaseTargets = new();
    private readonly EventedCollection<GameObject> attackTargets = new();
    public FiniteStateKeySo Key => myStateKey;

    public FiniteStateKeySo UpdateState(FiniteStateKeySo priorActive) {
      if (!chaseTargets.Items.Any()) {
        return Exit(idleStateKey);
      }
      
      if (attackTargets.Items.Any()) {
        return Exit(attackStateKey);
      }

      if (priorActive != myStateKey) {
        events.onEnter.Invoke();
      }
      var delta = chaseTargets.Items.First().transform.position - transform.position;
      events.onMoveRequested.Invoke(new Vector2(delta.x, delta.z).normalized);
      return myStateKey;
    }

    private FiniteStateKeySo Exit(FiniteStateKeySo exitVal) {
      events.onMoveRequested.Invoke(Vector2.zero);
      events.onExit.Invoke();
      return exitVal;
    }
    
    public void ModifyAttackTargets(GameObject item, bool isAdded) {
      attackTargets.OnChange(item, isAdded);
    }
    
    public void ModifyChaseTargets(GameObject item, bool isAdded) {
      chaseTargets.OnChange(item, isAdded);
    }
  
  }
}
