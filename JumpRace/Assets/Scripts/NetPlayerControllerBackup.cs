using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using NETWORK_ENGINE;

public class NetPlayerControllerBackup : NetworkComponent {
  #region CONSTANTS
  private const float WALK_SPEED = 5.0f;
  private const float JUMP_SPEED = 11.0f;
  #endregion

  #region PROPERTIES
  public string Name { get; set; }
  #endregion

  #region FIELDS
  [SerializeField] private Rigidbody2D rb;
  private Vector2 lastInput;
  public float maxJumpTime = 1.0f;
  public float currentJumpTime;
  public bool isJumping = false;
  public bool isGrounded = true;
  public bool lastJump;
  #endregion

  #region M_NETWORK_ENGINE
  public override void NetworkedStart() {
    if (IsServer) {
      rb = GetComponent<Rigidbody2D>();
      if (rb == null) {
        throw new System.Exception($"ERROR: {name} is missing a Rigidbody2D component!");
      }
    }

    if (IsLocalPlayer) {
      Camera.main.orthographic = true;
      Camera.main.orthographicSize = 4.5f;
    }
  }

  public override void HandleMessage(string flag, string value) {
    if (flag == "INPUT") {
      if (IsServer) {
        lastInput = Parser.ParseVector2(value);
      }
    }

    if (flag == "CAN_JUMP") {
      if (IsServer) {
        isGrounded = bool.Parse(value);
      }
    }

    if (flag == "LAST_JUMP") {
      if (IsServer) {
        lastJump = bool.Parse(value);
      }
    }

    if (flag == "JUMPING") {
      if (IsServer) {
        isJumping = bool.Parse(value);
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
      isGrounded = IsGrounded();
      SendUpdate("CAN_JUMP", isGrounded.ToString());

      if (isGrounded) {
        currentJumpTime = maxJumpTime;
        Move();

        if (lastJump) {
          lastJump = false;
          SendUpdate("LAST_JUMP", lastJump.ToString());
          Jump();
        }
      }

      if (!isJumping) {
        rb.velocity = new Vector2(rb.velocity.x, 0);
      }

      if (IsDirty) {
        SendUpdate("NAME", Name);
        SendUpdate("CAN_JUMP", isGrounded.ToString());
        SendUpdate("LAST_JUMP", lastJump.ToString());
        IsDirty = false;
      }

      yield return new WaitForSeconds(0.05f);
    }
  }
  #endregion

  #region M_UNITY
  void Start() {
    isGrounded = true;
    currentJumpTime = maxJumpTime;
  }

  void Update() {
    if (IsClient) {
      // animations
    }

    if (IsLocalPlayer) {
      Vector3 offset = new Vector3(0, 2, -10);
      Camera.main.transform.position = transform.position + offset;
    }
  }
  #endregion

  #region M_INPUT
  public void OnMove(InputAction.CallbackContext c) {
    if (IsLocalPlayer) {
      if (c.started || c.performed) {
        Vector2 input = c.ReadValue<Vector2>();
        SendCommand("INPUT", input.ToString("F2"));
      }
      else if (c.canceled) {
        SendCommand("INPUT", Vector2.zero.ToString());
      }
    }
  }

  public void OnJump(InputAction.CallbackContext c) {
    if (IsLocalPlayer) {
      if (c.started) {
        SendCommand("LAST_JUMP", true.ToString());
        SendCommand("JUMPING", true.ToString());
      }
      else if (c.canceled) {
        SendCommand("JUMPING", false.ToString());
      }
    }
  }
  #endregion

  #region M_MOVEMENT
  private void Move() {
    rb.velocity = new Vector2(lastInput.x, 0) * WALK_SPEED;
  }

  private void Jump() {
    isGrounded = false;
    SendUpdate("CAN_JUMP", isGrounded.ToString());

    if (isJumping) {
      rb.velocity = new Vector2(lastInput.x, 0) * WALK_SPEED + new Vector2(0, JUMP_SPEED);
    }

    //rb.velocity = new Vector2(lastInput.x, 0) * WALK_SPEED + new Vector2(0, JUMP_SPEED);
  }

  private bool IsGrounded() {
    bool isGrounded = false;

    if (rb.velocity.y <= 0) {
      LayerMask walkableLayer = LayerMask.GetMask("Walkable");
      Vector2 origin = transform.position;
      Vector2 direction = Vector2.down;
      float distance = 0.22f;

      RaycastHit2D centerHit = Physics2D.Raycast(origin, direction, distance, walkableLayer);
      RaycastHit2D leftHit = Physics2D.Raycast(origin + new Vector2(-0.3f, 0), direction, distance, walkableLayer);
      RaycastHit2D rightHit = Physics2D.Raycast(origin + new Vector2(0.3f, 0), direction, distance, walkableLayer);

      if (centerHit.collider != null || leftHit.collider != null || rightHit.collider != null) {
        isGrounded = true;
      }
    }

    return isGrounded;
  }
  #endregion
}
