using System.Collections;
using UnityEngine;
using NETWORK_ENGINE;

public class Goal : NetworkComponent {
  #region NETWORK_ENGINE

  public override void NetworkedStart() { }

  public override void HandleMessage(string flag, string value) { }

  public override IEnumerator SlowUpdate() {
    yield return new WaitForSeconds(0.1f);
  }

  #endregion

  private void OnTriggerEnter2D(Collider2D collision) {
    if (collision.gameObject.tag == "Player") {
      if (IsServer) {
        GameObject player = collision.gameObject;
        int playerNetId = player.GetComponent<NetworkID>().NetId;
        int playerOwner = player.GetComponent<NetworkID>().Owner;

        MyCore.NetDestroyObject(playerNetId);

        NetPlayerManager[] netPlayerManagers = GameObject.FindObjectsOfType<NetPlayerManager>();
        foreach (NetPlayerManager npm in netPlayerManagers) {
          if (npm.Owner == playerOwner) {
            npm.HasReachedGoal = true;
          }
        }
      }

      if (IsClient) {
        // play sfx/vfx
      }
    }
  }
}
