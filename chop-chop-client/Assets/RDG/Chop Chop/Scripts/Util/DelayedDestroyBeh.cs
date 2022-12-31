using System.Threading.Tasks;
using RDG.Chop_Chop.Scripts.Combat;
using RDG.UnityUtil;
using UnityEngine;

namespace RDG.Chop_Chop.Scripts.Util {
  
  // Destroy a GameObject after a delay
  public class DelayedDestroyBeh : MonoBehaviour {

    [SerializeField] private float delaySeconds = 10;
    
    public void DestroyDelayed() {
      TaskUtils.WaitCoroutine(this, delaySeconds).ContinueWith((_) => {
        Destroy(gameObject);
      }, TaskContinuationOptions.ExecuteSynchronously);
    }
  }
}
