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

  private void OnTriggerEnter(Collider other) {
    Debug.Log(other.gameObject);
    if (other.gameObject.tag == "Player") {
      if (IsServer) {
        Debug.Log("Player entered goal box");
        int playerNetId = other.gameObject.GetComponent<NetworkID>().NetId;
        MyCore.NetDestroyObject(playerNetId);

        // add player name to ranking list
      }
      if (IsClient) {
        // play sfx and vfx
      }
    }
  }
}
