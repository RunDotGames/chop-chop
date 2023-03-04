using System.Linq;
using RDG.Chop_Chop.Scripts.Sense;
using RDG.UnityFSM;
using UnityEngine;
namespace RDG.Chop_Chop.Scripts.Bots {

 
  public class BotStateIdleBeh: MonoBehaviour, FiniteState {

    [SerializeField] private FiniteStateKeySo myStateKey;
    [SerializeField] private FiniteStateKeySo chaseStateKey;
    
    [SerializeField] private GameObject chaseVisionRoot;
    
    private Vision chaseVision;
    public FiniteStateKeySo Key => myStateKey;

    public void Awake() {
      chaseVision = chaseVisionRoot.GetComponentInChildren<Vision>();
    }
    public FiniteStateKeySo UpdateState(FiniteStateKeySo priorActive) {
      return chaseVision.Visible.Any() ? chaseStateKey : myStateKey;
    }
  
  }
}
