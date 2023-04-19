using System.Collections;
using UnityEngine;
using TMPro;
using NETWORK_ENGINE;

public class NetJumperController : NetworkComponent {
  #region FIELDS

  [SerializeField]
  private Rigidbody2D rb;

  [SerializeField]
  private NetInputController input;

  [SerializeField]
  private PhysicsMaterial2D bouncyMaterial;

  [SerializeField]
  private PhysicsMaterial2D normalMaterial;

  [SerializeField]
  private ContactFilter2D contactFilter;

  [SerializeField]
  private Animator animator;

  private float moveSpeed;    // horizontal movement speed
  private float jumpSpeed;    // variable jump speed
  private float minJumpSpeed; // minimum jump speed, used if jumpSpeed is too low
  private float maxJumpSpeed; // maximum jump speed, used if jumpSpeed passes upper limit

  #endregion

  #region PROPERTIES

  public string Name { get; set; }
  public float VelocityX => input.Direction * moveSpeed;  // return horizontal velocity
  public bool IsJumping => input.IsJumping;               // return true if player is pressing jump input
  public bool IsGrounded => rb.IsTouching(contactFilter); // return true if rigidbody is touching the contact filter

  #endregion

  #region UNITY

  void Start() {
    rb = GetComponent<Rigidbody2D>();
    if (rb == null) {
      throw new System.Exception($"ERROR: {name} is missing a Rigidbody2D component!");
    }

    animator = GetComponent<Animator>();
    if (animator == null) {
      throw new System.Exception($"ERROR: {name} is missing an Animator component!");
    }

    moveSpeed = 5f;
    jumpSpeed = 0f;
    minJumpSpeed = 12f;
    maxJumpSpeed = 20f;
  }

  void Update() {
    if (IsClient) {
      animator.SetFloat("xVelocity", rb.velocity.x);
      animator.SetFloat("yVelocity", rb.velocity.y);
      animator.SetFloat("jumpCharge", jumpSpeed);
      animator.SetInteger("jumpDirection", input.Direction);
      animator.SetBool("isGrounded", IsGrounded);
    }

    if (IsLocalPlayer) {
      Vector3 offset = new Vector3(0, 1, -10);
      Camera.main.transform.position = transform.position + offset;
    }
  }

  void FixedUpdate() {
    if (IsServer) {
      if (IsJumping && IsGrounded) {
        jumpSpeed += 0.5f;
        SendUpdate("JUMP_SPEED", jumpSpeed.ToString());
      }
    }
  }

  #endregion

  #region NETWORK_ENGINE

  public override void NetworkedStart() {
    if (IsServer) {
      rb.gravityScale = 4f;
    }

    if (IsLocalPlayer) {
      input = GetComponent<NetInputController>();
      if (input == null) {
        throw new System.Exception($"ERROR: {name} is missing a NetInputController script!");
      }

      Camera.main.orthographic = true;
      Camera.main.orthographicSize = 11.25f;
    }
  }

  public override void HandleMessage(string flag, string value) {
    if (flag == "NAME") {
      if (IsClient) {
        Name = value;
        transform.GetComponentInChildren<TMP_Text>().text = Name;
      }
    }

    if (flag == "JUMP_SPEED") {
      jumpSpeed = float.Parse(value);
      if (IsServer) {
        SendUpdate(flag, value);
      }
    }
  }

  public override IEnumerator SlowUpdate() {
    if (IsClient) {
      rb.gravityScale = 0f;
    }

    while (IsServer) {
      Jump();
      Move();

      if (IsDirty) {
        SendUpdate("NAME", Name);
        SendUpdate("JUMP_SPEED", jumpSpeed.ToString());
        IsDirty = false;
      }

      yield return new WaitForSeconds(0.01f);
    }
  }

  #endregion

  #region MOVEMENT

  private void Move() {
    if (IsGrounded && jumpSpeed == 0) {
      rb.velocity = new(VelocityX, rb.velocity.y);
    }
  }

  private void Jump() {
    if (IsJumping && IsGrounded) {
      rb.velocity = new(0f, rb.velocity.y);
      rb.sharedMaterial = bouncyMaterial;

      if (jumpSpeed >= maxJumpSpeed) {
        rb.velocity = new(VelocityX, maxJumpSpeed);
        Invoke("ResetJumpSpeed", 0.025f);
      }
    }

    if (!IsJumping) {
      if (jumpSpeed >= minJumpSpeed) {
        rb.velocity = new(VelocityX, jumpSpeed);
        Invoke("ResetJumpSpeed", 0.025f);
      }
      else if (jumpSpeed > 0) {
        rb.velocity = new(VelocityX, minJumpSpeed);
        Invoke("ResetJumpSpeed", 0.025f);
      }
    }

    if (rb.velocity.y <= -1f) {
      rb.sharedMaterial = normalMaterial;
    }
  }

  private void ResetJumpSpeed() {
    jumpSpeed = 0f;
    SendUpdate("JUMP_SPEED", jumpSpeed.ToString());
  }

  #endregion
}
