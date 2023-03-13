using System;
using System.Linq;
using RDG.Chop_Chop.Scripts.Util;
using RDG.UnityFSM;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace RDG.Chop_Chop.Scripts.Bots {
  
  [Serializable]
  public class BotStateWonderEvents {
    public UnityEvent<Vector2> onMoveRequested;
    public UnityEvent onEnter;
    public UnityEvent onExit;
  }
  public class BotStateWonderBeh : MonoBehaviour, FiniteState {

    [SerializeField] private FiniteStateKeySo myStateKey;
    [SerializeField] private FiniteStateKeySo chaseStateKey;
    [SerializeField] private FiniteStateKeySo returnStateKey;
    [SerializeField] private BotStateWonderEvents events;
    
    [SerializeField] private float watchDuration;
    [SerializeField] private float watchMoveDuration;
    [SerializeField] private int watchCount;
    
    public FiniteStateKeySo Key => myStateKey;
    
    private readonly EventedCollection<GameObject> chaseTargets = new();
    private float startTime;
    private Vector2 moveDir;
    private int count;
    private bool isWatching;


    private FiniteStateKeySo HandleWatch() {
      if (!isWatching) {
        return null;
      }
      
      if ( (Time.time - startTime) <= watchDuration) {
        return myStateKey;
      }
          
      isWatching = false;
      count++;
      if (count > watchCount ) {
        return Exit(returnStateKey);
      }

      moveDir = Random.insideUnitCircle;
      startTime = Time.time;
      return myStateKey;
    }

    private FiniteStateKeySo HandleMove() {
      if (isWatching) {
        return null;
      }
      
      if ( (Time.time - startTime) <= watchMoveDuration) {
        events.onMoveRequested?.Invoke(moveDir);
        return myStateKey;
      }
      
      events.onMoveRequested?.Invoke(Vector2.zero);
      startTime = Time.time;
      isWatching = true;
      return myStateKey;
    }

    
    public FiniteStateKeySo UpdateState(FiniteStateKeySo priorActive) {
      if (priorActive != myStateKey) {
        isWatching = true;
        startTime = Time.time;
        count = 0;
        events.onEnter?.Invoke();
      }
      
      Debug.Log(Time.time - startTime);
      
      if (chaseTargets.Items.Any()) {
        return Exit(chaseStateKey);
      }

      var watchResult = HandleWatch();
      if (watchResult != null) {
        return watchResult;
      }

      var moveResult = HandleMove();
      return moveResult == null ? Exit(returnStateKey) : moveResult;
    }
    
    public void ModifyChaseTargets(GameObject target, bool isAdded) {
      chaseTargets.OnChange(target, isAdded);
    }
    
    private FiniteStateKeySo Exit(FiniteStateKeySo exitVal) {
      events.onMoveRequested.Invoke(Vector2.zero);
      events.onExit.Invoke();
      return exitVal;
    }
  }
}
