using System.Linq;
using RDG.Chop_Chop.Scripts.Util;
using RDG.UnityFSM;
using UnityEngine;
namespace RDG.Chop_Chop.Scripts.Bots {

 
  public class BotStateIdleBeh: MonoBehaviour, FiniteState {

    [SerializeField] private FiniteStateKeySo myStateKey;
    [SerializeField] private FiniteStateKeySo chaseStateKey;
    
    private readonly EventedCollection<GameObject> visibleTargets = new();
    
    public FiniteStateKeySo Key => myStateKey;

    public FiniteStateKeySo UpdateState(FiniteStateKeySo priorActive) {
      return visibleTargets.Items.Any() ? chaseStateKey : myStateKey;
    }
    
    public void ModifyChaseTargets(GameObject target, bool isAdded) {
      visibleTargets.OnChange(target, isAdded);
    }
  
  }
}
