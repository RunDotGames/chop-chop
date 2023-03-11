using System;
using System.Collections.Generic;
using System.Linq;
using RDG.Chop_Chop.Scripts.Util;
using UnityEngine;
using UnityEngine.Events;

namespace RDG.Chop_Chop.Scripts.Sense {

 

  [Serializable]
  public class VisionBubbleConfig {
    public float distance;
    public float height;
  }
  
  public class VisionBubbleBeh : MonoBehaviour, Vision {

    [SerializeField] private SenseSo sense;
    [SerializeField] private VisionBubbleConfig config;
    [SerializeField] private VisionEvents events;

    private HashSet<GameObject> visionCache = new();
    
    public float ViewFactor { get; set; }


    public void Awake() {
      ViewFactor = 1.0f;
    }
    public void OnDisable() {
      foreach (var visible in visionCache) {
        events.onVisibilityChanged.Invoke(visible, false);
      }
      visionCache.Clear();
    }

    public void OnDestroy() {
      events.Release();
    }
    public void FixedUpdate() {
      var hitCount = Physics.OverlapSphereNonAlloc(
        transform.position,
        config.distance * ViewFactor,
        CacheUtil.Colliders,
        sense.VisionLayer
      );
      HashSet<GameObject> visibleNow = new();
      for (var index = 0; index < hitCount; index++) {
        var visible = sense.GetVisible(CacheUtil.Colliders[index]);
        if (visible == null) {
          continue;
        }
        var heightDiff = visible.VisibleCollider.transform.position.y - transform.position.y;
        if ( Math.Abs(heightDiff) > config.height){
          continue;
        }
        
        visibleNow.Add(visible.VisibleCollider.gameObject);
        if (!visionCache.Contains(visible.VisibleCollider.gameObject)) {
          events.onVisibilityChanged.Invoke(visible.VisibleCollider.gameObject, true);
          continue;
        }
        
        visionCache.Remove(visible.VisibleCollider.gameObject);
      }

      foreach (var visible in visionCache) {
        events.onVisibilityChanged.Invoke(visible, false);
      }
      visionCache = visibleNow;
    }
    public IEnumerable<GameObject> Visible => visionCache;

  }
}
