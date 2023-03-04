using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace RDG.Chop_Chop.Scripts.Movement {
  
  [Serializable]
  public class MotorConfig {
    public float moveSpeed = 10.0f;
    public float jumpSpeed = 10.0f;
    public float rotationSpeed = 10.0f;
    public float radius = 1.0f;
    public float height = 1.0f;
  }

  [Serializable]
  public class MotorEvents {
    public UnityEvent onGrounded;
    public UnityEvent onJump;
    public void Release() {
      onGrounded.RemoveAllListeners();
      onJump.RemoveAllListeners();
    }
  }

  public class MotorBeh : MonoBehaviour {
    enum JumpState {
      Able, InProgress, Exhausted
    }
    
    class MovementInfo {
      public Vector3 Position { get; set; }
      public Quaternion Rotation { get; set; }
      public Vector3 ForwardNormal { get; set; }
      public float RemainingDistance { get; set; }
      public bool IsJumping { get; set; }
      public bool IsGrounded { get; set; }
    }
    
    private const int CACHE_SIZE = 100;
    // ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
    private const RigidbodyConstraints FREEZE_ROTATION = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    private static readonly RaycastHit[] HitCache = new RaycastHit[CACHE_SIZE];
    private static readonly Vector3[] DowncastOffsets ={
      Vector3.left, Vector3.zero, Vector3.right, Vector3.forward, Vector3.back
    };
    
    [SerializeField] private MovementSo movement;
    [SerializeField] private MotorConfig config;
    [SerializeField] private MotorEvents events;
    [SerializeField] private Rigidbody body;
    
    private float fallVelocity;
    private float jumpDuration;
    private JumpState jumpState;
    private bool isGrounded;
    private bool isJumpRequested;
    private Vector2 moveRequested;
    private GroundBeh[] myGround = {};
    private LayerMask groundLayerMask;
    private bool isRequestLocked;
    public void Start() {
      body.constraints = FREEZE_ROTATION;
      myGround = body.GetComponentsInChildren<GroundBeh>();
      groundLayerMask = LayerMask.GetMask(movement.Config.groundLayerName);
    }

    public void RequestDriveDirection(Vector2 dir) {
      moveRequested = dir;
    }

    public void RequestJumpFlag(bool jumpFlag) {
      isJumpRequested = jumpFlag;
    }

    public void LockRequests() {
      isRequestLocked = true;
    }

    public void UnlockRequests() {
      isRequestLocked = false;
    }

    public bool IsRequestLocked => isRequestLocked;

    public Vector2 LastMoveDirection { get; private set; }

    public void FixedUpdate() {
      var bodyTransform = body.transform;
      var directorDir = moveRequested;
      if (isRequestLocked) {
        directorDir = Vector2.zero;
      }
      var info = new MovementInfo{
        Position = bodyTransform.position,
        Rotation = bodyTransform.rotation,
        ForwardNormal = new Vector3(directorDir.x, 0, directorDir.y).normalized,
        IsJumping = false,
        RemainingDistance = config.moveSpeed * Time.fixedDeltaTime
      };
      
      UpdateStateForJumpUp(info);
      UpdateStateForForwardRotation(info);
      UpdateStateForGroundAngle(info);
      UpdateStateForForwardMove(info);
      UpdateStateForForwardMove(info);
      UpdateStateForFalling(info);
      UpdateStateForGroundLanding(info);

      body.MovePosition(info.Position);
      body.MoveRotation(info.Rotation);

      UpdateStateForGroundLanding(info);
      LastMoveDirection = directorDir;
    }
    
    // Handle landing back on the ground
    private void UpdateStateForGroundLanding(MovementInfo info) {
      var wasGrounded = isGrounded;
      isGrounded = info.IsGrounded;
      if (isGrounded && !wasGrounded) {
        events.onGrounded.Invoke();
      }
      if (isGrounded && !isJumpRequested) {
        jumpState = JumpState.Able;
      }
    }

    // Handle movement due to jump upward cycle
    private void UpdateStateForJumpUp(MovementInfo state) {
      if (isJumpRequested && jumpState == JumpState.Able && !isRequestLocked) {
        jumpState = JumpState.InProgress;
        events.onJump.Invoke();
      }

      if (jumpState != JumpState.InProgress) {
        return;
      }
      
      if (!isJumpRequested || isRequestLocked) {
        jumpState = JumpState.Exhausted;
        return;
      }
      
      var maxJump = movement.Config.jumpCurve.keys.Last().time;
      if (jumpDuration > maxJump) {
        jumpState = JumpState.Exhausted;
        return;
      }
      
      state.IsJumping = true;
      jumpDuration += Time.fixedDeltaTime;
      var jumpSpeed = config.jumpSpeed * movement.Config.jumpCurve.Evaluate(jumpDuration) * Time.deltaTime;
      var heightOffset = Vector3.up * config.height;
      var ground = UpCast(state.Position + heightOffset, jumpSpeed, out var hit);
      if (ground == null) {
        state.Position += Vector3.up * jumpSpeed;
        return;
      }

      state.Position = hit.point - heightOffset;
      jumpState = JumpState.Exhausted;
    }

    // Update the forward direction for tilted ground when grounded
    private void UpdateStateForGroundAngle(MovementInfo state) {
      var ground = DownCast(state.Position, config.radius, movement.Config.groundCheckDistance, out var hit, out _);
      if (ground == null) {
        return;
      }

      if (state.ForwardNormal.sqrMagnitude == 0) {
        return;
      }
      
      var cross = Vector3.Cross(hit.normal, state.ForwardNormal);
      var groundTangentDir = Vector3.Cross(cross, hit.normal);
      state.ForwardNormal = groundTangentDir.normalized;
    }
    
    // Move along the forward direction, tangent to any walls encountered
    private void UpdateStateForForwardMove(MovementInfo state) {
      if (state.ForwardNormal.sqrMagnitude == 0 || state.RemainingDistance <= 0.0f) {
        return;
      }

      var distance = state.RemainingDistance + config.radius;
      var position = state.Position + Vector3.up * (movement.Config.wallHeightCheck);
      var wall = WidthCast(position, state.ForwardNormal, config.radius, distance, out var hit);
      if (wall == null) {
        state.Position += state.ForwardNormal * state.RemainingDistance;
        state.RemainingDistance = 0;
        return;
      }

      // Move up to or out out of wall
      var hitDistance = hit.distance;
      state.Position += state.ForwardNormal * (hitDistance - config.radius);
      state.RemainingDistance -= (hitDistance - config.radius);
      
      // Forward is now tangent to wall
      var cross = Vector3.Cross(hit.normal, state.ForwardNormal);
      state.ForwardNormal = Vector3.Cross(cross, hit.normal);
    }

    // Rotate to face the forward direction when one is present
    private void UpdateStateForForwardRotation(MovementInfo state) {
      if (state.ForwardNormal.sqrMagnitude == 0) {
        return;
      }

      state.Rotation = Quaternion.RotateTowards(
        state.Rotation,
        Quaternion.LookRotation(state.ForwardNormal, Vector3.up),
        Time.fixedDeltaTime * config.rotationSpeed
      );
    }

    // Move downward when not jumping and in the air
    private void UpdateStateForFalling(MovementInfo state) {
      if (state.IsJumping) {
        return;
      }
      
      fallVelocity += (movement.Config.fallRate * Time.fixedDeltaTime);
      fallVelocity = Mathf.Min(fallVelocity, movement.Config.fallMaxSpeed);
      var ground = DownCast(state.Position, config.radius, fallVelocity, out var hit, out var offset);
      if (ground == null) {
        state.Position += Vector3.down * fallVelocity;
        return;
      }
      
      jumpDuration = 0.0f;
      fallVelocity = 0.0f;
      state.IsGrounded = true;
      state.Position = hit.point - offset * config.radius;
    }
    
    private Ground GetGround(int hitIndex) {
      return movement.GetGround(HitCache[hitIndex].collider);
    }

    private Ground GetNearestCacheGround(int count, out RaycastHit hit) {
      Ground nearestGround = null;
      var nearest = float.MaxValue;
      hit = HitCache[0];
      for (var i = 0; i < count; i++) {
        var ground = GetGround(i);
        if (ground == null) {
          continue;
        }
        
        if (myGround.Contains(ground)) {
          continue;
        }
        
        var distance = HitCache[i].distance;
        if (distance >= nearest) {
          continue;
        }

        hit = HitCache[i];
        nearestGround = ground;
        nearest = distance;
      }
      return nearestGround;
    }
    
    private Ground WidthCast(Vector3 position, Vector3 direction, float radius, float distance, out RaycastHit hit) {
      direction = direction.normalized;
      Vector3[] positions ={
        position + Vector3.Cross(direction, Vector3.up).normalized * radius,
        position,
        position + Vector3.Cross(Vector3.up, direction).normalized * radius
      };
      Ground nearestGround = null;
      var nearestDistance = float.MaxValue;
      hit = HitCache[0];
      foreach (var aPos in positions) {
        if (movement.Config.drawDebug) {
          Debug.DrawRay(aPos, direction * distance, Color.yellow);
        }
        var hitCount = Physics.RaycastNonAlloc(aPos, direction, HitCache, distance, groundLayerMask);
        var ground = GetNearestCacheGround(hitCount, out var groundHit);
        if(ground == null){
          continue;
        }
        if (groundHit.distance >= nearestDistance) {
          continue;
        }
        nearestGround = ground;
        nearestDistance = groundHit.distance;
        hit = groundHit;
      }
      return nearestGround;
    }
    
    private Ground DownCast(Vector3 position, float radius, float distance, out RaycastHit hit, out Vector3 hitOffset) {
      var direction = Vector3.down;
      Ground nearestGround = null;
      var nearestDistance = float.MaxValue;
      hit = HitCache[0];
      hitOffset = Vector3.zero;
      foreach (var offset in DowncastOffsets) {
        var pos = position + (offset * radius) + Vector3.up * movement.Config.checkBackupDistance;
        var checkDistance = distance + movement.Config.checkBackupDistance;
        if (movement.Config.drawDebug) {
          Debug.DrawRay(pos, direction * checkDistance, Color.magenta);
        }
        var hitCount = Physics.RaycastNonAlloc(pos, direction, HitCache, checkDistance, groundLayerMask);
        var ground = GetNearestCacheGround(hitCount, out var groundHit);
        if(ground == null){
          continue;
        }
        if (groundHit.distance >= nearestDistance) {
          continue;
        }
        hitOffset = offset;
        nearestGround = ground;
        nearestDistance = groundHit.distance;
        hit = groundHit;
      }
      return nearestGround;
    }
    
    private Ground UpCast(Vector3 position, float distance, out RaycastHit hit) {
      var dirNormal = Vector3.up;
      var checkStart = position + Vector3.down * movement.Config.checkBackupDistance;
      var checkDistance = distance + movement.Config.checkBackupDistance;
      if (movement.Config.drawDebug) {
        Debug.DrawRay(checkStart, dirNormal.normalized * checkDistance, Color.magenta);
      }
      var hitCount = Physics.RaycastNonAlloc(checkStart, dirNormal, HitCache, checkDistance, groundLayerMask);
      var nearestGround = GetNearestCacheGround(hitCount, out var upHit);
      hit = upHit;
      return nearestGround;
    }

    public void OnDestroy() {
      events.Release();
    }

  }
  
}
