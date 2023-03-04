using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace RDG.Chop_Chop.Scripts.Sense {
  [Serializable]
  public class VisionEvents {
    public UnityEvent<GameObject, bool> onVisibilityChanged;

    public void Release() {
      onVisibilityChanged.RemoveAllListeners();
    }
  }

  public interface Vision {
    public IEnumerable<GameObject> Visible { get; }
  }

}
