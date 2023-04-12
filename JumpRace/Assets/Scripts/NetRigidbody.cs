using System.Collections;
using UnityEngine;
using NETWORK_ENGINE;

public class NetRigidbody : NetworkComponent {
  #region FIELDS
  public Vector3 lastPosition;
  public Vector3 lastVelocity;
  public Vector3 lastAngularVelocity;
  public Vector3 lastRotation;

  public Vector3 adaptiveVelocity;
  public float speed = 5.0f;
  public bool usingAdaptiveSpeed = true;

  public float minThreshold = 0.1f;
  public float maxThreshold = 3.0f;

  [SerializeField] private Rigidbody rb;
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
          lastRotation = Parser.ParseVector3(value);
        }
      }

      if (flag == "VEL") {
        lastVelocity = Parser.ParseVector3(value);
        float deltaVelocity = lastVelocity.magnitude;
        if (deltaVelocity < minThreshold) {
          lastVelocity = Vector3.zero;
          adaptiveVelocity = Vector3.zero;
        }
      }

      if (flag == "ANG") {
        lastAngularVelocity = Parser.ParseVector3(value);
        if (lastAngularVelocity.magnitude < minThreshold) {
          lastAngularVelocity = Vector3.zero;
        }
      }
    }
  }

  public override IEnumerator SlowUpdate() {
    while (IsServer) {
      float deltaPosition = (lastPosition - rb.position).magnitude;
      float deltaRotation = (lastRotation - rb.rotation.eulerAngles).magnitude;
      float deltaVelocity = (lastVelocity - rb.velocity).magnitude;
      float deltaAngularVelocity = (lastAngularVelocity - rb.angularVelocity).magnitude;

      if (deltaPosition > minThreshold) {
        lastPosition = rb.position;
        SendUpdate("POS", lastPosition.ToString("F2"));
      }

      if (deltaRotation > minThreshold) {
        lastRotation = rb.rotation.eulerAngles;
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
      yield return new WaitForSeconds(0.05f);
    }
  }
  #endregion

  #region M_UNITY
  void Start() {
    rb = GetComponent<Rigidbody>();
  }

  void Update() {
    if (IsClient) {
      rb.velocity = lastVelocity;
      rb.rotation = Quaternion.Euler(lastRotation);
      rb.angularVelocity = lastAngularVelocity;

      if (rb.velocity.magnitude > minThreshold && usingAdaptiveSpeed) {
        rb.velocity += adaptiveVelocity;
      }
    }
  }
  #endregion
}
