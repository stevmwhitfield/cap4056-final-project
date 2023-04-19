using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NETWORK_ENGINE;

public class NetPlayerManager : NetworkComponent {
  #region FIELDS

  public TMP_InputField NameInput;
  public Button IsReadyButton;
  public TMP_Text NameText;
  public TMP_Text IsReadyText;
  public Image PlaceholderSprite;

  #endregion

  #region PROPERTIES

  public string Name { get; set; } = "";
  public int TimeToGoal { get; set; } = 300;
  public int Score => TimeToGoal + CoinsCollected * 20;
  public int CoinsCollected { get; set; } = 0;
  public bool IsReady { get; set; } = false;
  public bool HasReachedGoal { get; set; } = false;

  #endregion

  #region NETWORK_ENGINE
  public override void NetworkedStart() {
    if (!IsLocalPlayer) {
      NameInput.gameObject.SetActive(false);
      IsReadyButton.gameObject.SetActive(false);
    }

    try {
      Transform playerHolderTransform = GameObject.FindGameObjectWithTag("PlayerHolder").transform;
      transform.SetParent(playerHolderTransform, false);
    }
    catch {
      Debug.Log("ERROR: Object with tag \"PlayerHolder\" not found!");
    }
  }

  public override void HandleMessage(string flag, string value) {
    if (flag == "NAME") {
      Name = value;

      if (IsServer) {
        SendUpdate(flag, value);
      }

      if (IsClient) {
        NameText.text = value;
      }
    }

    if (flag == "READY") {
      IsReady = bool.Parse(value);

      if (IsServer) {
        SendUpdate(flag, value);
      }

      if (IsReady) {
        if (IsClient) {
          IsReadyText.text = "Ready";
          IsReadyText.color = new Color32(57, 255, 20, 255);
        }

        if (IsLocalPlayer) {
          NameInput.gameObject.SetActive(false);
          IsReadyButton.gameObject.SetActive(false);
        }
      }
    }

  }

  public override IEnumerator SlowUpdate() {
    while (IsConnected) {
      if (IsServer) {
        if (IsDirty) {
          SendUpdate("NAME", Name);
          SendUpdate("READY", IsReady.ToString());
          IsDirty = false;
        }
      }
      if (IsLocalPlayer) {
        if (Name == "") {
          IsReadyButton.interactable = false;
        }
        else {
          IsReadyButton.interactable = true;
        }
      }
      yield return new WaitForSeconds(0.1f);
    }
  }
  #endregion

  #region UNITY
  void Start() { }

  void Update() { }
  #endregion

  #region M_UI
  public void SetName(string name) {
    if (IsLocalPlayer) {
      SendCommand("NAME", name);
    }
  }

  public void SetIsReady() {
    if (IsLocalPlayer) {
      if (!IsReady) {
        SendCommand("READY", true.ToString());
      }
    }
  }
  #endregion

  public IEnumerator DecreaseTimeToGoal() {
    while (TimeToGoal > 0 && !HasReachedGoal) {
      yield return new WaitForSecondsRealtime(1f);
      TimeToGoal -= 1;
      Debug.Log($"{Name}: {Score}");
    }
  }
}
