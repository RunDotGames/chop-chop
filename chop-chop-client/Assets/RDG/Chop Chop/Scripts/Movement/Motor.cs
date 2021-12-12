using UnityEngine;
using System;
using System.Linq;

namespace RDG.Chop_Chop.Scripts.Movement {
  [Serializable]
  public class MotorConfig {
    public float moveSpeed = 10.0f;
    public float jumpSpeed = 10.0f;
    public float rotationSpeed = 10.0f;
    public float radius = 1.0f;
    public float height = 1.0f;
  }


  public interface MotorDirector {
    public Vector2 GetDirection();
    public bool GetJumpRequested();
  }

  public class Motor {

    public event Action OnGrounded;
    
    class MovementInfo {
      public Vector3 Position { get; set; }
      public Quaternion Rotation { get; set; }
      public Vector3 ForwardNormal { get; set; }
      public float RemainingDistance { get; set; }
      public bool IsJumping { get; set; }
      public bool IsGrounded { get; set; }

    }

    private readonly Rigidbody body;
    private readonly MotorConfig motorConfig;
    private readonly MovementConfig movementConfig;
    private readonly MotorDirector director;
    private readonly MovementProvider movementProvider;
    private readonly LayerMask groundLayerMask;
    
    private float fallVelocity;
    private float jumpDuration;
    private bool canJump;
    private bool isGrounded;
    
    private static readonly RaycastHit[] HitCache ={
      new RaycastHit(), new RaycastHit(), new RaycastHit()
    };


    public Motor(
      MovementConfig movementConfig,
      MotorConfig motorConfig,
      Rigidbody body,
      MotorDirector director,
      MovementProvider movementProvider
    ) {
      this.body = body;
      // ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
      const RigidbodyConstraints freezeRotation = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
      body.constraints = freezeRotation;
      this.movementConfig = movementConfig;
      this.motorConfig = motorConfig;
      this.director = director;
      this.movementProvider = movementProvider;
      groundLayerMask = LayerMask.GetMask(movementConfig.groundLayerName);
    }

    private Ground GetGround(int hitIndex) {
      return movementProvider.GetGround(HitCache[hitIndex].collider);
    }
    
    private int WidthCast(Vector3 position, Vector3 direction, float radius, float distance) {
      Vector3[] positions ={
        position + Vector3.Cross(direction, Vector3.up).normalized * radius,
        position,
        position + Vector3.Cross(Vector3.up, direction).normalized * radius
      };
      var nearestIndex = -1;
      var nearestDistance = float.MaxValue;
      for (var i = 0; i < positions.Length; i++) {
        if (movementConfig.drawDebug) {
          Debug.DrawRay(positions[i], direction * distance, Color.yellow);
        }
        if (!Physics.Raycast(positions[i], direction, out HitCache[i], distance, groundLayerMask)) {
          continue;
        }
        if (HitCache[i].distance >= nearestDistance) {
          continue;
        }
        nearestIndex = i;
        nearestDistance = HitCache[i].distance;
      }
      return nearestIndex;
    }
    
    private int DownCast(Vector3 position, float distance) {
      var dirNormal = Vector3.down;
      var checkStart = position + Vector3.up * movementConfig.checkBackupDistance;
      var checkDistance = distance + movementConfig.checkBackupDistance;
      if (movementConfig.drawDebug) {
        Debug.DrawRay(checkStart, dirNormal.normalized * checkDistance, Color.magenta);
      }
      return Physics.Raycast(checkStart, dirNormal, out HitCache[0], checkDistance, groundLayerMask) ? 0 : -1;
    }
    
    private int UpCast(Vector3 position, float distance) {
      var dirNormal = Vector3.up;
      var checkStart = position + Vector3.down * movementConfig.checkBackupDistance;
      var checkDistance = distance + movementConfig.checkBackupDistance;
      if (movementConfig.drawDebug) {
        Debug.DrawRay(checkStart, dirNormal.normalized * checkDistance, Color.magenta);
      }
      return Physics.Raycast(checkStart, dirNormal, out HitCache[0], checkDistance, groundLayerMask) ? 0 : -1;
    }

