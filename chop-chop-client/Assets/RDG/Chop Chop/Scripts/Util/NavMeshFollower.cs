using UnityEngine;
using UnityEngine.AI;

namespace RDG.Chop_Chop.Scripts.Util {
  
  public class NavMeshFollower {
    
    private readonly Transform myTransform;
    private readonly float arrivalDistance;

    private int pathPoint = int.MaxValue;
    private NavMeshPath path;
    
    public NavMeshFollower(Transform myTransform, float arrivalDistance) {
      this.myTransform = myTransform;
      this.arrivalDistance = arrivalDistance;
    }

    public Vector2 Update() {
      if (!HasPath) {
        return Vector2.zero;
      }
      
      while (CheckPathPoint(pathPoint)) {
        pathPoint++;
      }
      
      if (pathPoint >= path.corners.Length) {
        return Vector2.zero;
      }
      
      for (var i = 0; i < path.corners.Length - 1; i++)
        Debug.DrawLine(path.corners[i], path.corners[i + 1], Color.blue);

      var delta = path.corners[pathPoint] - myTransform.position;
      return new Vector2(delta.x, delta.z).normalized;
    }
    
    
    // Returns true if re-navigation was successful, false otherwise (maintains old path on fail)
    public bool NavTo(Vector3 position) {
      var newPath = new NavMeshPath();
      NavMesh.CalculatePath(myTransform.position, position, NavMesh.AllAreas, newPath);
      if (newPath.status != NavMeshPathStatus.PathComplete) {
        return false;
      }

      pathPoint = 0;
      path = newPath;
      return true;
    }

    public bool HasPath => path is{ status: NavMeshPathStatus.PathComplete };
    
    private bool CheckPathPoint(int position) {
      if ( !HasPath || pathPoint >= path.corners.Length) {
        return false;
      }
      
      var delta = myTransform.position - path.corners[pathPoint];
      delta.y = 0;
      return delta.magnitude < arrivalDistance;
    }
  }
}
