using System.Collections;
using UnityEngine;
using NETWORK_ENGINE;

public class NetRigidbody2D : NetworkComponent {
  #region FIELDS
  public Vector2 lastPosition;
  public Vector2 lastVelocity;

  public Vector2 adaptiveVelocity;
  public float speed = 5.0f;
  public bool usingAdaptiveSpeed = true;

  public float minThreshold = 0.1f;
  public float maxThreshold = 3.0f;

  [SerializeField] private Rigidbody2D rb;
  #endregion

  #region M_NETWORK_ENGINE
  public override void NetworkedStart() { }

  public override void HandleMessage(string flag, string value) {
    if (flag == "POS") {
      if (IsServer) {
        Debug.Log($"SERVER: {name} NetRigidbody2D: {flag}, {value}");
      }
      if (IsClient) {
        Debug.Log($"CLIENT: {name} NetRigidbody2D: {flag}, {value}");
      }
      if (IsLocalPlayer) {
        Debug.Log($"LOCAL: {name} NetRigidbody2D: {flag}, {value}");
      }

      if (IsClient) {
        lastPosition = Parser.ParseVector2(value);

        float deltaPosition = (lastPosition - rb.position).magnitude;

        if (deltaPosition > maxThreshold) {
          rb.position = lastPosition;
          adaptiveVelocity = Vector2.zero;
        }
        else if (deltaPosition > minThreshold && usingAdaptiveSpeed) {
          adaptiveVelocity = (lastPosition - rb.position).normalized * speed * Time.deltaTime;
        }
      }

      if (flag == "VEL") {
        if (IsServer) {
          Debug.Log($"SERVER: {name} NetRigidbody2D: {flag}, {value}");
        }
        if (IsClient) {
          Debug.Log($"CLIENT: {name} NetRigidbody2D: {flag}, {value}");
        }
        if (IsLocalPlayer) {
          Debug.Log($"LOCAL: {name} NetRigidbody2D: {flag}, {value}");
        }

        if (IsClient) {
          lastVelocity = Parser.ParseVector2(value);

          float deltaVelocity = lastVelocity.magnitude;

          if (deltaVelocity < minThreshold) {
            lastVelocity = Vector2.zero;
            adaptiveVelocity = Vector2.zero;
          }
        }
      }
    }
  }

  public override IEnumerator SlowUpdate() {
    while (IsServer) {
      float deltaPosition = (lastPosition - rb.position).magnitude;
      float deltaVelocity = (lastVelocity - rb.velocity).magnitude;

      if (deltaPosition > minThreshold) {
        lastPosition = rb.position;
        Debug.Log($"SERVER: deltaPosition: {deltaPosition} > minThreshold: {minThreshold}. Updating POSITION.");
        SendUpdate("POS", lastPosition.ToString("F2"));
      }

      if (deltaVelocity > minThreshold) {
        lastVelocity = rb.velocity;
        Debug.Log($"SERVER: deltaVelocity: {deltaVelocity} > minThreshold: {minThreshold}. Updating VELOCITY.");
        SendUpdate("VEL", lastVelocity.ToString("F2"));
      }

      if (IsDirty) {
        Debug.Log($"SERVER: {name} NetRigidbody2D is dirty. Sending updates.");
        SendUpdate("POS", lastPosition.ToString("F2"));
        SendUpdate("VEL", lastVelocity.ToString("F2"));
        IsDirty = false;
      }
      yield return new WaitForSeconds(0.05f);
    }
  }
  #endregion

  #region M_UNITY
  void Start() {
    rb = GetComponent<Rigidbody2D>();
    usingAdaptiveSpeed = true;
  }

  void Update() {
    if (IsClient) {
      //rb.position = lastPosition;
      Debug.Log($"CLIENT: rb.velocity: {rb.velocity} = lastVelocity: {lastVelocity}");
      rb.velocity = lastVelocity;

      if (rb.velocity.magnitude > minThreshold && usingAdaptiveSpeed) {
        rb.velocity += adaptiveVelocity;
      }
    }
  }
  #endregion
}
