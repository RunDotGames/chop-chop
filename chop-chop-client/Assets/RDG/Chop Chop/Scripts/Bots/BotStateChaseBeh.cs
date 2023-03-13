using System;
using System.Collections.Generic;
using System.Linq;
using RDG.Chop_Chop.Scripts.Faction;
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

    private Vector3 chaseTarget;
    private bool hasChaseTarget;
    private NavMeshFollower follower;

    public FiniteStateKeySo UpdateState(FiniteStateKeySo priorActive) {
      if (priorActive != myStateKey) {
        chaseTarget = Vector3.zero;
        hasChaseTarget = false;
        events.onEnter.Invoke();
        follower = new NavMeshFollower(transform, .1f);
      }
      
      if (attackTargets.Items.Any()) {
        return Exit(attackStateKey);
      }

      var foundTarget = GetNearestTarget(transform.position, chaseTargets.Items, out var nearest);
      if (!foundTarget && !hasChaseTarget) {
        return Exit(idleStateKey);
      }
      
      // If I was not pathing or the path target has sufficiently changed
      if (!follower.HasPath || (chaseTarget - nearest).magnitude > .33f) {
        chaseTarget = follower.NavTo(nearest) ? nearest : chaseTarget;
        hasChaseTarget = hasChaseTarget || chaseTarget == nearest;
      }
      var dir = follower.Update();
      if (dir == Vector2.zero) {
        return Exit(idleStateKey);
      }
      
      events.onMoveRequested.Invoke(dir);
      return myStateKey;
    }

    private static bool GetNearestTarget(Vector3 position, IEnumerable<GameObject> targets, out Vector3 nearest) {
      var nearestDistance = float.MaxValue;
      var nearestPoint = Vector3.zero;
      var found = false;
      foreach (var aTarget in targets) {
        aTarget.GetComponentInChildren<FactionedBeh>();
        var dist = (position - aTarget.transform.position).magnitude;
        if (dist > nearestDistance) {
          continue;
        }
          
        found = true;
        nearestDistance = dist;
        nearestPoint = aTarget.transform.position;
      }
      nearest = nearestPoint;
      return found;
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
