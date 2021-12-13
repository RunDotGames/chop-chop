using System;
using UnityEngine;

namespace RDG.Chop_Chop.Scripts.Camera {
  public class CameraEventProducerBeh : MonoBehaviour {

    public CameraSo cameraSo;
    
    public void Update() {
      cameraSo.Update();
    }

    public void OnDestroy() {
      cameraSo.Clear();
    }
  }
}