    private MovementInfo JumpUp(MovementInfo state) {
      if (!director.GetJumpRequested()) {
        return state;
      }
      
      canJump = canJump && director.GetJumpRequested();
      if (!canJump) {
        return state;
      }
      
      var maxJump = movementConfig.jumpCurve.keys.Last().time;
      if (jumpDuration > maxJump) {
        canJump = false;
        return state;
      }
      
      state.IsJumping = true;
      jumpDuration += Time.deltaTime;
      var jumpSpeed = motorConfig.jumpSpeed * movementConfig.jumpCurve.Evaluate(jumpDuration) * Time.deltaTime;
      var heightOffset = Vector3.up * motorConfig.height;
      var hitIndex = UpCast(state.Position + heightOffset, jumpSpeed);
      if (hitIndex < 0) {
        state.Position += Vector3.up * jumpSpeed;
        return state;
      }

      state.Position = HitCache[hitIndex].point - heightOffset;
      canJump = false;
      return state;
    }

    private MovementInfo GroundForward(MovementInfo state) {
      var hitIndex = DownCast(state.Position, movementConfig.groundCheckDistance);
      if (hitIndex < 0 || GetGround(hitIndex) == null) {
        return state;
      }

      if (state.ForwardNormal.sqrMagnitude == 0) {
        return state;
      }
      
      var cross = Vector3.Cross(HitCache[hitIndex].normal, state.ForwardNormal);
      var groundTangentDir = Vector3.Cross(cross, HitCache[hitIndex].normal);
      state.ForwardNormal = groundTangentDir.normalized;
      return state;
    }
    
    private MovementInfo MoveForward(MovementInfo state) {
      if (state.ForwardNormal.sqrMagnitude == 0 || state.RemainingDistance <= 0.0f) {
        return state;
      }

      var distance = state.RemainingDistance + motorConfig.radius;
      var position = state.Position + Vector3.up * (movementConfig.wallHeightCheck);
      var hitIndex = WidthCast(position, state.ForwardNormal, motorConfig.radius, distance);
      var wall = hitIndex >= 0 ? GetGround(hitIndex) : null;
      if (wall == null) {
        state.Position += state.ForwardNormal * state.RemainingDistance;
        state.RemainingDistance = 0;
        return state;
      }

      //Move up to or out out of wall
      var hitDistance = HitCache[hitIndex].distance;
      state.Position += state.ForwardNormal * (hitDistance - motorConfig.radius);
      state.RemainingDistance -= (hitDistance - motorConfig.radius);
      
      //Forward is now tangent to wall
      var cross = Vector3.Cross(HitCache[hitIndex].normal, state.ForwardNormal);
      state.ForwardNormal = Vector3.Cross(cross, HitCache[hitIndex].normal);
      return state;
    }

    // Rotate to face lace move step direction
    private MovementInfo RotateForward(MovementInfo state) {
      if (state.ForwardNormal.sqrMagnitude == 0) {
        return state;
      }

      state.Rotation = Quaternion.RotateTowards(
        state.Rotation,
        Quaternion.LookRotation(state.ForwardNormal, Vector3.up),
        Time.fixedDeltaTime * motorConfig.rotationSpeed
      );
      return state;
    }

    private MovementInfo FallDown(MovementInfo state) {
      if (state.IsJumping) {
        return state;
      }
      
      fallVelocity += (movementConfig.fallRate * Time.deltaTime);
      fallVelocity = Mathf.Min(fallVelocity, movementConfig.fallMaxSpeed);
      var hitIndex = DownCast(state.Position, fallVelocity);
      if (hitIndex < 0 || GetGround(hitIndex) == null) {
        state.Position += Vector3.down * fallVelocity;
        return state;
      }
      
      jumpDuration = 0.0f;
      fallVelocity = 0.0f;
      state.IsGrounded = true;
      state.Position = HitCache[hitIndex].point;
      return state;
    }

    public void FixedUpdate() {
      var transform = body.transform;
      var directorDir = director.GetDirection();
      
      var info = new MovementInfo{
        Position = transform.position,
        Rotation = transform.rotation,
        ForwardNormal = new Vector3(directorDir.x, 0, directorDir.y).normalized,
        IsJumping = false,
        RemainingDistance = motorConfig.moveSpeed * Time.deltaTime
      };
      
      info = JumpUp(info);
      info = RotateForward(info);
      info = GroundForward(info);
      info = MoveForward(info);
      info = MoveForward(info);
      info = MoveForward(info);
      info = FallDown(info);

      transform.position = info.Position;
      transform.rotation = info.Rotation;
      
      var wasGrounded = isGrounded;
      isGrounded = info.IsGrounded;
      if (isGrounded && !wasGrounded) {
        OnGrounded?.Invoke();
      }
      if (isGrounded && !director.GetJumpRequested()) {
        canJump = true;
      }
    }
  }
}