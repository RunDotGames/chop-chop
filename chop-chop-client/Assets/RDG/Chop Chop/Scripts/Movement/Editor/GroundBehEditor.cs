using UnityEditor;
using UnityEngine;

namespace RDG.Chop_Chop.Scripts.Movement.Editor {
  
  [CustomEditor(typeof(GroundBeh))]
  [CanEditMultipleObjects]
  public class GroundBehEditor : UnityEditor.Editor {

    public void Reset() {
      foreach (var aTarget in targets) {
        var ground = (GroundBeh)aTarget;
        SetAsStaticNavigable(ground.gameObject);
       
      }
    }

    private static void SetAsStaticNavigable(GameObject go) {
      var goTransform = go.transform;
      var childCount = goTransform.childCount;
      for (var i = 0; i < childCount; i++) {
        SetAsStaticNavigable(goTransform.GetChild(i).gameObject);
      }
      var flags = GameObjectUtility.GetStaticEditorFlags(go);
      if ((flags & StaticEditorFlags.NavigationStatic) != 0) {
        return;
      }
        
      go.isStatic = true;
      GameObjectUtility.SetStaticEditorFlags(go, flags | StaticEditorFlags.NavigationStatic);
    }
  }
}
