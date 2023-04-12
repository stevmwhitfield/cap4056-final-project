using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using NETWORK_ENGINE;

public class NetPlayerController3D : NetworkComponent {
  #region FIELDS
  public Rigidbody rb;
  public Vector2 lastInput;
  public float speed = 5.0f;
  public bool canJump = true;
  public bool lastJump;
  #endregion

  #region PROPERTIES
  public string Name { get; set; } = "<Name>";
  #endregion

  #region M_NETWORK_ENGINE
  public override void NetworkedStart() {
    if (IsServer) {
      rb = GetComponent<Rigidbody>();
      if (rb == null) {
        throw new System.Exception($"ERROR: {name} is missing a Rigidbody component!");
      }
    }
  }

  public override void HandleMessage(string flag, string value) {
    if (flag == "MOVE") {
      if (IsServer) {
        lastInput = Parser.ParseVector2(value);
      }
    }

    if (flag == "JUMP") {
      if (IsServer) {
        lastJump = bool.Parse(value);
      }
    }

    if (flag == "NAME") {
      if (IsClient) {
        Name = value;
        transform.GetComponentInChildren<TMP_Text>().text = Name;
      }
    }
  }

  public override IEnumerator SlowUpdate() {
    while (IsServer) {
      //if (lastJump && canJump) {
      //  canJump = false;
      //  SendUpdate("CAN_JUMP", canJump.ToString());
      //  // jump logic
      //}
      if (IsDirty) {
        SendUpdate("NAME", Name);
      }
      yield return new WaitForSeconds(0.05f);
    }
  }
  #endregion

  #region M_UNITY
  void Start() { }

  void Update() {
    if (IsServer) {
      Vector3 currentVerticalVelocity = new Vector3(0, rb.velocity.y, 0);
      rb.velocity = new Vector3(lastInput.x, 0, 0) * speed + currentVerticalVelocity;
    }

    if (IsClient) {
      // animations
    }
  }
  #endregion

  #region M_INPUT
  public void OnMove(InputAction.CallbackContext c) {
    if (IsLocalPlayer) {
      if (c.phase == InputActionPhase.Started || c.phase == InputActionPhase.Performed) {
        Vector2 input = c.ReadValue<Vector2>();
        SendCommand("MOVE", input.ToString("F2"));
      }
      else if (c.phase == InputActionPhase.Canceled) {
        SendCommand("MOVE", Vector2.zero.ToString());
      }
    }
  }

  public void OnJump(InputAction.CallbackContext c) {
    if (IsLocalPlayer) {
      if (canJump) {
        if (c.phase == InputActionPhase.Started) {
          SendCommand("JUMP", true.ToString());
        }
      }
    }
  }
  #endregion
}
