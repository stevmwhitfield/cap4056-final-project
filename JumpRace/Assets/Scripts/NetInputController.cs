using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using NETWORK_ENGINE;

public class NetInputController : NetworkComponent {
  #region PROPERTIES
  public int Direction { get; private set; }
  public bool IsJumping { get; private set; }
  #endregion

  #region INPUT_CALLBACKS
  public void OnMove(InputAction.CallbackContext c) {
    if (IsLocalPlayer) {
      if (c.started || c.performed) {
        int direction = (int)c.ReadValue<Vector2>().x;
        SendCommand("DIRECTION", direction.ToString());
      }
      else if (c.canceled) {
        SendCommand("DIRECTION", 0.ToString());
      }
    }
  }

  public void OnJump(InputAction.CallbackContext c) {
    if (IsLocalPlayer) {
      if (c.started) {
        SendCommand("JUMPING", true.ToString());
      }
      else if (c.canceled) {
        SendCommand("JUMPING", false.ToString());
      }
    }
  }
  #endregion

  #region NETWORK_ENGINE
  public override void NetworkedStart() { }

  public override void HandleMessage(string flag, string value) {
    if (flag == "DIRECTION") {
      Direction = int.Parse(value);

      if (IsServer) {
        SendUpdate(flag, value);
      }
    }

    if (flag == "JUMPING") {
      if (IsServer) {
        IsJumping = bool.Parse(value);
      }
    }
  }

  public override IEnumerator SlowUpdate() {
    while (IsServer) {
      if (IsDirty) {
        SendUpdate("DIRECTION", Direction.ToString());
        SendUpdate("JUMPING", IsJumping.ToString());
        IsDirty = false;
      }
      yield return new WaitForSeconds(0.1f);
    }
  }
  #endregion
}
