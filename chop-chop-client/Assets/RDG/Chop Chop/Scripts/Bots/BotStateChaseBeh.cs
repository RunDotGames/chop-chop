using System;
using System.IO;
using System.Linq;
using RDG.Chop_Chop.Scripts.Sense;
using RDG.Chop_Chop.Scripts.Util;
using RDG.UnityFSM;
using UnityEngine;
using UnityEngine.AI;
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
    private NavMeshPath chasePath;
    private int pathPoint;

    public FiniteStateKeySo UpdateState(FiniteStateKeySo priorActive) {
      if (!chaseTargets.Items.Any()) {
        chasePath = null;
        return Exit(idleStateKey);
      }
      
      if (attackTargets.Items.Any()) {
        chasePath = null;
        return Exit(attackStateKey);
      }

      if (priorActive != myStateKey) {
        events.onEnter.Invoke();
      }

      var targetPosition = chaseTargets.Items.First().transform.position;
      if (chasePath == null || (targetPosition - chaseTarget).magnitude > .333f) {
        chaseTarget = targetPosition;
        chasePath = new NavMeshPath();
        NavMesh.CalculatePath(transform.position, targetPosition, NavMesh.AllAreas, chasePath);
        pathPoint = 0;
      }
      if (chasePath.status != NavMeshPathStatus.PathComplete) {
        Debug.LogError(chasePath.status);
        return myStateKey;
      }
      
      while (CheckPathPoint(pathPoint)) {
        pathPoint++;
      }
      
      if (pathPoint >= chasePath.corners.Length) {
        Debug.LogError("oob");
        return myStateKey;
      }
      
      for (var i = 0; i < chasePath.corners.Length - 1; i++)
        
        Debug.DrawLine(chasePath.corners[i], chasePath.corners[i + 1], Color.blue);

      var delta = chasePath.corners[pathPoint] - transform.position;
      Debug.Log(delta);
      events.onMoveRequested.Invoke(new Vector2(delta.x, delta.z).normalized);
      return myStateKey;
    }

    private bool CheckPathPoint(int position) {
      if (pathPoint >= chasePath.corners.Length) {
        return false;
      }
      var delta = transform.position - chasePath.corners[pathPoint];
      delta.y = 0;
      return delta.magnitude < .1f;
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
