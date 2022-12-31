using System;
using System.Threading.Tasks;
using RDG.Chop_Chop.Scripts.Combat;
using RDG.UnityUtil;
using UnityEngine;

namespace RDG.Chop_Chop.Scripts.Combat {
  
  
  public class CombatTargetRespawnerBeh: MonoBehaviour {
    [SerializeField] private GameObject toSpawnPrefab;
    [SerializeField] private float respawnDelaySeconds = 15;
    [SerializeField] private bool spawnOnStart = true;

    private CombatTarget target;
    public void Start() {
      if (!spawnOnStart) {
        return;
      }
      
      Spawn();
    }
    private async void HandleDead(CombatTarget _) {
      await TaskUtils.WaitCoroutine(this, respawnDelaySeconds);
      Spawn();
    }

    private void Spawn() {
      var spawned = Instantiate(toSpawnPrefab, transform);
      var myTransform = transform;
      spawned.transform.position = myTransform.position;
      spawned.transform.forward = myTransform.forward;
      var found = spawned.GetComponentInChildren<CombatTargetBeh>();
      if (found == null) {
        throw new Exception("re-spawner cannot locate child combat target");
      }
      found.Events.onDeath.AddListener(HandleDead);
    }
  }
}
