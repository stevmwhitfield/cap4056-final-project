using System.Collections;
using UnityEngine;
using NETWORK_ENGINE;

public class NetRigidbody2D : NetworkComponent {
  #region FIELDS
  private Vector2 lastPosition;
  private Vector2 lastVelocity;
  private float lastAngularVelocity;
  private float lastRotation;

  private Vector2 adaptiveVelocity;
  private float speed;
  private bool usingAdaptiveSpeed;

  private float minThreshold = 0.1f;
  private float maxThreshold = 3.0f;

  [SerializeField] private Rigidbody2D rb;
  #endregion

  #region M_NETWORK_ENGINE
  public override void NetworkedStart() { }

  public override void HandleMessage(string flag, string value) {
    if (flag == "POS") {
      if (IsClient) {
        lastPosition = Parser.ParseVector3(value);
        float deltaPosition = (lastPosition - rb.position).magnitude;
        if (deltaPosition > maxThreshold) {
          rb.position = lastPosition;
          adaptiveVelocity = Vector3.zero;
        }
        else if (deltaPosition > minThreshold && usingAdaptiveSpeed) {
          adaptiveVelocity = (lastPosition - rb.position).normalized * speed * Time.deltaTime;
        }
      }

      if (flag == "ROT") {
        if (IsClient) {
          lastRotation = float.Parse(value);
        }
      }

      if (flag == "VEL") {
        lastVelocity = Parser.ParseVector2(value);
        float deltaVelocity = lastVelocity.magnitude;
        if (deltaVelocity < minThreshold) {
          lastVelocity = Vector2.zero;
          adaptiveVelocity = Vector2.zero;
        }
      }

      if (flag == "ANG") {
        lastAngularVelocity = float.Parse(value);
        if (lastAngularVelocity < minThreshold) {
          lastAngularVelocity = 0;
        }
      }
    }
  }

  public override IEnumerator SlowUpdate() {
    while (IsServer) {
      float deltaPosition = (lastPosition - rb.position).magnitude;
      float deltaRotation = lastRotation - rb.rotation;
      float deltaVelocity = (lastVelocity - rb.velocity).magnitude;
      float deltaAngularVelocity = lastAngularVelocity - rb.angularVelocity;

      if (deltaPosition > minThreshold) {
        lastPosition = rb.position;
        SendUpdate("POS", lastPosition.ToString("F2"));
      }

      if (deltaRotation > minThreshold) {
        lastRotation = rb.rotation;
        SendUpdate("ROT", lastRotation.ToString("F2"));
      }

      if (deltaVelocity > minThreshold) {
        lastVelocity = rb.velocity;
        SendUpdate("VEL", lastVelocity.ToString("F2"));
      }

      if (deltaAngularVelocity > minThreshold) {
        lastAngularVelocity = rb.angularVelocity;
        SendUpdate("ANG", lastAngularVelocity.ToString("F2"));
      }

      if (IsDirty) {
        SendUpdate("POS", lastPosition.ToString("F2"));
        SendUpdate("ROT", lastRotation.ToString("F2"));
        SendUpdate("VEL", lastVelocity.ToString("F2"));
        SendUpdate("ANG", lastAngularVelocity.ToString("F2"));
        IsDirty = false;
      }
      yield return new WaitForSeconds(0.1f);
    }
  }
  #endregion

  #region M_UNITY
  void Start() {
    rb = GetComponent<Rigidbody2D>();
  }

  void Update() {
    if (IsClient) {
      rb.velocity = lastVelocity;
      rb.rotation = lastRotation;
      rb.angularVelocity = lastAngularVelocity;

      if (rb.velocity.magnitude > minThreshold && usingAdaptiveSpeed) {
        rb.velocity += adaptiveVelocity;
      }
    }
  }
  #endregion
}