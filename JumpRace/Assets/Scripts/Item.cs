using System.Collections;
using UnityEngine;
using NETWORK_ENGINE;

public class Item : NetworkComponent {
  #region UNITY

  void Start() { }

  void Update() { }

  #endregion

  #region NETWORK_ENGINE

  public override void NetworkedStart() { }

  public override void HandleMessage(string flag, string value) { }

  public override IEnumerator SlowUpdate() {
    yield return new WaitForSeconds(0.1f);
  }

  #endregion

  private void OnTriggerEnter2D(Collider2D collision) {
    Debug.Log(collision.gameObject);
    if (collision.gameObject.tag == "Player") {
      if (IsServer) {
        Debug.Log("Player collided with item");
        MyCore.NetDestroyObject(MyId.NetId);

        // add score to player
      }
      if (IsClient) {
        // play sfx and vfx
      }
    }
  }
}
