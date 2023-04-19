using System.Collections;
using UnityEngine;
using NETWORK_ENGINE;
using TMPro;

public class Score : NetworkComponent {
  public TMP_Text playerNameText;
  public TMP_Text playerScoreText;

  public string playerName;
  public int playerScore;
  public string PlayerName { get; set; } = "";
  public int PlayerScore { get; set; } = 0;

  void Update() {
    playerName = PlayerName;
    playerScore = PlayerScore;
  }

  public override void HandleMessage(string flag, string value) {
    if (flag == "SCORE_NAME") {
      PlayerName = value;

      if (IsServer) {
        SendUpdate(flag, value);
      }

      if (IsClient) {
        playerNameText.text = value;
      }
    }

    if (flag == "SCORE_VALUE") {
      PlayerScore = int.Parse(value);

      if (IsServer) {
        SendUpdate(flag, value);
      }

      if (IsClient) {
        playerScoreText.text = value;
      }
    }
  }

  public override void NetworkedStart() {
    try {
      Transform scoreHolderTransform = GameObject.FindGameObjectWithTag("ScoreHolder").transform;
      transform.SetParent(scoreHolderTransform, false);
    }
    catch {
      Debug.Log("ERROR: Object with tag \"ScoreHolder\" not found!");
    }
  }

  public override IEnumerator SlowUpdate() {
    while (IsConnected) {
      if (IsServer) {
        if (IsDirty) {
          SendUpdate("SCORE_NAME", PlayerName);
          SendUpdate("SCORE_VALUE", PlayerScore.ToString());
          IsDirty = false;
        }
      }
      yield return new WaitForSeconds(0.1f);
    }
  }
}
