using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using NETWORK_ENGINE;

public class NetPlayerController : NetworkComponent {
  #region FIELDS
  private Rigidbody2D rb;
  private Vector2 lastInput;
  private float speed = 5.0f;
  private bool canJump = true;
  private bool lastJump;
  #endregion

  #region PROPERTIES
  public string Name { get; set; } = "<Name>";
  #endregion

  #region M_NETWORK_ENGINE
  public override void NetworkedStart() { }

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
      yield return new WaitForSeconds(0.1f);
    }
  }
  #endregion

  #region M_UNITY
  void Start() {
    rb = GetComponent<Rigidbody2D>();
    if (rb == null) {
      throw new System.Exception($"ERROR: {name} is missing a Rigidbody2D component!");
    }
  }

  void Update() {
    rb.velocity = new Vector2(lastInput.x, 0) * speed;

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
