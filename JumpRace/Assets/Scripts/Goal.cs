using System.Collections;
using UnityEngine;
using NETWORK_ENGINE;

public class Goal : NetworkComponent {

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
        Debug.Log("Player entered goal box");
        int playerNetId = collision.gameObject.GetComponent<NetworkID>().NetId;
        MyCore.NetDestroyObject(playerNetId);

        // add player name to ranking list
      }
      if (IsClient) {
        // play sfx and vfx
      }
    }
  }
}
