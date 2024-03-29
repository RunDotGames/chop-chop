﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace RDG.Chop_Chop.Scripts.Movement {

  public interface MovementProvider {
    public Ground GetGround(Collider collider);
    
  }

  [Serializable]
  public class MovementConfig {
    public string groundLayerName;
    public float groundCheckDistance = 0.05f;
    public float checkBackupDistance = 0.1f;
    public bool drawDebug;
    public float fallRate = 5.0f;
    public float fallMaxSpeed = 10;
    public float wallHeightCheck = .01f;
    public AnimationCurve jumpCurve;
  }
  [CreateAssetMenu(menuName = "RDG/ChopChop/Movement")]
  public class MovementSo: ScriptableObject, MovementProvider {

    [SerializeField] private MovementConfig config;

    private readonly Dictionary<string, Ground> nameToGround = new Dictionary<string, Ground>();

    public void AddGround(Ground ground) {
      ground.Collider.gameObject.layer = LayerMask.NameToLayer(config.groundLayerName);
      nameToGround.Add(ground.Collider.name, ground);
    }
    public void RemoveGround(Ground ground) {
      nameToGround.Remove(ground.Collider.name);
    }

    public Ground GetGround(Collider collider) {
      if (collider == null) {
        return null;
      }
      return nameToGround.TryGetValue(collider.name, out var ground) ? ground : null;
    }
    public MovementConfig Config => config;
  }
}
