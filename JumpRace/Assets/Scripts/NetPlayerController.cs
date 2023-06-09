using System.Collections;
using UnityEngine;
using TMPro;
using NETWORK_ENGINE;

public class NetPlayerController : NetworkComponent {
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

  //#region CONSTANTS
  //private const float MOVE_SPEED = 5f;
  //public float JUMP_SPEED = 600f;
  //#endregion

  //#region PROPERTIES
  //public string Name { get; set; }
  //#endregion

  //#region FIELDS
  //[SerializeField] private Rigidbody2D rb;

  //private NetInputController input;

  //public bool isGrounded = true;
  //#endregion

  #region UNITY
  void Start() {
    rb = GetComponent<Rigidbody2D>();
    if (rb == null) {
      throw new System.Exception($"ERROR: {name} is missing a Rigidbody2D component!");
    }

    input = GetComponent<NetInputController>();
    if (input == null) {
      throw new System.Exception($"ERROR: {name} is missing a NetInputController script!");
    }

    moveSpeed = 8f;
    jumpSpeed = 0f;
    minJumpSpeed = 10f;
    maxJumpSpeed = 20f;
  }

  void Update() {
    if (IsServer) {
      Jump();
      Move();
    }

    if (IsClient) {
      // animations
    }

    if (IsLocalPlayer) {
      Vector3 offset = new Vector3(0, 2, -10);
      Camera.main.transform.position = transform.position + offset;
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

      Camera.main.orthographic = true;
      Camera.main.orthographicSize = 4.5f;
    }
  }

  public override void HandleMessage(string flag, string value) {
    //if (flag == "GROUNDED") {
    //  if (IsServer) {
    //    isGrounded = bool.Parse(value);
    //  }
    //}

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
      //isGrounded = IsGrounded();
      //SendUpdate("CAN_JUMP", isGrounded.ToString());

      //if (isGrounded) {
      //  rb.velocity = new Vector2(rb.velocity.x, 0);
      //  Move();

      //  if (input.IsJumping) {
      //    Jump();
      //  }
      //}

      //Jump();
      //Move();

      if (IsDirty) {
        SendUpdate("NAME", Name);
        SendUpdate("JUMP_SPEED", jumpSpeed.ToString());
        //SendUpdate("GROUNDED", IsGrounded.ToString());
        IsDirty = false;
      }

      yield return new WaitForSeconds(0.1f);
    }
  }
  #endregion

  #region M_MOVEMENT
  private void Move() {
    //Vector2 targetVelocity = new Vector2(input.Direction * MOVE_SPEED, rb.velocity.y);
    //rb.velocity = targetVelocity;

    if (IsGrounded && jumpSpeed == 0) {
      rb.velocity = new(VelocityX, rb.velocity.y);
    }
  }

  private void Jump() {
    //Vector2 jumpForce = new Vector2(0, JUMP_SPEED);
    //rb.AddForce(jumpForce);

    //isGrounded = false;
    //SendUpdate("GROUNDED", isGrounded.ToString());

    if (IsJumping && IsGrounded) {
      jumpSpeed += 0.5f;
      SendUpdate("JUMP_SPEED", jumpSpeed.ToString());
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

  //private bool IsGrounded() {
  //  bool isGrounded = false;

  //  if (rb.velocity.y <= 0) {
  //    LayerMask walkableLayer = LayerMask.GetMask("Walkable");
  //    Vector2 origin = transform.position;
  //    Vector2 direction = Vector2.down;
  //    float distance = 0.22f;

  //    RaycastHit2D centerHit = Physics2D.Raycast(origin, direction, distance, walkableLayer);
  //    RaycastHit2D leftHit = Physics2D.Raycast(origin + new Vector2(-0.3f, 0), direction, distance, walkableLayer);
  //    RaycastHit2D rightHit = Physics2D.Raycast(origin + new Vector2(0.3f, 0), direction, distance, walkableLayer);

  //    if (centerHit.collider != null || leftHit.collider != null || rightHit.collider != null) {
  //      isGrounded = true;
  //    }
  //  }

  //  return isGrounded;
  //}
  #endregion
}
