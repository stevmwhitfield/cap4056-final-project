using System.Collections;
using UnityEngine;
using NETWORK_ENGINE;

public class Goal : NetworkComponent {
  public AudioClip audioClip;

  #region NETWORK_ENGINE

  public override void NetworkedStart() { }

  public override void HandleMessage(string flag, string value) { }

  public override IEnumerator SlowUpdate() {
    yield return new WaitForSeconds(0.1f);
  }

  #endregion

  private void OnTriggerEnter2D(Collider2D collision) {
    if (collision.gameObject.tag == "Player") {
      GameObject player = collision.gameObject;

      if (IsClient) {
        player.GetComponent<AudioSource>().PlayOneShot(audioClip);
      }

      if (IsServer) {
        int playerNetId = player.GetComponent<NetworkID>().NetId;
        int playerOwner = player.GetComponent<NetworkID>().Owner;

        MyCore.NetDestroyObject(playerNetId);

        NetPlayerManager[] netPlayerManagers = GameObject.FindObjectsOfType<NetPlayerManager>();
        foreach (NetPlayerManager npm in netPlayerManagers) {
          if (npm.Owner == playerOwner) {
            npm.HasReachedGoal = true;
            npm.CalculateScore();
          }
        }
      }
    }
  }
}
