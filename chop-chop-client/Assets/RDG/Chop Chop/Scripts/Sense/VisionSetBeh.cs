using System.Collections.Generic;
using UnityEngine;

namespace RDG.Chop_Chop.Scripts.Sense {
  public class VisionSetBeh : MonoBehaviour, Vision {

    private readonly HashSet<GameObject> visible = new();
    public IEnumerable<GameObject> Visible => visible;

    public void Modify(GameObject item, bool isAdded) {
      switch (isAdded) {
        case true when !visible.Contains(item):
          visible.Add(item);
          break;
        case false when visible.Contains(item):
          visible.Remove(item);
          break;
      }
    }
    
  }
}
