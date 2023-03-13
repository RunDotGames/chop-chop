using System;
using RDG.Chop_Chop.Scripts.Util;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RDG.Chop_Chop.Scripts.Faction {
  public class NotMyFactionFilterBeh : MonoBehaviour, GameObjectFilter {

    public GameObject root;

    private FactionedBeh myFaction;
    public void Awake() {
      root = root == null ? gameObject : root;
      myFaction = root.GetComponentInChildren<FactionedBeh>();
      if (myFaction == null) {
        Debug.LogError("Not My Faction Filter could not locate my faction");
      }
    }
    
    public bool Check(GameObject go) {
      var theirFaction = go.GetComponentInChildren<FactionedBeh>();
      if (myFaction == null || theirFaction == null) {
        return false;
      }

      return myFaction.Faction != theirFaction.Faction;
    }
  }
}
