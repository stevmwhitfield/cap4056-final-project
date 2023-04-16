using System.Collections;
using UnityEngine;
using NETWORK_ENGINE;

[RequireComponent(typeof(Rigidbody2D))]
public class NetRigidbody2D : NetworkComponent {
  #region FIELDS

  [SerializeField]
  private Rigidbody2D rb;

  public Vector2 lastPosition;
  public Vector2 lastVelocity;
  public Vector2 adaptiveVelocity;
  private float minThreshold = 0.1f;
  private float maxThreshold = 1f;
  private bool usingAdaptiveVelocity = false;

  #endregion

  #region UNITY

  void Start() {
    rb = GetComponent<Rigidbody2D>();
  }

  void Update() {
    if (IsClient) {
      if (lastVelocity.magnitude < 0.05f) {
        adaptiveVelocity = Vector2.zero;
      }

      if (usingAdaptiveVelocity) {
        rb.velocity = lastVelocity + adaptiveVelocity;
      }
      else {
        rb.velocity = lastVelocity;
      }
    }
  }

  #endregion

  #region NETWORK_ENGINE

  public override void NetworkedStart() { }

  public override void HandleMessage(string flag, string value) {
    if (flag == "POS") {
      if (IsClient) {
        lastPosition = Parser.ParseVector2(value);

        float deltaPosition = (rb.position - lastPosition).magnitude;

        if (deltaPosition > maxThreshold || lastVelocity.magnitude < 0.1f || !usingAdaptiveVelocity) {
          rb.position = lastPosition;
          adaptiveVelocity = Vector2.zero;
        }
        else {
          adaptiveVelocity = lastPosition - rb.position;
        }
      }
    }

    if (flag == "VEL") {
      if (IsClient) {
        lastVelocity = Parser.ParseVector2(value);
      }
    }
  }

  public override IEnumerator SlowUpdate() {
    if (IsClient) {
      rb.gravityScale = 0f;
    }

    while (IsServer) {
      float deltaPosition = (lastPosition - rb.position).magnitude;
      float deltaVelocity = (lastVelocity - rb.velocity).magnitude;

      if (deltaPosition > minThreshold) {
        lastPosition = rb.position;
        SendUpdate("POS", lastPosition.ToString("F2"));
      }

      if (deltaVelocity > minThreshold) {
        lastVelocity = rb.velocity;
        SendUpdate("VEL", lastVelocity.ToString("F2"));
      }

      if (IsDirty) {
        SendUpdate("POS", lastPosition.ToString("F2"));
        SendUpdate("VEL", lastVelocity.ToString("F2"));
        IsDirty = false;
      }

      yield return new WaitForSeconds(0.01f);
    }
  }

  #endregion
}
