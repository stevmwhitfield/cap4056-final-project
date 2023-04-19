using TMPro;
using UnityEngine;
using NETWORK_ENGINE;
using System.Collections;

public class LevelTime : NetworkComponent {
  public TMP_Text TimeText;

  private GameMaster gm;
  private int time;

  public override void NetworkedStart() { }

  public override void HandleMessage(string flag, string value) {
    if (flag == "TIME") {
      time = int.Parse(value);

      if (IsClient) {
        TimeText.text = time.ToString();
      }
    }
  }



  public override IEnumerator SlowUpdate() {
    while (IsServer) {
      if (IsDirty) {
        SendUpdate("TIME", time.ToString());
        IsDirty = false;
      }
      yield return new WaitForSeconds(0.1f);
    }
  }

  void Start() {
    gm = GameObject.FindObjectOfType<GameMaster>();
  }

  void Update() {
    if (IsServer) {
      time = gm.GameTime;
      SendUpdate("TIME", time.ToString());
    }
  }
}
